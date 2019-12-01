using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using System.Collections;

//nécessite un input
[RequireComponent(typeof(InputField))]

public class menuGarageInputPseudo : MonoBehaviour
{

    const string clefPrefPseudo = "pseudoDefaut";

    // Start is called before the first frame update
    void Start()
    {
        // Definir un pseudo par défaut
        string pseudoDefaut = "Hussard";

        // Mettre l'input dans une valeur
        InputField champInput = this.GetComponent<InputField>();

        // Si l'input existe
        if (champInput != null)
        {
            // Si le joueur a déjà une preference pour son pseudo (il a déjà joué et mis un pseudo)
            if (PlayerPrefs.HasKey(clefPrefPseudo))
            {
                // On récupère la préférence du joueur pour la définir en tant que pseudo par défaut
                pseudoDefaut = PlayerPrefs.GetString(clefPrefPseudo);
                champInput.text = pseudoDefaut;
            }
        }

        PhotonNetwork.NickName = pseudoDefaut;
    }

    // Défini le nom préféré du joueur selon la valeur (value) dans le champs
    public void changerNomJoueur(string value)
    {

        string pseudoEcrit = "Hussard";

        //QQchose est ecrit ?
        if (!string.IsNullOrEmpty(value))
        {
            pseudoEcrit = value;
        }
        PhotonNetwork.NickName = pseudoEcrit;
        
        PlayerPrefs.SetString(clefPrefPseudo, pseudoEcrit);
    }
}
