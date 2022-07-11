using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    
    internal float speed;
    internal Vector2 direction;
    internal Vector2 position {
        get {
            return new Vector2(transform.position.x, transform.position.y);
        }
    }

    //create paths
    public int numberOfPathsToCreate;
    internal Vector3[] possiblePathVectors;

    internal float speedMultiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        if(numberOfPathsToCreate % 2 == 0) numberOfPathsToCreate++;
        possiblePathVectors = new Vector3[numberOfPathsToCreate];
        possiblePathVectors[0] = Vector3.right;


        float piFraction = 1.618033f;
        float piStart = Mathf.PI / 2;
        float angle = 0;
        for (int i = 1; i < numberOfPathsToCreate; i+=2) {

            angle = piStart + (Mathf.PI * piFraction * i);
            possiblePathVectors[i] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);

            angle = piStart - (Mathf.PI * piFraction * i);
            possiblePathVectors[i+1] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        for(int i =0; i<numberOfPathsToCreate;i++)
            Debug.DrawRay(transform.position, transform.TransformVector(possiblePathVectors[i]).normalized * 2, Color.red);

        transform.Translate(new Vector3(direction.x,direction.y) * Time.deltaTime * speed * speedMultiplier, Space.World);
        transform.right = direction;
    }

    // The move direction of the boid
    public void AdjustVelocityBy(Vector2 velocityChange, float bias) {
        direction =  Vector3.Lerp(direction, velocityChange, bias);
        direction = direction.normalized;
    }

    // The speed of the boid
    public void AdjustSpeedBy(float targetSpeed, float bias) {
        speed = (speed * (1 - bias)) + (targetSpeed * bias);
    }

}
