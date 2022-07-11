using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviour {

    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private AudioSource fireSound;

    public float damage;
    [SerializeField] private float speed = 5f;
    private bool mouseButtonHeldDown;

    private void Update() {
        // Check if the mouse button is held down
        if (Input.GetMouseButtonDown(0)) {
            mouseButtonHeldDown = true;
        }

        // Shoots when mouse button is released
        if (Input.GetMouseButtonUp(0)) {
            Shoot();
            fireSound.Play();
            mouseButtonHeldDown = false;
        }

    }

    // Shoot a shuriken at the enemy
    private void Shoot() {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * speed, ForceMode.Impulse);
    }

    // This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
