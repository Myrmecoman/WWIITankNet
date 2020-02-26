﻿using UnityEngine;
using Photon.Pun;


public class PUN2_RoomController : MonoBehaviourPunCallbacks
{
    public Transform SunRotation;
    public GameObject playerPrefab;
    public Transform[] spawnPoint;

    private double t = 0;
    private bool spawned = false;
    private int h;
    private int m;

    
    void Start()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("Is not in the room, returning back to Lobby");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            h = PlayerPrefs.GetInt("Hour");
            m = PlayerPrefs.GetInt("Minute");
            Debug.Log("Time is " + h + " : " + m);
            SunRotation.rotation = Quaternion.Euler(360 * (h - 6) / 24 + 15 * m / 60, SunRotation.rotation.eulerAngles.y, SunRotation.rotation.eulerAngles.z);
        }
        else
            photonView.RPC("Ask", RpcTarget.MasterClient);
    }

    [PunRPC]
    void Ask()
    {
        Debug.Log("We are master client, send rotations for time");
        photonView.RPC("Rotations", RpcTarget.Others, h, m);
    }

    [PunRPC]
    void Rotations(int hi, int mi)
    {
        Debug.Log("We are joining, receive time");
        SunRotation.rotation = Quaternion.Euler(360 * (hi - 6) / 24 + 15 * mi / 60, SunRotation.rotation.eulerAngles.y, SunRotation.rotation.eulerAngles.z);
    }

    void Update()
    {
        if (t < 1.5f)
            t += Time.deltaTime;
        else if (!spawned)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint[Random.Range(0, spawnPoint.Length - 1)].position, Quaternion.identity, 0);
            spawned = true;
        }
    }

    void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        //Leave this Room
        if (GUI.Button(new Rect(5, 5, 125, 25), "Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }

        //Show the Room name
        GUI.Label(new Rect(135, 5, 200, 25), PhotonNetwork.CurrentRoom.Name);

        //Show the list of the players connected to this Room
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            //Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
            string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? ": MasterClient" : "");
            GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].NickName + isMasterClient);
        }
    }

    public override void OnLeftRoom()
    {
        //We have left the Room, return back to the GameLobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
    }
}