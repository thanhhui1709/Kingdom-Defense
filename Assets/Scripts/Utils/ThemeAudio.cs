using UnityEngine;
using System.Collections;

public class ThemeAudio : MonoBehaviour
{
    [Header("Theme Music Playlist")]
    [SerializeField] private AudioClip[] themePlaylist;
    [Range(0f, 1f)][SerializeField] private float themeVolume = 0.7f;
    [SerializeField] private bool shuffle = false;

    [Header("Special Music Settings")]
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private AudioClip loseMusic;
    [Range(0f, 1f)][SerializeField] private float specialVolume = 1.0f;

    [Header("Fading")]
    [SerializeField] private float crossfadeDuration = 1.5f;

    private AudioSource themeSource;
    private AudioSource specialSource;

    private int currentTrackIndex = -1;
    private bool isPlayingSpecial = false;
    private Coroutine fadeCoroutine;
    private bool hasMuted = false;

    private void Awake()
    {
      
        GameManager.Instance.ThemeAudio = this;

        themeSource = gameObject.AddComponent<AudioSource>();
        specialSource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(themeSource, themeVolume);
        ConfigureSource(specialSource, 0f);

        // 🧩 Gắn event cho win/lose
        GameEvent.Instance.SubscribeWinStage(OnGameWin);
        GameEvent.Instance.SubscribeGameOver(OnGameOver);
    }

    private void ConfigureSource(AudioSource source, float initialVolume)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = initialVolume;
    }

    private void Start()
    {
        hasMuted = false;
        if (themePlaylist.Length > 0)
        {
            if (shuffle) ShufflePlaylist();
            PlayNextThemeTrack();
        }
    }
    private void OnEnable()
    {
        hasMuted = false;
        isPlayingSpecial = false;
    }

    private void Update()
    {
        if (!hasMuted&&!isPlayingSpecial && !themeSource.isPlaying && themePlaylist.Length > 0)
        {
            PlayNextThemeTrack();
        }
    }

    private void PlayNextThemeTrack()
    {
        if (shuffle)
            currentTrackIndex = Random.Range(0, themePlaylist.Length);
        else
            currentTrackIndex = (currentTrackIndex + 1) % themePlaylist.Length;

        themeSource.clip = themePlaylist[currentTrackIndex];
        themeSource.volume = themeVolume;
        themeSource.Play();
    }

    // 🏆 Khi thắng game
    private void OnGameWin()
    {
        StopAllCoroutines();
        hasMuted = true;
        if (winMusic != null)
        {
            StartCoroutine(FadeOutAndPlaySpecial(winMusic));
        }
        else
        {
            StartCoroutine(FadeOutThemeOnly());
        }
    }

    // 💀 Khi thua game
    private void OnGameOver()
    {
        StopAllCoroutines();
        hasMuted = true;
        if (loseMusic != null)
        {
            StartCoroutine(FadeOutAndPlaySpecial(loseMusic));
        }
        else
        {
            StartCoroutine(FadeOutThemeOnly());
        }
    }

    // 🔥 Fade out nhạc nền và phát special
    private IEnumerator FadeOutAndPlaySpecial(AudioClip clip)
    {
        float timer = 0f;
        float startVolume = themeSource.volume;

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;
            themeSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        themeSource.Stop();

        PlaySpecialMusic(clip);
    }

    private IEnumerator FadeOutThemeOnly()
    {
        float timer = 0f;
        float startVolume = themeSource.volume;

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;
            themeSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        themeSource.Stop();
    }

    public void PlaySpecialMusic(AudioClip clip)
    {
        if (clip == null) return;

        isPlayingSpecial = true;
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        specialSource.clip = clip;
        specialSource.loop = false;
        specialSource.volume = 0f;
        specialSource.Play();

        fadeCoroutine = StartCoroutine(FadeInSpecial());
    }

    private IEnumerator FadeInSpecial()
    {
        float timer = 0f;
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;
            specialSource.volume = Mathf.Lerp(0f, specialVolume, t);
            yield return null;
        }

        specialSource.volume = specialVolume;
        fadeCoroutine = null;
    }

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

    public void StopMusic()
    {
        themeSource.Stop();
        specialSource.Stop();
    }
    private void OnDestroy()
    {
        // Rất quan trọng: Hủy đăng ký các sự kiện khi script này bị hủy
        if (GameEvent.Instance != null)
        {
            // Bạn sẽ cần thêm các hàm Unsubscribe vào GameEvent
            GameEvent.Instance.UnsubscribeWinStage(OnGameWin);
            GameEvent.Instance.UnsubscribeGameOver(OnGameOver);
        }
    }

}
