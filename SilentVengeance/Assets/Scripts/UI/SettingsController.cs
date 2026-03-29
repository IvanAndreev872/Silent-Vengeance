using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider soundSlider;

    void Start()
    {
        musicSlider.value = 1f;
        soundSlider.value = 1f;

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        soundSlider.onValueChanged.AddListener(OnSoundChanged);
    }

    void OnMusicChanged(float value)
    {
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("MusicVolume", dB);
    }

    void OnSoundChanged(float value)
    {
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("SFXVolume", dB);
    }
}