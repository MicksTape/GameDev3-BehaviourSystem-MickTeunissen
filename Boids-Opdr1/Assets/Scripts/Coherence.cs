using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cohesion : MonoBehaviour
{
    public GameObject coherencePointVisual;

    private BoidManager manager;
    private Vector2 sumOfAllBoidPositions;

    private void Start()
    {
        manager = GetComponent<BoidManager>();
    }

    // Update is called once per frame
    void Update()
    {
        sumOfAllBoidPositions = new Vector2();
        foreach (Boid boid in manager.boids)
        {
            sumOfAllBoidPositions += boid.position;
        }
    }


    public Vector2 GetCoherence(Boid currentBoid){

        Vector2 centerOfMassOfBoidsEceptCurrentBoid = (sumOfAllBoidPositions - currentBoid.position) / (manager.boids.Length - 1);
        coherencePointVisual.transform.position = centerOfMassOfBoidsEceptCurrentBoid;
        return (centerOfMassOfBoidsEceptCurrentBoid - currentBoid.position).normalized;

    }
}
