using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUIHandler : MonoBehaviour{

    public Transform target;
 
    // Move the position of the health UI to the unit position
    void Update() {
        if (target != null) {
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
            
            if(target.GetComponent<UnitEnemy>() != null) {
                if (target.GetComponent<UnitEnemy>().CurrentHealth <= 0) {
                    Destroy(gameObject, .5f);
                }
            }
        }
    }
}
