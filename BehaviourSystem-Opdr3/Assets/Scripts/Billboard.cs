﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

    Camera mainCamera;

    private void Start() {
        mainCamera = Camera.main;
    }

    // Makes it so that it faces the main camera
    private void Update() {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}
