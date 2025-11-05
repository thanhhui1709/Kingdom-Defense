using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{


    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public VideoClip winClip;
    public VideoClip loseClip;

    private string nextSceneAfterVideo = "WaitScene"; 

    private void Awake()
    {
       GameManager.Instance.CutsceneManager = this;
       GameEvent.Instance.SubscribeWinStage(PlayWinVideo);
       GameEvent.Instance.SubscribeGameOver(PlayLoseVideo);
    }

    // 🔥 Gọi hàm này khi thắng
    public void PlayWinVideo()
    {
        PlayVideo(winClip);
    }

    // 🔥 Gọi hàm này khi thua
    public void PlayLoseVideo()
    {
        PlayVideo(loseClip);
    }

    private void PlayVideo(VideoClip clip)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer not assigned in CutsceneManager!");
            return;
        }
        GameManager.Instance.InGameUIManager.gameObject.SetActive(false);
        videoPlayer.clip = clip;
        videoPlayer.Play();

        // Khi phát xong video → chuyển scene
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;

        SceneManager.LoadScene(nextSceneAfterVideo);
    }
}
