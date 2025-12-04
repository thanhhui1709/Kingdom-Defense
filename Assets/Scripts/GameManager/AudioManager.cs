using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("References")]
    [SerializeField] private GameSetting gameSetting;
    [SerializeField] private AudioMixer mainMixer;

    // --- THÊM MỚI: Biến lưu trữ Group ---
    private AudioMixerGroup musicGroup;
    private AudioMixerGroup sfxGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; 
        }

        // --- TÌM GROUP TRONG MIXER ---
        // Lưu ý: Tên string phải chuẩn 100% như trong cửa sổ Audio Mixer
        var musicGroups = mainMixer.FindMatchingGroups("Music");
        var sfxGroups = mainMixer.FindMatchingGroups("SFX");

        if (musicGroups.Length > 0) musicGroup = musicGroups[0];
        if (sfxGroups.Length > 0) sfxGroup = sfxGroups[0];
    }

    private void Start()
    {
        // Load lại setting cũ hoặc dùng mặc định từ GameSetting
        float savedMusic = PlayerPrefs.GetFloat("MusicVol", gameSetting.MusicVolume);
        float savedSFX = PlayerPrefs.GetFloat("SFXVol", gameSetting.SFXVolume);

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
    }

    public void SetMusicVolume(float value)
    {
        gameSetting.MusicVolume = value;
        ApplyVolume(Const.MIXER_MUSIC_VOL, value);
        PlayerPrefs.SetFloat("MusicVol", value); // Lưu lại
    }

    public void SetSFXVolume(float value)
    {
        gameSetting.SFXVolume = value;
        ApplyVolume(Const.MIXER_SFX_VOL, value);
        PlayerPrefs.SetFloat("SFXVol", value); // Lưu lại
    }

    // Công thức chuyển từ Slider (0-1) sang Decibel (-80 đến 0)
    private void ApplyVolume(string parameterName, float sliderValue)
    {
        // Mathf.Log10(0) sẽ lỗi, nên slider min phải > 0 (ví dụ 0.0001)
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dbValue);
    }
    public AudioMixerGroup GetMusicGroup()
    {
        return musicGroup;
    }

    public AudioMixerGroup GetSFXGroup()
    {
        return sfxGroup;
    }
}