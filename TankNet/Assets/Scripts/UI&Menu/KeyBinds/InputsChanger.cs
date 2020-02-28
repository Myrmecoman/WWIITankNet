using UnityEngine;
using UnityEngine.UI;


public class InputsChanger : MonoBehaviour
{
    public Keybindings binds;
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
    private string nameButton;


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
                    binds.keybindingChecks[0].keycode = vKey;
                }
                if (nameButton == "backward")
                {
                    backwardTxt.text = vKey.ToString();
                    binds.keybindingChecks[1].keycode = vKey;
                }
                if (nameButton == "left")
                {
                    leftTxt.text = vKey.ToString();
                    binds.keybindingChecks[2].keycode = vKey;
                }
                if (nameButton == "right")
                {
                    rightTxt.text = vKey.ToString();
                    binds.keybindingChecks[3].keycode = vKey;
                }
                if (nameButton == "brake")
                {
                    brakeTxt.text = vKey.ToString();
                    binds.keybindingChecks[4].keycode = vKey;
                }
                if (nameButton == "shoot")
                {
                    shootTxt.text = vKey.ToString();
                    binds.keybindingChecks[5].keycode = vKey;
                }
                if (nameButton == "lock_turret")
                {
                    lock_turretTxt.text = vKey.ToString();
                    binds.keybindingChecks[6].keycode = vKey;
                }
                if (nameButton == "reload")
                {
                    reloadtxt.text = vKey.ToString();
                    binds.keybindingChecks[7].keycode = vKey;
                }
                // if (nameButton == "aim")
                // {
                //    
                // }
                if (nameButton == "light")
                {
                    lightTxt.text = vKey.ToString();
                    binds.keybindingChecks[8].keycode = vKey;
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