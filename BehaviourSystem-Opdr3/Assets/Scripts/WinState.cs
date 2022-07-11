using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinState : MonoBehaviour {

    // Check if all enemies are dead and then load win screen
    private void Update() {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0) {
            SceneManager.LoadScene("Win");
        }
    }
}
