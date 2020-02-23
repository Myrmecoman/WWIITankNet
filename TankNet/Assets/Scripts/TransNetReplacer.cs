using UnityEngine;
using Photon.Pun;

public class TransNetReplacer : MonoBehaviourPun
{
    private double t = 0;

    
    void Update()
    {
        if (photonView.IsMine)
        {
            if (t > 1)
            {
                t = t % 1;
                photonView.RPC("UpdateTransform", RpcTarget.Others, transform.position, photonView.ViewID);
            }
        }
    }


    [PunRPC]
    void UpdateTransform(Vector3 pos, int ph)
    {
        if(photonView.ViewID == ph)
            transform.position = pos;
    }
}