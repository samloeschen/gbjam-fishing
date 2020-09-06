using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodBehaviour : MonoBehaviour {
    public Animator animator;
    public RodState currentState;

    [Header("Bobber")]
    public BobberBehaviour bobberBehaviour;
    public Animator bobberAnimator;

    public event Action<RodAnimationEvent> onAnimationEvent;

    // [HideInInspector]
    public Vector2 bobberPosAnim = Vector2.zero;
    [HideInInspector]
    public float bobberScaleAnim = 1f;

    [Header("SFX")]
    public AudioClip castOneShot;
    public AudioClip reelOneShot;
    public AudioClip tugOneShot;

    public AudioSource bigBiteLoopSource;

    void OnEnable() {
        currentState = RodState.Idle;
        HandleInput(RodAction.Idle);
    }

    public void HandleInput(RodAction action) {
        switch (action) {
            case RodAction.Cast:
            OneShotManager.PlayOneShot(castOneShot);
            animator.SetTrigger("CastStart");
            break;

            case RodAction.SmallBite:
            OneShotManager.PlayOneShot(tugOneShot);
            animator.SetTrigger("SmallBite");
            break;

            case RodAction.BigBite:
            bigBiteLoopSource.Play();
            animator.SetTrigger("BigBite");
            break;

            case RodAction.Reel:
            bigBiteLoopSource.Stop();
            OneShotManager.PlayOneShot(reelOneShot);
            animator.SetTrigger("ReelStart");
            break;

            case RodAction.Idle:
            break;
        }
    }

    public void Start() {
        onAnimationEvent += (RodAnimationEvent e) => {
            switch (e) {
                
                case RodAnimationEvent.CastStart:
                currentState = RodState.Casting;
                bobberAnimator.SetTrigger("Reset");
                break;
                
                case RodAnimationEvent.CastEnd:
                currentState = RodState.WaitingForBite;
                bobberAnimator.SetTrigger("HitWater");
                break;
                
                case RodAnimationEvent.BiteStart:
                break;
                
                case RodAnimationEvent.BiteEnd:
                break;
                
                case RodAnimationEvent.ReelStart:
                currentState = RodState.Reeling;
                bobberAnimator.SetTrigger("Reel");
                bobberBehaviour.isInWater = false;
                break;
                
                case RodAnimationEvent.ReelEnd:
                currentState = RodState.Idle;
                break;
            }
        };
    }

    public void Update() {
    }

    public void AnimationEventHook(RodAnimationEvent callback)  {
        Debug.Log(callback);
        if (onAnimationEvent != null) {   
            onAnimationEvent(callback);
        }
    }
}


public enum RodAnimationEvent {
    CastStart, CastEnd, BiteStart, BiteEnd, ReelStart, ReelEnd
}

public enum RodState {
    Idle, Casting, WaitingForBite, Reeling,
}

public enum RodAction {
    Idle, Cast, SmallBite, BigBite, Reel,
}
