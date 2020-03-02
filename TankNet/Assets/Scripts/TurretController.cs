using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class TurretController : MonoBehaviourPun
{
    public int lifePoints = 5;
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
    public AudioClip[] shellsClips;
    public AudioSource[] shellSounds;
    public AudioSource ReloadSound;
    public GameObject Shell;
    public AudioListener MainAudioListen;
    public GameObject MuzzleFlash;
    public GameObject GunSound;
    public Canvas SightCanvas;
    public RectTransform CanvasSize;
    public Text DestroyText;
    public Text reloadingTxt;
    public Light lightL;
    public Light lightR;
    public Canvas PauseUI;

    private AudioReverbZone normalReverb;
    private AudioReverbZone scopeReverb;
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
    private bool isLocked = false;
    private bool isPaused = false;
    private VehicleController vehicleControl;
    private InputManager im;

    Vector3 latestPos;
    Quaternion latestRot;


    // Update is called once per frame
    void Start()
    {
        lightL.enabled = false;
        lightR.enabled = false;
        SightCanvas.enabled = false;
        PauseUI.enabled = false;

        if (photonView.IsMine)
        {
            vehicleControl = gameObject.GetComponent<VehicleController>();
            im = InputManager.instance;
            normalReverb = GameObject.Find("ReverbSoundNormal").GetComponent<AudioReverbZone>();
            scopeReverb = GameObject.Find("ReverbSoundScope").GetComponent<AudioReverbZone>();
            scopeReverb.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            magNb = 10;
            timeBetweenShots = 0.2f;
            ReloadTime = 4;
            fovGunner = GunnerCam.fieldOfView;
            fovLevel = 0;
            shot = false;
            Commandercam.GetComponent<Camera>().enabled = true;
            GunnerCam.GetComponent<Camera>().enabled = false;
            reload = false;
            vehicleRIGI = vehicle.GetComponent<Rigidbody>();
        }
        else
        {
            DestroyText.enabled = false;
            reloadingTxt.enabled = false;
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
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(Cursor.lockState == CursorLockMode.Locked)
                {
                    isPaused = true;
                    vehicleControl.destroyed = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    PauseUI.enabled = true;
                }
                else
                {
                    isPaused = false;
                    vehicleControl.destroyed = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    PauseUI.enabled = false;
                }
            }

            if (im.GetKey(KeybindingActions.lock_turret))
                isLocked = true;
            else
                isLocked = false;

            if(im.GetKeyDown(KeybindingActions.light))
            {
                lightL.enabled = !lightL.enabled;
                lightR.enabled = !lightR.enabled;
                photonView.RPC("SendLightStatus", RpcTarget.Others, lightL.enabled);
            }

            // handle the drowning of the vehicle
            if (!isDestroyed && !isPaused)
            {
                if (vehicle.transform.position.y < 14.3f)
                {
                    drownCounter = drownCounter - Time.deltaTime;
                    DestroyText.text = "You will sink in " + (int)drownCounter + " seconds";
                    if (drownCounter < 0)
                        Die();
                }
                else
                {
                    if (drownCounter < 10)
                        drownCounter = 10;
                    if (DestroyText.text != "")
                        DestroyText.text = "";
                }
                
                // cam transforms
                float X = targetparent.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensivity;
                float Y = targetparent.localEulerAngles.y + Input.GetAxis("Mouse X") * sensivity;
                targetparent.localEulerAngles = new Vector3(X, Y, 0);
                Commandercam.transform.eulerAngles = new Vector3(targetparent.localEulerAngles.x, targetparent.eulerAngles.y + 180, targetparent.localEulerAngles.z);

                if (!isLocked)
                {
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
                        CanvasSize.localScale = new Vector3(CanvasSize.localScale.x * 2, CanvasSize.localScale.y * 2, CanvasSize.localScale.z * 2);
                        fovGunner = fovGunner / 2;
                        fovLevel = fovLevel + 1;
                        sensivity = sensivity / 2;
                    }

                    // go to gun camera
                    if (Input.GetAxis("Mouse ScrollWheel") > 0 && Commandercam.GetComponent<Camera>().enabled)
                    {
                        Commandercam.GetComponent<Camera>().enabled = false;
                        GunnerCam.GetComponent<Camera>().enabled = true;
                        scopeReverb.enabled = true;
                        normalReverb.enabled = false;
                        SightCanvas.enabled = true;
                        fovLevel = 1;
                    }

                    // go to commander camera
                    if (Input.GetAxis("Mouse ScrollWheel") < 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel == 1)
                    {
                        Commandercam.GetComponent<Camera>().enabled = true;
                        GunnerCam.GetComponent<Camera>().enabled = false;
                        scopeReverb.enabled = false;
                        normalReverb.enabled = true;
                        SightCanvas.enabled = false;
                        fovLevel = 0;
                    }

                    // zoom out
                    if (Input.GetAxis("Mouse ScrollWheel") < 0 && GunnerCam.GetComponent<Camera>().enabled && fovLevel > 1)
                    {
                        GunnerCam.fieldOfView = fovGunner * 2;
                        CanvasSize.localScale = new Vector3(CanvasSize.localScale.x / 2, CanvasSize.localScale.y / 2, CanvasSize.localScale.z / 2);
                        fovGunner = fovGunner * 2;
                        fovLevel = fovLevel - 1;
                        sensivity = sensivity * 2;
                    }
                }

                // shooting system
                if(im.GetKeyDown(KeybindingActions.reload) && !reload && magNb != 10)
                {
                    magNb = 10;
                    ReloadTime = 4;
                    reload = true;
                    ReloadSound.Play();
                    Debug.Log("Force Reloading");
                }

                if (ReloadTime > 0 && reload == true)
                {
                    ReloadTime -= Time.deltaTime;
                    reloadingTxt.text = "Reloading : " + ReloadTime.ToString("F1");
                }
                else if (reload == true)
                {
                    reload = false;
                    reloadingTxt.text = "Reloading : 4";
                }

                if (timeBetweenShots > 0)
                    timeBetweenShots -= Time.deltaTime;

                if (im.GetKey(KeybindingActions.shoot) && timeBetweenShots <= 0 && !reload)
                {
                    magNb--;
                    timeBetweenShots = 0.2f;
                    shot = true;
                    if (magNb == 0)
                    {
                        magNb = 10;
                        ReloadTime = 4;
                        reload = true;
                        ReloadSound.Play();
                        Debug.Log("Reloading");
                    }
                }
            }
            else if(isDestroyed)
            {
                destroyTime = destroyTime + Time.deltaTime;
                if (destroyTime > 5)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    PhotonNetwork.Destroy(photonView);
                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.LoadLevel("GameLobby");
                }
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
                AudioClip i = shellsClips[Random.Range(0, shellsClips.Length - 1)];
                for (int u = 0; u < shellSounds.Length; u++)
                {
                    if (!shellSounds[u].isPlaying)
                    {
                        shellSounds[u].clip = i;
                        shellSounds[u].Play();
                        break;
                    }
                }
                photonView.RPC("Shooting", RpcTarget.All, ShellFireTrans.position, ShellFireTrans.rotation);
                shot = false;
            }
        }
    }


    [PunRPC]
    void Shooting(Vector3 pos, Quaternion rot)
    {
        GameObject flash = Instantiate(MuzzleFlash, pos, rot);
        GameObject sound = Instantiate(GunSound, pos, rot);
        sound.GetComponent<AudioSource>().pitch = sound.GetComponent<AudioSource>().pitch - (Vector3.Distance(Commandercam.transform.position, pos)/100) + Random.Range(-0.1f, 0);
        flash.transform.SetParent(gun.transform);
        sound.transform.SetParent(gun.transform);
        GameObject throwIt = Instantiate(Shell, pos, rot * Quaternion.Euler(90, 0, 0));
        throwIt.GetComponent<Rigidbody>().AddForce(ShellFireTrans.forward * -2200, ForceMode.Impulse);
    }


    [PunRPC]
    void SendLightStatus(bool b)
    {
        lightL.enabled = b;
        lightR.enabled = b;
    }
    

    void OnCollisionEnter(Collision col)
    {
        if (photonView.IsMine && col.transform.tag == "Shell")
        {
            Debug.Log("Vehicle with view " + photonView.GetInstanceID() + " was hit");
            lifePoints -= 1;
            if (lifePoints <= 0)
                Die();
        }
    }


    void Die()
    {
        if (photonView.IsMine)
        {
            isDestroyed = true;
            DestroyText.text = "Vehicle destroyed";
            vehicleControl.destroyed = true;
        }
    }
}