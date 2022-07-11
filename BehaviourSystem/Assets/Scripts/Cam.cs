using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour {

    public Transform target;
    public float distance = 5;

	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {

        Vector3 modifiedPos = target.position - (transform.forward * distance);

        transform.position = modifiedPos;
	}
}
