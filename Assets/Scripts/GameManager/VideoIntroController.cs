using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Điều khiển phát video intro và chuyển scene khi kết thúc
/// </summary>
public class VideoIntroController : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("Đường dẫn video trong Assets (ví dụ: Video/1030.mp4)")]
    public string videoPath = "Video/1030.mp4";
    
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

    private VideoPlayer videoPlayer;
    private bool hasVideoStarted = false;
    private bool isVideoEnded = false;

    void Start()
    {
        SetupVideoPlayer();
        
        // Hiển thị text hướng dẫn skip nếu có
        if (skipText != null)
        {
            skipText.text = allowSkip ? "Nhấn SPACE hoặc Click để bỏ qua" : "";
        }
    }

    void SetupVideoPlayer()
    {
        // Lấy hoặc thêm VideoPlayer component
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Cấu hình VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane; // Phát full screen ở background
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch; // Kéo giãn full màn hình
        
        // Set video clip từ Resources hoặc StreamingAssets
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = System.IO.Path.Combine(Application.dataPath, videoPath);
        
        // Cấu hình audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioVolume(0, videoVolume);
        
        // Đăng ký sự kiện khi video kết thúc
        videoPlayer.loopPointReached += OnVideoEnd;
        
        // Đăng ký sự kiện khi video sẵn sàng
        videoPlayer.prepareCompleted += OnVideoPrepared;
        
        // Chuẩn bị video
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Video prepared, starting playback...");
        videoPlayer.Play();
        hasVideoStarted = true;
    }

    void Update()
    {
        // Cho phép skip video
        if (allowSkip && hasVideoStarted && !isVideoEnded)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
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
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
