using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TurretController : MonoBehaviourPun, IPunObservable
{
    public int lifePoints = 2;
    public float sensivity = 1.5f;
    public float depression;
    public float elevation;
    public GameObject gun;
    public Animator gunAnim;
    public Transform targetparent;
    public Transform target; // this is a far object that the gun, turret and cameras constantly look at
    public GameObject vehicle;
    public Transform turret;
    public Camera Commandercam;
    public Camera GunnerCam;

    private bool reload; // say if the tank shot
    private float timeVar; // to countdown after a shot
    private int mag;
    private float timeMag;
    private bool shot;
    private float fovGunner; // used to zoom the gun camera
    private float fovLevel; // to know the zooming level
    private GameObject throwIt;
    private bool isDestroyed = false;
    private float destroyTime = 0;
    private Rigidbody vehicleRIGI;
    private float drownCounter = 10;

    Vector3 latestPos;
    Quaternion latestRot;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!stream.IsWriting)
        {
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot = (Quaternion)stream.ReceiveNext();
        }
    }


    // Update is called once per frame
    void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.visible = false;
            mag = 10;
            timeMag = 0.19f;
            timeVar = 4;
            fovGunner = 25;
            fovLevel = 0;
            shot = false;
            Commandercam.GetComponent<Camera>().enabled = true;
            GunnerCam.GetComponent<Camera>().enabled = false;
            reload = false;
            vehicleRIGI = vehicle.GetComponent<Rigidbody>();
        }
        else
        {
            Commandercam.enabled = false;
            GunnerCam.enabled = false;
        }
    }


    // Start is called before the first frame update
    void Update()
    {
        if (photonView.IsMine)
        {
            // handle the drowning of the vehicle
            if (!isDestroyed)
            {
                if (vehicle.transform.position.y < -1)
                {
                    drownCounter = drownCounter - Time.deltaTime;
                    if (drownCounter < 0)
                        Die();
                }
                else
                {
                    if (drownCounter < 10)
                        drownCounter = 10;
                }

                // cam transforms
                float X = targetparent.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensivity;
                float Y = targetparent.localEulerAngles.y + Input.GetAxis("Mouse X") * sensivity;
                targetparent.localEulerAngles = new Vector3(X, Y, 0);
                Commandercam.transform.eulerAngles = new Vector3(targetparent.localEulerAngles.x, targetparent.eulerAngles.y + 180, targetparent.localEulerAngles.z);

                // turret rotation
                RotateTurret(target, 22);

                // gun rotation
                Quaternion cons = gun.transform.rotation;
                gun.transform.localEulerAngles = new Vector3(-Commandercam.transform.localEulerAngles.x, 0, 0);
                float newX;
                if (gun.transform.localEulerAngles.x >= 0 && gun.transform.localEulerAngles.x < 180)
                    newX = Mathf.Clamp(gun.transform.localEulerAngles.x, 0, 89);
                else
                    newX = Mathf.Clamp(gun.transform.localEulerAngles.x, 354.5f, 360);
                gun.transform.localEulerAngles = new Vector3(newX, 0, 0);
                gun.transform.rotation = Quaternion.RotateTowards(cons, gun.transform.rotation, 22 * Time.smoothDeltaTime);
                
                // zoom in
                if (Input.GetAxis("Mouse ScrollWheel") > 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel < 2)
                {
                    GunnerCam.fieldOfView = fovGunner / 2;
                    fovGunner = fovGunner / 2;
                    fovLevel = fovLevel + 1;
                    sensivity = sensivity / 2;
                }

                // go to gun camera
                if (Input.GetAxis("Mouse ScrollWheel") > 0 && Commandercam.GetComponent<Camera>().enabled)
                {
                    Commandercam.GetComponent<Camera>().enabled = false;
                    GunnerCam.GetComponent<Camera>().enabled = true;
                    fovLevel = 1;
                }

                // go to commander camera
                if (Input.GetAxis("Mouse ScrollWheel") < 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel == 1)
                {
                    Commandercam.GetComponent<Camera>().enabled = true;
                    GunnerCam.GetComponent<Camera>().enabled = false;
                    fovLevel = 0;
                }

                // zoom out
                if (Input.GetAxis("Mouse ScrollWheel") < 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel > 1)
                {
                    GunnerCam.fieldOfView = fovGunner * 2;
                    fovGunner = fovGunner * 2;
                    fovLevel = fovLevel - 1;
                    sensivity = sensivity * 2;
                }

                // shooting system
                if (timeVar < 4 && reload == true)
                    timeVar += Time.deltaTime;
                else if (reload == true)
                    reload = false;

                if (timeMag < 0.19f)
                    timeMag += Time.deltaTime;
                
                if (Input.GetKey("mouse 0") && timeMag >= 0.19f && reload == false)
                {
                    if (mag == 0)
                    {
                        timeVar = 0;
                        reload = true;
                    }
                    else
                    {
                        timeMag = 0;
                        shot = true;
                    }
                    mag--;
                }
            }
            else
            {
                destroyTime = destroyTime + Time.deltaTime;
                if (destroyTime > 6)
                    Destroy(gameObject, 0);
            }
        }
    }


    void RotateTurret(Transform target, float rotationSpeed)
    {
        Vector3 dir = (target.position - turret.position).normalized;
        Quaternion newRot = Quaternion.LookRotation(dir, turret.up);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, newRot, rotationSpeed * Time.smoothDeltaTime);
        turret.localEulerAngles = new Vector3(0f, turret.localEulerAngles.y, 0f);
    }

    
    private void FixedUpdate()
    {
        // give recoil to the tank and throw projectile
        if (shot)
        {
            gunAnim.Play("shoot");
            //throwIt = PhotonNetwork.Instantiate("Projectile", new Vector3(gun.transform.position.x, gun.transform.position.y, gun.transform.position.z + 3.5f), gun.transform.rotation);
            //throwIt.GetComponent<Rigidbody>().AddForce(gun.transform.forward * 25000, ForceMode.Impulse);
            double radturretY = turret.localEulerAngles.y * 0.0174533;
            vehicleRIGI.AddForce(new Vector3((float)System.Math.Sin(radturretY) * 500000, 0, (float)System.Math.Cos(radturretY) * 500000));
            shot = false;
        }
    }


    public void HitVehicle(int damage)
    {
        if (photonView.IsMine)
        {
            lifePoints = lifePoints - damage;
            if (lifePoints <= 0)
                Die();
        }
    }


    private void Die()
    {
        if (photonView.IsMine)
            isDestroyed = true;
    }
}