using UnityEngine;
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
            photonView.RPC("Ask", RpcTarget.MasterClient, photonView.ViewID);
    }

    [PunRPC]
    void Ask(int id)
    {
        Debug.Log("Your are MasterClient and I want your fucking sun rotations");
        photonView.RPC("Send", RpcTarget.Others, SunRotation.rotation, id);
    }

    [PunRPC]
    void Send(Quaternion sunrot, int id)
    {
        Debug.Log("MasterClient sends you all his fucking rotation");
        if(photonView.ViewID == id)
            SunRotation.rotation = sunrot;
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