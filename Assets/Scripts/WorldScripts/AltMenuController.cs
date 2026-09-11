using UnityEngine;
using UnityEngine.SceneManagement;

public class AltMenuController : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("TitleScreen");
        Debug.Log("Pressed and loading");
        //SceneManager.LoadScene("UI", LoadSceneMode.Additive);
    }

    public void OnQuitClick()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }
}