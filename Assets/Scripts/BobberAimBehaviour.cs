using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberAimBehaviour: MonoBehaviour {
    public CircleMover reticleMover;
    public RodBehaviour rodBehaviour; 
    public BobberBehaviour bobberBehaviour;
    public FishManager fishManager;
    
    public float moveSpeed;


    [Header("Mash Indicator")]
    public Animator mashIndicatorAnimator;
    public Vector2 mashIndicatorOffset;


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
            if (rodBehaviour.currentState == RodState.Idle) {
                rodBehaviour.HandleInput(RodAction.Cast);
                reticleMover.gameObject.SetActive(false);
                bobberBehaviour.targetPosition = reticleMover.position;
            }
            else if (rodBehaviour.currentState == RodState.WaitingForBite) {
                if (fishManager.TryCatchFish()) {
                    rodBehaviour.HandleInput(RodAction.Bite);
                } else {
                    rodBehaviour.HandleInput(RodAction.Reel);
                    reticleMover.gameObject.SetActive(true);
                }
            }
        }
        reticleMover.Move(delta * Time.deltaTime);
        delta = delta.normalized * Time.deltaTime * moveSpeed;
    }
}
