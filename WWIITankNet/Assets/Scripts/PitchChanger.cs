using UnityEngine;

public class PitchChanger : MonoBehaviour
{
    public float pitchChangeValue;


    void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.pitch = source.pitch + Random.Range(-pitchChangeValue, pitchChangeValue);
        source.Play();
    }
}