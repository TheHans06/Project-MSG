using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Save Slots");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings Scene");
    }

    public void QuitGame()
    {
        Debug.Log("Game berhasil ditutup!");
        Application.Quit();
    }
}
