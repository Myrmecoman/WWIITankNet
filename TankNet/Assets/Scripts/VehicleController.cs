using UnityEngine;
using Photon.Pun;

public class VehicleController : MonoBehaviourPun, IPunObservable
{
    // all the wheels that are controlled
    public WheelCollider frontleft, frontright;
    public WheelCollider rearleft, rearright;
    public Transform frontleftT, frontrightT;
    public Transform rearleftT, rearrightT;
    public float maxSteerAngle = 30;
    public float motorForce = 300;

    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steeringAngle;
    private Rigidbody rb;
    private float s;
    private bool destroyed = false;
    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(rb.centerOfMass.x, rb.centerOfMass.y - 1, rb.centerOfMass.z);
    }


    private void Update()
    {
        s = rb.velocity.magnitude;
        Debug.Log("speed is " + s);
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            // manage the movements of the tank
            GetInput();
            Steer();
            UpdateWheelPoses();
            if (Input.GetKey("space") || destroyed)
            {
                frontleft.brakeTorque = 5000;
                frontright.brakeTorque = 5000;
                rearleft.brakeTorque = 5000;
                rearright.brakeTorque = 5000;
            }
            else
            {
                frontleft.brakeTorque = 0;
                frontright.brakeTorque = 0;
                rearleft.brakeTorque = 0;
                rearright.brakeTorque = 0;
                Accelerate();
            }
        }
    }
    
    public void GetInput()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = -Input.GetAxis("Vertical");
    }
    
    private void Steer()
    {
        m_steeringAngle = maxSteerAngle * m_horizontalInput;
        frontleft.steerAngle = m_steeringAngle;
        frontright.steerAngle = m_steeringAngle;
    }
    
    private void Accelerate()
    {
        frontleft.motorTorque = m_verticalInput * motorForce;
        frontright.motorTorque = m_verticalInput * motorForce;
        rearleft.motorTorque = m_verticalInput * motorForce;
        rearright.motorTorque = m_verticalInput * motorForce;
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