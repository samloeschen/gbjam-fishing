using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {
    public new Transform transform;
    public float speed;
    public Vector3 axis;

    void OnValidate() {
        if (!transform) {
            transform = GetComponent<Transform>();
        }
    }
    void OnEnable() {
        if (!transform) {
            this.enabled = false;
        }
    }
    void Update() {
        transform.Rotate(axis * speed * Time.deltaTime, Space.Self);
    }
}
