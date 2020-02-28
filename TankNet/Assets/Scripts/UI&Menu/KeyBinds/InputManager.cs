using UnityEngine;


public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    [SerializeField] private Keybindings keybindings;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(this);
        DontDestroyOnLoad(this);
    }


    public KeyCode GetKeyForAction(KeybindingActions keybindingAction)
    {
        foreach(Keybindings.KeybindingCheck keybindingCheck in  keybindings.keybindingChecks)
        {
            if (keybindingCheck.keybindingAction == keybindingAction)
                return keybindingCheck.keycode;
        }
        return KeyCode.None;
    }


    public bool GetKeyDown(KeybindingActions key)
    {
        foreach (Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks)
        {
            if (keybindingCheck.keybindingAction == key)
                return Input.GetKeyDown(keybindingCheck.keycode);
        }
        return false;
    }


    public bool GetKey(KeybindingActions key)
    {
        foreach (Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks)
        {
            if (keybindingCheck.keybindingAction == key)
                return Input.GetKey(keybindingCheck.keycode);
        }
        return false;
    }


    public bool GetKeyUp(KeybindingActions key)
    {
        foreach (Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks)
        {
            if (keybindingCheck.keybindingAction == key)
                return Input.GetKeyUp(keybindingCheck.keycode);
        }
        return false;
    }
}