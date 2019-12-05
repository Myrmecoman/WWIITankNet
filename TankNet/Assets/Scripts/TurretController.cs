﻿using System.Collections;
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
    public Transform target; // this is a far object that the gun, turret and cameras constantly look at
    public GameObject vehicle;
    public Transform turret;
    public Camera Commandercam;
    public Camera GunnerCam;

    private bool justShot; // say if the tank shot
    private float fovGunner; // used to zoom the gun camera
    private float fovLevel; // to know the zooming level
    private float timeVar; // to countdown after a shot
    private Animator anim;
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
            anim = gun.GetComponent<Animator>();
            timeVar = 4;
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

                // make the target look at the cursor
                float X = Commandercam.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensivity;
                float Y = Commandercam.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensivity;
                Commandercam.transform.localEulerAngles = new Vector3(X, Y, 0);
                
                // countdown to reload
                if (timeVar < 4)
                    timeVar = timeVar + Time.deltaTime;

                // do the necessary things after shooting
                if (Input.GetKeyDown("mouse 0") && timeVar >= 4)
                {
                    timeVar = 0;
                    justShot = true;
                }


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
            }
            else
            {
                destroyTime = destroyTime + Time.deltaTime;
                if (destroyTime > 6)
                    Destroy(gameObject, 0);
            }
        }
    }
    

    private void FixedUpdate()
    {
        // give recoil to the tank and throw projectile
        if (justShot)
        {
            anim.Play("shoot");
            throwIt = PhotonNetwork.Instantiate("Projectile", new Vector3(gun.transform.position.x, gun.transform.position.y, gun.transform.position.z + 3.5f), gun.transform.rotation);
            throwIt.GetComponent<Rigidbody>().AddForce(gun.transform.forward * 25000, ForceMode.Impulse);
            double radturretY = turret.localEulerAngles.y * 0.0174533;
            vehicleRIGI.AddForce(new Vector3((float)System.Math.Sin(radturretY) * -480000, 0, (float)System.Math.Cos(radturretY) * -480000));
            justShot = false;
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
