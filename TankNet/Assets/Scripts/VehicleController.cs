using UnityEngine;
using Photon.Pun;

public class VehicleController : MonoBehaviourPun
{
    // all the wheels that are controlled
    public WheelCollider frontleft, frontright;
    public WheelCollider rearleft, rearright;
    public Transform frontleftT, frontrightT;
    public Transform rearleftT, rearrightT;
    public float maxSteerAngle = 30;
    public float motorForce = 300;
    public AudioSource audioEngine;
    public AudioClip engineIdle;
    public AudioClip engineRunning1;
    public AudioClip engineRunning2;
    public AudioClip engineRunning3;

    [HideInInspector]
    public bool destroyed = false;

    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steeringAngle;
    private Rigidbody rb;
    private float s;
    private float force;
    private InputManager im;


    private void Start()
    {
        if (photonView.IsMine)
        {
            im = InputManager.instance;
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = new Vector3(rb.centerOfMass.x, rb.centerOfMass.y - 1, rb.centerOfMass.z);
        }
    }


    void Update()
    {
        float sp = rb.velocity.magnitude;
        if (sp < 0.2f)
        {
            if (audioEngine.isPlaying && audioEngine.clip.name != engineIdle.name)
            {
                audioEngine.Stop();
                audioEngine.clip = engineIdle;
                audioEngine.Play();
            }
            audioEngine.pitch = 0.95f;
        }
        else if (sp < 4.2f)
        {
            if (audioEngine.isPlaying && audioEngine.clip.name != engineRunning1.name)
            {
                audioEngine.Stop();
                audioEngine.clip = engineRunning1;
                audioEngine.Play();
            }
            audioEngine.pitch = 0.95f + ((sp - 0.2f) / 20);
        }
        else if (sp < 8.4f)
        {
            if (audioEngine.isPlaying && audioEngine.clip.name != engineRunning2.name)
            {
                audioEngine.Stop();
                audioEngine.clip = engineRunning2;
                audioEngine.Play();
            }
            audioEngine.pitch = 0.95f + ((sp - 4.2f) / 20);
        }
        else
        {
            if (audioEngine.isPlaying && audioEngine.clip.name != engineRunning3.name)
            {
                audioEngine.Stop();
                audioEngine.clip = engineRunning3;
                audioEngine.Play();
            }
            audioEngine.pitch = 0.95f + ((sp - 8.4f) / 20);
        }
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            s = rb.velocity.magnitude;
            //Debug.Log("speed is " + s * 3.6f);

            if(s == 0)
                photonView.RPC("SendBrake", RpcTarget.Others);

            GetInput();
            Steer();
            UpdateWheelPoses();
            if (im.GetKey(KeybindingActions.brake) || destroyed || s >= 16)
            {
                frontleft.brakeTorque = 7000;
                frontright.brakeTorque = 7000;
                rearleft.brakeTorque = 7000;
                rearright.brakeTorque = 7000;
            }
            else
            {
                frontleft.brakeTorque = 0;
                frontright.brakeTorque = 0;
                rearleft.brakeTorque = 0;
                rearright.brakeTorque = 0;
                Accelerate(s);
            }
        }
    }

    [PunRPC]
    void SendBrake()
    {
        frontleft.brakeTorque = 7000;
        frontright.brakeTorque = 7000;
        rearleft.brakeTorque = 7000;
        rearright.brakeTorque = 7000;
    }
    
    public void GetInput()
    {
        if (im.GetKey(KeybindingActions.forward) || im.GetKey(KeybindingActions.backward))
        {
            m_horizontalInput = Input.GetAxis("Horizontal");
            m_verticalInput = -Input.GetAxis("Vertical");
        }
        else
        {
            m_horizontalInput = 0;
            m_verticalInput = 0;
        }
    }
    
    private void Steer()
    {
        m_steeringAngle = maxSteerAngle * m_horizontalInput;
        frontleft.steerAngle = m_steeringAngle;
        frontright.steerAngle = m_steeringAngle;
    }
    
    private void Accelerate(float s)
    {
        if (s < 1.4f)
        {
            force = motorForce * 3;
        }
        else if (s < 4.2f)
        {
            force = motorForce * 2;
        }
        else if (s < 8.4f)
        {
            force = motorForce;
        }
        else if (s < 16)
        {
            force = motorForce * 0.5f;
        }
        else
        {
            force = 0;
        }

        frontleft.motorTorque = m_verticalInput * force;
        frontright.motorTorque = m_verticalInput * force;
        rearleft.motorTorque = m_verticalInput * force;
        rearright.motorTorque = m_verticalInput * force;
    }
    
    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontleft, frontleftT);
        UpdateWheelPose(frontright, frontrightT);
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
}