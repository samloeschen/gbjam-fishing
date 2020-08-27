using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {
    public new Rigidbody2D rigidbody2D;
    void OnValidate() {
        if (!rigidbody2D) {
            rigidbody2D = GetComponent<Rigidbody2D>();
        }
    }

    void OnEnable() {
        if (!rigidbody2D) {
            rigidbody2D = GetComponent<Rigidbody2D>();
            if (!rigidbody2D) {
                this.enabled = false;
            }
        }
    }

    public void Move(Vector2 delta) {
         rigidbody2D.MovePosition(rigidbody2D.position + delta);
    }
}
