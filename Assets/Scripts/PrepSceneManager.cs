using UnityEngine;
using UnityEngine.SceneManagement;


public class PrepSceneManager : MonoBehaviour
{
    #region UI References
    public GameObject supplyGrid;
    public GameObject recipeGrid;
    public GameObject saveButton;
    public GameObject loadButton;
    public GameObject backButton;
    public GameObject quitButton;
    public GameObject playButton;
    public PrepManager prepManager;
    #endregion

    #region Switch Supply-Recipe
    public void showSupply() 
    {
        supplyGrid.SetActive(true);
        recipeGrid.SetActive(false);
    }

    public void showRecipe() 
    {
        supplyGrid.SetActive(false);
        recipeGrid.SetActive(true);
    }
    #endregion

    #region MENU
    public void backPrep()
    {
        SceneManager.LoadScene("Save Slots");
    }

    public void quitPrep()
    {
        Debug.Log("Game berhasil ditutup!");
        Application.Quit();
    }

    public void savePrep()
    {
        SceneManager.LoadScene("Save Slots");
    }

    public void loadPrep()
    {
        SceneManager.LoadScene("Save Slots");
    }

    public void playPrep()
    {
        SceneManager.LoadScene("Main Gameplay");
    }
    #endregion
}
