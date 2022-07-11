using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour {

    [SerializeField] private float speed = 15f;

    private Transform player;
    private Vector3 target;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        target = new Vector3(player.position.x, 0, player.position.z);
    }

    //Moves the object
    private void FixedUpdate() {
        Vector3 velocity = this.transform.forward * speed;
        this.transform.position = this.transform.position + velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")) {
            DestroyObject();
        } else if(other.CompareTag("Environment")) {
            DestroyObject();
        }
    }

    //Destoyrs object
    private void DestroyObject() {

        Destroy(gameObject);
    }
}
