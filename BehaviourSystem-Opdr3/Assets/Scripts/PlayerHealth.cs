using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour {

    [Header("Health")]
	[SerializeField] private Image healthBar;
	[SerializeField] private AudioSource takeHitSound;
	[SerializeField] private UnitEnemy unitGuard;
    private Material safeMat;

	[SerializeField] private float currentHealth;
	private float currentHealthValue;
	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private float lerpSpeed = 10f;

    [SerializeField] private UnitAlly unitAlly;

    public float CurrentHealth {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    private void Start() {
		currentHealth = maxHealth;

    }

    private void Update() {
        if (healthBar != null) {
            //player health
            currentHealthValue = Map(CurrentHealth, 0, maxHealth, 0, 1);
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealthValue, Time.deltaTime * lerpSpeed);
        }

        // Dies
		if (CurrentHealth <= 0) {
        	SceneManager.LoadScene("Lose");
        }
    }

    // Takes damage
	private void OnTriggerEnter(Collider other) {
		if(other.CompareTag("Blade")) {
			CurrentHealth -= unitGuard.slashDamage;
			takeHitSound.Play();
            unitAlly.playerInDanger = true;

		}
	}


    // This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
