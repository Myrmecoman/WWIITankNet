using UnityEngine;

[CreateAssetMenu(fileName = "Keybindings", menuName = "Keybindings")]
public class Keybindings : ScriptableObject
{
    [System.Serializable]
    public class KeybindingCheck
    {
        public KeybindingActions keybindingAction;
        public KeyCode keycode;
    }

    public KeybindingCheck[] keybindingChecks;
}
