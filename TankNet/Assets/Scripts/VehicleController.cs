using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class VehicleController : MonoBehaviourPun, IPunObservable
{
    public Text lmaoText;
    public int lifePoints = 2;
    public float sensivity = 1.5f;
    public float depression;
    public float elevation;
    public GameObject gun;
    public Transform target; // this is a far object that the gun, turret and cameras constantly look at
    public GameObject vehicle;
    public Transform turret;
    public Camera Commandercam;
    public Camera GunnerCam;
    public Transform MainCam;
    public SpriteRenderer reticle;

    // all the wheels that are controlled
    public WheelCollider frontleft, frontright;
    public WheelCollider middleleft1, middleright1;
    public WheelCollider middleleft2, middleright2;
    public WheelCollider rearleft, rearright;
    public Transform frontleftT, frontrightT;
    public Transform middleleft1T, middleright1T;
    public Transform middleleft2T, middleright2T;
    public Transform rearleftT, rearrightT;
    public float maxSteerAngle = 30;
    public float motorForce = 300;

    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steeringAngle;
    private bool justShot; // say if the tank shot
    private float fovGunner; // used to zoom the gun camera
    private float fovLevel; // to know the zooming level
    private float timeVar; // to countdown after a shot
    private Animator anim;
    private GameObject throwIt;
    private bool isLightActiveBoi;
    private bool isDestroyed = false;
    private float destroyTime = 0;
    private Rigidbody vehicleRIGI;
    private float drownCounter = 10;

    Vector3 latestPos;
    Quaternion latestRot;


    void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.visible = false;
            anim = gun.GetComponent<Animator>();
            timeVar = 8.5f;
            fovGunner = 18;
            fovLevel = 0;
            Commandercam.GetComponent<Camera>().enabled = true;
            GunnerCam.GetComponent<Camera>().enabled = false;
            justShot = false;
            vehicleRIGI = vehicle.GetComponent<Rigidbody>();
        }
        else
        {
            Commandercam.enabled = false;
            GunnerCam.enabled = false;
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot = (Quaternion)stream.ReceiveNext();
        }
    }


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
                    lmaoText.text = "You will drown in " + drownCounter.ToString("0");
                    if (drownCounter < 0)
                        Die();
                }
                else
                {
                    if (lmaoText.text != "")
                        lmaoText.text = "";
                    if (drownCounter < 10)
                        drownCounter = 10;
                }

                // make the target look at the cursor
                float X = MainCam.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensivity;
                float Y = MainCam.localEulerAngles.y + Input.GetAxis("Mouse X") * sensivity;
                MainCam.localEulerAngles = new Vector3(X, Y, 0);

                // turn the turret towards the target
                turret.LookAt(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));
                float newY = turret.localEulerAngles.y;
                turret.localEulerAngles = new Vector3(0, newY, 0);

                // turn the gun towards the target and determine if we shot
                gun.transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));
                float newX = gun.transform.localEulerAngles.x;
                if (newX > depression && newX < 180)
                    newX = depression;
                if (newX < 360 - elevation && newX >= 180)
                    newX = 360 - elevation;
                gun.transform.localEulerAngles = new Vector3(newX, 0, 0);

                // countdown to reload
                if (timeVar < 8.5f)
                    timeVar = timeVar + Time.deltaTime;

                // do the necessary things after shooting
                if (Input.GetKeyDown("mouse 0") && timeVar >= 8.5f)
                {
                    timeVar = 0;
                    justShot = true;
                }

                // set the cameras to follow the target
                Commandercam.transform.LookAt(target);
                GunnerCam.transform.rotation = gun.transform.rotation;


                // zoom in
                if (Input.GetAxis("Mouse ScrollWheel") > 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel < 4)
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
                    reticle.GetComponent<SpriteRenderer>().enabled = true;
                    GunnerCam.GetComponent<Camera>().enabled = true;
                    fovLevel = 1;
                }

                // go to commander camera
                if (Input.GetAxis("Mouse ScrollWheel") < 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel == 1)
                {
                    reticle.GetComponent<SpriteRenderer>().enabled = false;
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
            }
            else
            {
                destroyTime = destroyTime + Time.deltaTime;
                if (destroyTime > 6)
                    Destroy(gameObject, 0);
            }
        }
        else
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 50);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, latestRot, Time.deltaTime * 50);
        }
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (!isDestroyed)
            {
                // give recoil to the tank and throw projectile
                if (justShot)
                {
                    anim.Play("shoot");
                    throwIt = PhotonNetwork.Instantiate("Projectile", new Vector3(gun.transform.position.x, gun.transform.position.y, gun.transform.position.z + 3.5f), Quaternion.Euler(MainCam.transform.localEulerAngles.x - 90, MainCam.transform.localEulerAngles.y, MainCam.transform.localEulerAngles.z));
                    throwIt.GetComponent<Rigidbody>().AddForce(gun.transform.forward * 25000, ForceMode.Impulse);
                    double radturretY = turret.localEulerAngles.y * 0.0174533;
                    vehicleRIGI.AddForce(new Vector3((float)Math.Sin(radturretY) * -480000, 0, (float)Math.Cos(radturretY) * -480000));
                    justShot = false;
                }

                // manage the movements of the tank
                GetInput();
                Steer();
                UpdateWheelPoses();
                if (Input.GetKey("space"))
                {
                    frontleft.brakeTorque = 5000;
                    frontright.brakeTorque = 5000;
                    middleleft1.brakeTorque = 5000;
                    middleright1.brakeTorque = 5000;
                    middleleft2.brakeTorque = 5000;
                    middleright2.brakeTorque = 5000;
                    rearleft.brakeTorque = 5000;
                    rearright.brakeTorque = 5000;
                }
                else
                {
                    frontleft.brakeTorque = 0;
                    frontright.brakeTorque = 0;
                    middleleft1.brakeTorque = 0;
                    middleright1.brakeTorque = 0;
                    middleleft2.brakeTorque = 0;
                    middleright2.brakeTorque = 0;
                    rearleft.brakeTorque = 0;
                    rearright.brakeTorque = 0;
                    Accelerate();
                }
            }
            else
            {
                frontleft.brakeTorque = 5000;
                frontright.brakeTorque = 5000;
                middleleft1.brakeTorque = 5000;
                middleright1.brakeTorque = 5000;
                middleleft2.brakeTorque = 5000;
                middleright2.brakeTorque = 5000;
                rearleft.brakeTorque = 5000;
                rearright.brakeTorque = 5000;
            }
        }
    }


    public void GetInput()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");
    }


    private void Steer()
    {
        m_steeringAngle = maxSteerAngle * m_horizontalInput;
        frontleft.steerAngle = m_steeringAngle;
        frontright.steerAngle = m_steeringAngle;
        middleleft1.steerAngle = m_steeringAngle / 2;
        middleright1.steerAngle = m_steeringAngle / 2;
    }


    private void Accelerate()
    {
        frontleft.motorTorque = m_verticalInput * motorForce;
        frontright.motorTorque = m_verticalInput * motorForce;
        middleleft1.motorTorque = m_verticalInput * motorForce;
        middleright1.motorTorque = m_verticalInput * motorForce;
        middleleft2.motorTorque = m_verticalInput * motorForce;
        middleright2.motorTorque = m_verticalInput * motorForce;
        rearleft.motorTorque = m_verticalInput * motorForce;
        rearright.motorTorque = m_verticalInput * motorForce;
    }


    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontleft, frontleftT);
        UpdateWheelPose(frontright, frontrightT);
        UpdateWheelPose(middleleft1, middleleft1T);
        UpdateWheelPose(middleright1, middleright1T);
        UpdateWheelPose(middleleft2, middleleft2T);
        UpdateWheelPose(middleright2, middleright2T);
        UpdateWheelPose(rearleft, rearleftT);
        UpdateWheelPose(rearright, rearrightT);
    }


    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
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
        {
            lmaoText.text = "Vehicle destroyed";
            isDestroyed = true;
        }
    }
}