using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundSlider;

    void Start()
    {
        // Стартовые значения
        musicSlider.value = 1f;
        soundSlider.value = 1f;

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        soundSlider.onValueChanged.AddListener(OnSoundChanged);
    }

    void OnMusicChanged(float value)
    {
        // Здесь позже подключим AudioMixer
        Debug.Log("Музыка: " + value);
    }

    void OnSoundChanged(float value)
    {
        Debug.Log("Звук: " + value);
    }
}