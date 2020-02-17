using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public GameObject particleTerrain;
    public GameObject particleVehicle;
    public GameObject dSphere;
    

    // Use this for initialization
    void Start()
    {
        Destroy(gameObject, 8);
    }


    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("Player"))
        {
            Debug.Log("hit player");
            Instantiate(dSphere, transform.position, Quaternion.identity); // instantiate a sphere at collision for debug
            Instantiate(particleVehicle, transform.position, Quaternion.identity);
        }
        if (coll.collider.CompareTag("Terrainus"))
        {
            Debug.Log("hit terrain");
            Instantiate(dSphere, transform.position, Quaternion.identity); // instantiate a sphere at collision for debug
            Instantiate(particleTerrain, transform.position, Quaternion.identity);
        }
        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}