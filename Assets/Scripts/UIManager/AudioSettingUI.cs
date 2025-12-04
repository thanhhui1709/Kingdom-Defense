using UnityEngine;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // 1. Cập nhật giá trị Slider theo setting hiện tại
        musicSlider.value = gameSetting.MusicVolume;
        sfxSlider.value = gameSetting.SFXVolume;

        // 2. Lắng nghe sự kiện kéo Slider
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    public void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    public void OnSFXSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
}
