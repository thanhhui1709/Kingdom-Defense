using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<string> stageNames;

    private int numberOfWinStage = 0;
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
        }
    }

    public void WinGame()
    {
        int index = stageNames.IndexOf(SceneManager.GetActiveScene().name);
        if (numberOfWinStage == index)
        {
            numberOfWinStage++;
        }
    }
    public void GameOver()
    {

    }
    public void LoadScene(string sceneName)
    {
        LoadingScene.LoadScene(sceneName);
    }
    public void ReloadScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
    public void GoToNextScene()
    {
        if (numberOfWinStage == stageNames.Count)
        {
            //game da pha dao,go to outro

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
}

