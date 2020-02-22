using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject particleTerrain;
    public GameObject particleVehicle;
    public GameObject dSphere;

    private Rigidbody rb;
    

    // Use this for initialization
    private void Start()
    {
        Destroy(gameObject, 8);
        rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * 3000, ForceMode.Force);
    }


    private void OnCollisionEnter(Collision col)
    {
        if (col.transform.tag == "Player")
        {
            Instantiate(particleVehicle, transform.position, Quaternion.identity);
        }

        if (col.transform.tag == "Terrainus")
        {
            Instantiate(particleTerrain, transform.position, Quaternion.identity);
        }

        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}

// line I may want to use later for debug
//Instantiate(dSphere, transform.position, Quaternion.identity); // instantiate a sphere at collision for debug