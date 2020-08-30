using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberBehaviour : MonoBehaviour {
    public Transform physicsTransform;
    public Transform anchorTransform;
    public Transform spriteTransform;
    public Vector3 targetPosition;
    public float physicsT;
    public Vector2 castT;
    public new Rigidbody2D rigidbody2D;
    public Animator animator;

    public bool isInWater = false;
    public Vector2 position => rigidbody2D.position;
    public event Action<BobberAnimationEvent> animationEventCallback;

    void Start() {
        animationEventCallback += (BobberAnimationEvent e) => {
            switch (e) {
            case BobberAnimationEvent.SplashComplete:
                this.isInWater = true;
            break;

            case BobberAnimationEvent.ReelStart:
                this.isInWater = false;
            break;
            }
        };
    }

    public void AnimationEventCallbackHook(BobberAnimationEvent e) {
        if (animationEventCallback != null) {
            animationEventCallback(e);
        }
    }

    void Update() {
        Vector3 rootPos = Vector3.Lerp(physicsTransform.position, anchorTransform.position, physicsT);
        Vector3 currentCastPos = new Vector3 {
            x = Mathf.LerpUnclamped(rootPos.x, targetPosition.x, castT.x),
            y = Mathf.LerpUnclamped(rootPos.y, targetPosition.y, castT.y),
            z = transform.position.z
        };
        rigidbody2D.MovePosition(currentCastPos);
    }
    public void Spawn(GameObject gameObject) {
        PoolManager.PoolInstantiate(gameObject, spriteTransform.position + gameObject.transform.position, gameObject.transform.rotation);
    }

    public enum BobberAnimationEvent {
        SplashComplete, ReelStart
    }
}
