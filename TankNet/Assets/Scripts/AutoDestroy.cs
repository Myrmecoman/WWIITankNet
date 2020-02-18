using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public GameObject ObjToDestroy;
    public float TimeToDestroy;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(ObjToDestroy, TimeToDestroy);
    }
}
