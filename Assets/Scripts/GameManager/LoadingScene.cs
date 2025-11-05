using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [Header("UI Elements")]
    // THAY THẾ: Dùng 'Image' thay vì 'Slider'
    [SerializeField] private Image progressCircle;

    [Header("Settings")]
    [SerializeField] private float minLoadTime = 2.0f;

    private static string nextSceneToLoad;

    public static void LoadScene(string sceneName)
    {
        nextSceneToLoad = sceneName;
        SceneManager.LoadSceneAsync("LoadScene");
    }

    private void Start()
    {
        // Ẩn thanh progress lúc đầu
        if (progressCircle != null)
            progressCircle.fillAmount = 0; // Sửa

        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        float startTime = Time.time;
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(nextSceneToLoad);
        asyncOp.allowSceneActivation = false;

        while (asyncOp.progress < 0.9f)
        {
           
            if (progressCircle != null)
                progressCircle.fillAmount = asyncOp.progress;
        
            yield return null;
        }

        // Đã tải xong (ít nhất là 90%)
        if (progressCircle != null)
            progressCircle.fillAmount = 1.0f; 

        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minLoadTime)
        {
            yield return new WaitForSeconds(minLoadTime - elapsedTime);
        }

        asyncOp.allowSceneActivation = true;
    }

}
