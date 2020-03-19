using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Dropdown ResolutionDropdown;
    public Dropdown QualityDropdown;


    void Start()
    {
        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
        ResolutionDropdown.value = 6;
        ResolutionDropdown.RefreshShownValue();

        QualitySettings.SetQualityLevel(3);
        QualityDropdown.value = 3;
        QualityDropdown.RefreshShownValue();
    }


    public void SetResolution()
    {

        int a = 1920;
        int b = 1080;
        if (ResolutionDropdown.value == 0)
        {
            a = 800;
            b = 600;
        }
        if (ResolutionDropdown.value == 1)
        {
            a = 1024;
            b = 720;
        }
        if (ResolutionDropdown.value == 2)
        {
            a = 1280;
            b = 800;
        }
        if (ResolutionDropdown.value == 3)
        {
            a = 1360;
            b = 768;
        }
        if (ResolutionDropdown.value == 4)
        {
            a = 1600;
            b = 900;
        }
        if (ResolutionDropdown.value == 5)
        {
            a = 1680;
            b = 1050;
        }
        if (ResolutionDropdown.value == 6)
        {
            a = 1920;
            b = 1080;
        }
        Screen.SetResolution(a, b, Screen.fullScreen);
    }


    public void SetQuality()
    {
        QualitySettings.SetQualityLevel(QualityDropdown.value);
    }


    public void SetFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
