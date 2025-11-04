using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        AdjustAudioSource();
    }

    public void Play(AudioClip clip, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        // 1. Di chuyển vật thể này đến vị trí mong muốn
        transform.position = position;

        // 2. Cài đặt AudioSource
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        // Đảm bảo nó là 3D (phòng trường hợp trước đó gọi Play2D)
        audioSource.spatialBlend = 1.0f;

        // 3. Phát
        audioSource.Play();

        // 4. Bắt đầu đếm ngược để trả về Pool
        StartCoroutine(ReturnAfterPlay(clip.length));
    }
    public void Play(AudioClip clip, Vector3 position, float volume)
    {
        // Gọi hàm chính với min/max mặc định
        Play(clip, position, volume, 10f, 60f);
    }

    /// <summary>
    /// Chơi âm thanh 2D (giống code cũ của bạn, dùng cho UI, nhạc...)
    /// </summary>
    public void Play2D(AudioClip clip, float volume)
    {
        // Chuyển sang 2D
        audioSource.spatialBlend = 0.0f;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        StartCoroutine(ReturnAfterPlay(clip.length));
    }
    private IEnumerator ReturnAfterPlay(float duration)
    {
        yield return new WaitForSeconds(duration);
        ObjectPoolManager.ReturnObject(gameObject);
    }
    private void AdjustAudioSource()
    {
        if (audioSource == null) return;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 20f;
        audioSource.dopplerLevel = 0.5f;
    }
}
