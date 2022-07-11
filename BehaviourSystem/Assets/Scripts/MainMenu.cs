using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    // Load the game if play button is pressed
    public void LoadGame() {
        SceneManager.LoadScene("Main");
    }


    // Quit the game
    public void QuitGame() {
        Application.Quit();
    }
}