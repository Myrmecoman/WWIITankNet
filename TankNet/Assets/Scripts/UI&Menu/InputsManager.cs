using UnityEngine;
using UnityEngine.UI;


public class InputsManager : MonoBehaviour
{
    [HideInInspector]
    public string nameButton;

    public Text forwardTxt;
    public Text backwardTxt;
    public Text leftTxt;
    public Text rightTxt;
    public Text brakeTxt;
    public Text shootTxt;
    public Text lock_turretTxt;
    public Text reloadtxt;
    public Text aimTxt;
    public Text lightTxt;

    private bool selected = false;


    public void ButtonPressed(string nameB)
    {
        nameButton = nameB;
    }


    void Update()
    {
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(vKey))
            {
                if (nameButton == "forward")
                {
                    forwardTxt.text = vKey.ToString();
                }
                if (nameButton == "backward")
                {
                    backwardTxt.text = vKey.ToString();
                }
                if (nameButton == "left")
                {
                    leftTxt.text = vKey.ToString();
                }
                if (nameButton == "right")
                {
                    rightTxt.text = vKey.ToString();
                }
                if (nameButton == "brake")
                {
                    brakeTxt.text = vKey.ToString();
                }
                if (nameButton == "shoot")
                {
                    shootTxt.text = vKey.ToString();
                }
                if (nameButton == "lock_turret")
                {
                    lock_turretTxt.text = vKey.ToString();
                }
                if (nameButton == "reload")
                {
                    reloadtxt.text = vKey.ToString();
                }
                // if (nameButton == "aim")
                // {
                //    
                // }
                if (nameButton == "light")
                {
                    lightTxt.text = vKey.ToString();
                }
                selected = true;
            }
        }

        if (selected)
        {
            selected = false;
            gameObject.SetActive(false);
        }
    }
}