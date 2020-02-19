using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject particleTerrain;
    public GameObject particleVehicle;
    public GameObject dSphere;
    

    // Use this for initialization
    void Start()
    {
        Destroy(gameObject, 8);
    }


    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            Instantiate(dSphere, transform.position, Quaternion.identity); // instantiate a sphere at collision for debug
            Instantiate(particleVehicle, transform.position, Quaternion.identity);
        }

        if (col.gameObject.tag == "Terrainus")
        {
            //Instantiate(dSphere, transform.position, Quaternion.identity); // instantiate a sphere at collision for debug
            Instantiate(particleTerrain, transform.position, Quaternion.identity);
        }

        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}