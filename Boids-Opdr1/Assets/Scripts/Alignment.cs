using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alignment : MonoBehaviour {
    private BoidManager brain;
    private Vector2 sumOfAllBoidVelocities;
    private float sumOfAllBoidSpeeds;

    private void Start() {
        brain = GetComponent<BoidManager>();
    }

    // Update is called once per frame
    void Update() {
        sumOfAllBoidVelocities = new Vector2();
        sumOfAllBoidSpeeds = 0;
        foreach (Boid boid in brain.boids) {
            sumOfAllBoidVelocities += boid.direction;
            sumOfAllBoidSpeeds += boid.speed;
        }
    }

    public Vector2 GetVelocityMatchVector(Boid currentBoid, out float averageSpeed) {

        Vector2 averageVelocityOfBoidsEceptCurrentBoid = (sumOfAllBoidVelocities - currentBoid.direction) / (brain.boids.Length - 1);
        averageSpeed = (sumOfAllBoidSpeeds - currentBoid.speed) / (brain.boids.Length - 1);

        return averageVelocityOfBoidsEceptCurrentBoid.normalized;

    }
}
