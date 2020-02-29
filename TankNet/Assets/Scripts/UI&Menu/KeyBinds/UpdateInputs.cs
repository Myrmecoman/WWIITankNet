using UnityEngine;
using UnityEngine.UI;


public class UpdateInputs : MonoBehaviour
{
    public Keybindings binds;
    public Text[] texts;

    private string forw;
    private string back;
    private string left;
    private string right;
    private string brake;
    private string shoot;
    private string lockt;
    private string rel;
    private string lightt;


    private void Start()
    {
        forw = PlayerPrefs.GetString("forward");
        if (forw == "")
            forw = "Z";
        back = PlayerPrefs.GetString("backward");
        if (back == "")
            back = "S";
        left = PlayerPrefs.GetString("left");
        if (left == "")
            left = "Q";
        right = PlayerPrefs.GetString("right");
        if (right == "")
            right = "D";
        brake = PlayerPrefs.GetString("brake");
        if (brake == "")
            brake = "Space";
        shoot = PlayerPrefs.GetString("shoot");
        if (shoot == "")
            shoot = "Mouse0";
        lockt = PlayerPrefs.GetString("lock_turret");
        if (lockt == "")
            lockt = "Mouse1";
        rel = PlayerPrefs.GetString("reload");
        if (rel == "")
            rel = "R";
        lightt = PlayerPrefs.GetString("light");
        if (lightt == "")
            lightt = "L";
        binds.keybindingChecks[0].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), forw);
        binds.keybindingChecks[1].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), back);
        binds.keybindingChecks[2].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), left);
        binds.keybindingChecks[3].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), right);
        binds.keybindingChecks[4].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), brake);
        binds.keybindingChecks[5].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), shoot);
        binds.keybindingChecks[6].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), lockt);
        binds.keybindingChecks[7].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), rel);
        binds.keybindingChecks[8].keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), lightt);
    }


    public void UpdateTexts()
    {
        for (int i = 0; i < texts.Length; i++)
            texts[i].text = binds.keybindingChecks[i].keycode.ToString();
    }
}