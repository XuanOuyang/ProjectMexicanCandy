using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private GameObject howToPlayPanel;

    public void OnStartClick()
    {
        SceneManager.LoadScene("PlayerInitialization");
        //SceneManager.LoadScene("UI", LoadSceneMode.Additive);
    }

    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void OnHowToPlayClick()
    {
        howToPlayPanel.SetActive(true);
    }
}