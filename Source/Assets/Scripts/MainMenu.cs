using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Game"); // begins the checkers game

    }

    public void QuitGame()
    {
        Application.Quit();

        // for unity testing
        Debug.Log("Quit");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu"); // returns to main menu

    }
}
