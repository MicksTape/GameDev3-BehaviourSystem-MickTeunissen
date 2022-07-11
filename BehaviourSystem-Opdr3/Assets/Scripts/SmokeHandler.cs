using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start() {
        Object.Destroy(gameObject, 10.0f);
    }


}
