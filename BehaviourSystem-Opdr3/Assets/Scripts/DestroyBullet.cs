using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBullet : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Environment")) {
            DestroyShuriken();
        } 
        
        else if (other.CompareTag("Enemy")) {
            DestroyShuriken();
        } else {
            Object.Destroy(gameObject, 10.0f);
        }
    }

    // Destroys the fired object
    private void DestroyShuriken() {
        Destroy(gameObject);
    }
}
