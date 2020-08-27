using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberAimBehaviour: MonoBehaviour {
    public CircleMover reticleMover;
    public RodBehaviour rodBehaviour; 
    public BobberBehaviour bobberBehaviour;
    public float moveSpeed;
    void FixedUpdate() {
        Vector2 delta = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) {
            delta += Vector2.up;
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            delta += Vector2.down;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            delta += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            delta += Vector2.right;
        }

        // pick spot
        if (Input.GetKeyDown(KeyCode.X)) {
            bobberBehaviour.targetPosition = reticleMover.position;
            rodBehaviour.HandleInput(RodAction.Cast);
        }
        delta = delta.normalized * Time.deltaTime * moveSpeed;
    }
}
