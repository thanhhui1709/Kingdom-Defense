using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
   


    public void OnStartClicked()
    {
        LoadingScene.LoadScene("WaitScene");
    }
}
