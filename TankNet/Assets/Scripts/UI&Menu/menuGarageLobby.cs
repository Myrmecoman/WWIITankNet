using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class menuGarageLobby : MonoBehaviourPunCallbacks
{
    //Numéro de version
    string numeroVersion = "0.0.1";

    //Nombre maximum de personne par salle
    [Tooltip("Nombre de joueur maximum par salle. 0 Pour aucune limite")]
    [SerializeField]
    private byte joueurMaximumParSalle = 10 ;
    

    //lancé avant l'initialisation de l'objet
    void Awake()
    {
        //sychronise automatiquement les scenes entre joueurs et serveur.
        //Permet d'utiliser Photonnetwork.LoadLevel()
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    //Processus de connection
    // - Rejoindre une salle si déjà connecté
    // - Connecte l'application à PCN si non connecté
    public void Connect()
    {
        //Test si on est connecté
        if (PhotonNetwork.IsConnected)
        {
            //Tenter de joindre une salle
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            //se connecter à PCN
            PhotonNetwork.GameVersion = numeroVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    //Fonction appelée quand on est connecté au serveur
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connecté au PNC");

        //Rejoindre une salle au hasard
        PhotonNetwork.JoinRandomRoom();
    }

    //Fonction appelée quand on est deconnecté du serveur
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Deconnecté du PNC : " + cause);
    }

    //Fonctione appelée quand on n'a pas réussi à joindre une salle (cf voir PhotonNetwork.JoinRandomRoom() dans la fonction onConnectedToMaster() )
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Connection impossible à une salle. Erreur " + returnCode + " : " + message );

        //Puisque nous n'avons pas pu rejoindre une salle, on créer la notre
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = joueurMaximumParSalle } );
    }

    //Fonction appelée quand une salle a été rejoint
    public override void OnJoinedRoom()
    {
        Debug.Log("Connecté à une salle");
    }
}