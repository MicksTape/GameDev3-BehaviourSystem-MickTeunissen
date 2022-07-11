using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{

    public float collisionRaycastDistance = 2f;

    public Vector2 GetSoftColissionAvoidance(Boid current)
    {
        //for each vector in Possible path vectors
        foreach (Vector3 directionVector in current.possiblePathVectors)
        {
            //If no obstacle in directionVector
            if (!Physics.Raycast(current.transform.position, current.transform.TransformVector(directionVector), collisionRaycastDistance))
            {
                //modify direction to direction vector
                return current.transform.TransformVector(directionVector).normalized;
            }
        }

        // no way to go
        return new Vector2();
    }


    public Vector2 GetHardColissionAvoidance(Boid current, out float bias)
    {
        bias = 0;
        RaycastHit hit;

        //Checks if there is hard collision in the current path
        if (Physics.Raycast(current.transform.position, current.direction, out hit, collisionRaycastDistance, 1 << LayerMask.NameToLayer("HardCollision")))
        {
            //If in danger of collision increase the bias to prevent collision
            bias = 1 - (hit.distance / collisionRaycastDistance);
            //for each vector in Possible path vectors
            foreach (Vector3 directionVector in current.possiblePathVectors)
            {
                
                //if no obstacle in direction vector
                if (!Physics.Raycast(current.transform.position, current.transform.TransformVector(directionVector), collisionRaycastDistance,1 << LayerMask.NameToLayer("HardCollision")))
                {
                    //to the direction
                    return current.transform.TransformVector(directionVector).normalized;
                }
            }
        }

        // no way to go
        return new Vector2();
    }


}
