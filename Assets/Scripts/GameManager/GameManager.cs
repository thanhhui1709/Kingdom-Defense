using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<string> stageNames;
    private GameEvent _gameEvent;
    private InGameUIManager _inGameUIManager;
    private CutsceneManager _cutsceneManager;
    private ThemeAudio _themeAudio;

  

    public GameEvent GameEvent
    {
        get
        {
            return _gameEvent;
        }
        set
        {
            _gameEvent = value;
            _gameEvent.SubscribeWinStage(WinGame);
        }
    }
    public ThemeAudio ThemeAudio
    {
        get => _themeAudio;
        set => _themeAudio = value;
    }

    public CutsceneManager CutsceneManager
    {
        get
        {

            return _cutsceneManager;
        }
        set => _cutsceneManager = value;
    }
    public InGameUIManager InGameUIManager
    {
        get => _inGameUIManager;
        set => _inGameUIManager = value;
    }
    [SerializeField]
    private int numberOfWinStage = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 🔥 Khi scene mới được load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1.0f;
    
        if (InGameUIManager != null)
        {
            InGameUIManager.InitData();
        }


    }

    // 🧩 Hàm kiểm tra scene hiện tại có phải stage hợp lệ không


    public void WinGame()
    {
        int index = stageNames.IndexOf(SceneManager.GetActiveScene().name);
        if (numberOfWinStage == index)
        {
            numberOfWinStage++;
            StartMoney.Instance.AddMoney(13);
        }
    }

    public void GameOver() { }

    public void LoadScene(string sceneName)
    {
        LoadingScene.LoadScene(sceneName);
    }

    public void ReloadScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToNextScene()
    {
        if (numberOfWinStage == stageNames.Count)
        {
            // game đã phá đảo
        }
        else
        {
            LoadScene("WaitScene");
        }
    }

    public bool CheckValidScene(string name)
    {
        var existName = stageNames.Find(x => x.Equals(name));
        if (String.IsNullOrEmpty(existName))
        {
            Debug.LogError("Scene name is not exist in GameManager stageNames list!");
        }
        else
        {
            int index = stageNames.IndexOf(existName);
            if (index > numberOfWinStage)
                return false;
        }
        return true;
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Nếu đang chạy trong Editor, dừng Play Mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Nếu là bản build (PC, WebGL, Mobile), thoát ứng dụng
        Application.Quit();
#endif
    }
}
