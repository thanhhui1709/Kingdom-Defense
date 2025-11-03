using UnityEngine;
using System.Collections; // Cần cho Coroutine

public class ThemeAudio : MonoBehaviour
{
    [Header("Theme Music Playlist")]
    [Tooltip("Kéo các bản nhạc nền (theme) vào đây")]
    [SerializeField] private AudioClip[] themePlaylist;

    [Tooltip("Âm lượng cho nhạc nền")]
    [Range(0f, 1f)]
    [SerializeField] private float themeVolume = 0.7f;

    [Tooltip("Nếu bật, danh sách nhạc sẽ phát ngẫu nhiên. Nếu không, sẽ phát theo thứ tự.")]
    [SerializeField] private bool shuffle = false;

    [Header("Special Music Settings")]
    [Tooltip("Âm lượng cho nhạc đặc biệt (như nhạc Boss)")]
    [Range(0f, 1f)]
    [SerializeField] private float specialVolume = 1.0f;

    [Header("Fading")]
    [Tooltip("Thời gian (giây) để chuyển bài (crossfade)")]
    [SerializeField] private float crossfadeDuration = 1.5f;

    // ----- Internal -----
    private AudioSource themeSource;    // Nguồn phát nhạc nền
    private AudioSource specialSource;  // Nguồn phát nhạc đặc biệt

    private int currentTrackIndex = -1;
    private bool isPlayingSpecial = false;
    private Coroutine fadeCoroutine; // Coroutine đang chạy (nếu có)

    // Tạo Singleton để dễ dàng gọi từ script khác
    public static ThemeAudio Instance { get; private set; }

    private void Awake()
    {
        // --- Cài đặt Singleton ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Chỉ cho phép 1 MusicManager tồn tại
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ MusicManager khi chuyển scene

        // --- Tự động tạo 2 AudioSource ---
        themeSource = gameObject.AddComponent<AudioSource>();
        specialSource = gameObject.AddComponent<AudioSource>();

        // Cấu hình 2 nguồn
        ConfigureSource(themeSource, themeVolume);
        ConfigureSource(specialSource, 0f); // Bắt đầu với âm lượng 0
    }

    private void ConfigureSource(AudioSource source, float initialVolume)
    {
        source.playOnAwake = false; // Tắt tự động phát
        source.loop = false;        // Script sẽ tự xử lý loop
        source.spatialBlend = 0.0f; // Đảm bảo là nhạc 2D
        source.volume = initialVolume;
    }

    private void Start()
    {
        if (themePlaylist.Length > 0)
        {
            if (shuffle)
                ShufflePlaylist();

            PlayNextThemeTrack();
        }
    }

    private void Update()
    {
        // Nếu không đang chơi nhạc đặc biệt, và nhạc nền đã dừng (hết bài)
        if (!isPlayingSpecial && !themeSource.isPlaying && themePlaylist.Length > 0)
        {
            PlayNextThemeTrack();
        }
    }

    private void PlayNextThemeTrack()
    {
        if (shuffle)
        {
            currentTrackIndex = Random.Range(0, themePlaylist.Length);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % themePlaylist.Length;
        }

        themeSource.clip = themePlaylist[currentTrackIndex];
        themeSource.Play();
    }

    /// <summary>
    /// Hàm này được gọi từ script Boss hoặc một sự kiện.
    /// </summary>
    public void PlaySpecialMusic(AudioClip clip)
    {
        if (clip == null) return;

        isPlayingSpecial = true;

        // Dừng Coroutine (nếu đang chạy)
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Bắt đầu phát nhạc Boss (với âm lượng 0)
        specialSource.clip = clip;
        specialSource.loop = true; // Nhạc Boss thường lặp lại
        specialSource.volume = 0f;
        specialSource.Play();

        // Bắt đầu Coroutine để mờ dần
        fadeCoroutine = StartCoroutine(Crossfade(themeSource, specialSource, specialVolume));
    }

    /// <summary>
    /// Gọi hàm này khi Boss chết để quay lại nhạc nền.
    /// </summary>
    public void StopSpecialMusic()
    {
        if (!isPlayingSpecial) return;
        isPlayingSpecial = false;

        // Dừng Coroutine (nếu đang chạy)
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Bắt đầu Coroutine để mờ dần (theo hướng ngược lại)
        fadeCoroutine = StartCoroutine(Crossfade(specialSource, themeSource, themeVolume));
    }

    /// <summary>
    /// Coroutine để mờ dần
    /// </summary>
    private IEnumerator Crossfade(AudioSource sourceOut, AudioSource sourceIn, float targetInVolume)
    {
        float timer = 0f;
        float startOutVolume = sourceOut.volume;
        float startInVolume = sourceIn.volume;

        // Bật sourceIn nếu nó chưa bật (như themeSource)
        if (!sourceIn.isPlaying)
            sourceIn.Play();

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;

            sourceOut.volume = Mathf.Lerp(startOutVolume, 0f, t);
            sourceIn.volume = Mathf.Lerp(startInVolume, targetInVolume, t);

            yield return null;
        }

        // Đảm bảo kết quả chính xác
        sourceOut.volume = 0f;
        sourceOut.Stop();
        sourceIn.volume = targetInVolume;

        fadeCoroutine = null;
    }

    // Hàm xáo trộn danh sách (nếu bật shuffle)
    private void ShufflePlaylist()
    {
        for (int i = 0; i < themePlaylist.Length - 1; i++)
        {
            int rnd = Random.Range(i, themePlaylist.Length);
            AudioClip temp = themePlaylist[rnd];
            themePlaylist[rnd] = themePlaylist[i];
            themePlaylist[i] = temp;
        }
    }
}