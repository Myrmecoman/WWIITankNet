using UnityEngine;
using Photon.Pun;

public class TurretController : MonoBehaviourPun
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
    public Transform ShellFireTrans;
    public GameObject Shell;
    public AudioListener MainAudioListen;
    public GameObject MuzzleFlash;

    private bool reload; // say if the tank shot
    private float ReloadTime; // to countdown after a shot
    private int magNb;
    private float timeBetweenShots;
    private bool shot;
    private float fovGunner; // used to zoom the gun camera
    private float fovLevel; // to know the zooming level
    private bool isDestroyed = false;
    private float destroyTime = 0;
    private Rigidbody vehicleRIGI;
    private float drownCounter = 10;

    Vector3 latestPos;
    Quaternion latestRot;


    // Update is called once per frame
    void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.visible = false;
            magNb = 10;
            timeBetweenShots = 0.2f;
            ReloadTime = 4;
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
            MainAudioListen.enabled = false;
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
                if (ReloadTime > 0 && reload == true)
                    ReloadTime -= Time.deltaTime;
                else if (reload == true)
                    reload = false;

                if (timeBetweenShots > 0)
                    timeBetweenShots -= Time.deltaTime;
                
                if (Input.GetKey("mouse 0") && timeBetweenShots <= 0 && !reload)
                {
                    magNb--;
                    timeBetweenShots = 0.2f;
                    shot = true;
                    if (magNb == 0)
                    {
                        magNb = 10;
                        ReloadTime = 4;
                        reload = true;
                        Debug.Log("Reloading");
                    }
                }
            }
            else
            {
                destroyTime = destroyTime + Time.deltaTime;
                if (destroyTime > 6)
                    PhotonNetwork.Destroy(photonView);
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

    
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            // give recoil to the tank and throw projectile
            if (shot)
            {
                gunAnim.Play("shoot");
                photonView.RPC("Shooting", RpcTarget.All, ShellFireTrans.position, ShellFireTrans.rotation);
                shot = false;
            }
        }
    }


    [PunRPC]
    void Shooting(Vector3 pos, Quaternion rot)
    {
        GameObject flash = Instantiate(MuzzleFlash, pos, rot);
        GameObject throwIt = Instantiate(Shell, pos, rot);
        throwIt.GetComponent<Rigidbody>().AddForce(ShellFireTrans.forward * -25000, ForceMode.Impulse);
    }


    void OnCollisionEnter(Collision coll)
    {
        if (photonView.IsMine && coll.gameObject.tag == "Shell")
        {
            Debug.Log("Vehicle with view " + photonView.GetInstanceID() + " was hit");
            lifePoints = lifePoints - 1;
            if (lifePoints <= 0)
                Die();
        }
    }


    void Die()
    {
        if (photonView.IsMine)
        {
            isDestroyed = true;
            GetComponent<VehicleController>().enabled = false;
        }
    }
}