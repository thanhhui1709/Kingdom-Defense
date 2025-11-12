using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Điều khiển phát video intro và chuyển scene khi kết thúc
/// </summary>
public class VideoIntroController : MonoBehaviour
{
    
    [Tooltip("Tên scene sẽ load sau khi video kết thúc (ví dụ: Menu)")]
    public string nextSceneName = "Menu";
    
    [Header("UI References")]
    [Tooltip("Text hiển thị hướng dẫn skip (tùy chọn)")]
    public TMPro.TextMeshProUGUI skipText;
    
    [Tooltip("Cho phép skip video bằng phím Space/Click")]
    public bool allowSkip = true;
    
    [Header("Audio Settings")]
    [Tooltip("Âm lượng video (0-1)")]
    [Range(0f, 1f)]
    public float videoVolume = 1f;

    public VideoPlayer videoPlayer;
    private bool hasVideoStarted = false;
    private bool isVideoEnded = false;

    void Start()
    {

       
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer chưa được gán!", this);
                LoadNextScene(); // Không có video, bỏ qua
                return;
            }

            // 1. Cài đặt âm thanh
            // (Giả sử bạn đã kéo AudioSource vào videoPlayer)
            if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
            {
                videoPlayer.SetDirectAudioVolume(0, videoVolume);
            }

            // 2. Đăng ký sự kiện KHI KẾT THÚC
            videoPlayer.loopPointReached += OnVideoEnd;

            // 3. Bật video
            videoPlayer.Play();

            // 4. Đặt cờ cho phép SKIP
            hasVideoStarted = true;
            isVideoEnded = false;

            // Hiển thị text hướng dẫn skip nếu có
            if (skipText != null)
            {
                skipText.text = allowSkip ? "Nhấn SPACE hoặc Click để bỏ qua" : "";
            }
        
    }

    void Update()
    {
        // Cho phép skip video
        if (allowSkip && hasVideoStarted && !isVideoEnded)
        {
            if (Input.anyKeyDown)
            {
                SkipVideo();
            }
        }
    }

    void OnVideoEnd(VideoPlayer source)
    {
        Debug.Log("Video ended, loading next scene: " + nextSceneName);
        isVideoEnded = true;
        LoadNextScene();
    }

    void SkipVideo()
    {
        Debug.Log("Video skipped by user");
        isVideoEnded = true;
        
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        LoadNextScene();
    }

    void LoadNextScene()
    {
        // Kiểm tra scene có tồn tại không
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }

    void OnDestroy()
    {
        // Cleanup
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
