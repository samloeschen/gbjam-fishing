﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static FishManager;
public class BobberAimBehaviour: MonoBehaviour {
    public CircleMover reticleMover;
    public RodBehaviour rodBehaviour; 
    public BobberBehaviour bobberBehaviour;
    public FishManager fishManager;
    
    public float moveSpeed;


    [Header("Mash Indicator")]
    public Animator buttonIndicatorAnimator;
    public Vector2 mashIndicatorOffset;



    [Header("Catching")]
    public float mashDecayRate;
    public float mashPressValue;
    public float mashMaxDuration;
    public bool mashMode;
    float _currentMashValue;
    float _mashTimer;

    float _reelTimer;


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
        reticleMover.Move(delta * Time.deltaTime);
        delta = delta.normalized * Time.deltaTime * moveSpeed;
    }

    void Update () {

        if (mashMode) {
            _currentMashValue -= mashDecayRate * Time.deltaTime;
            _currentMashValue = Mathf.Max(_currentMashValue, 0f);
            _mashTimer -= Time.deltaTime;
            if (_mashTimer <= 0f) {
                FailCatch();
            }
        }

        if (_reelTimer > 0f) {
            _reelTimer -= Time.deltaTime;
            if (_reelTimer <= 0f) {
                DoReel();
            }
        }

        // pick spot
        if (Input.GetKeyDown(KeyCode.X)) {
            if (mashMode) {
                _currentMashValue += mashPressValue;
                if (_currentMashValue > 1f) {
                    CompleteCatch();
                }
            } 
            else if (rodBehaviour.currentState == RodState.Idle) {
                rodBehaviour.HandleInput(RodAction.Cast);
                reticleMover.gameObject.SetActive(false);
                bobberBehaviour.targetPosition = reticleMover.position;
            }
            else if (rodBehaviour.currentState == RodState.WaitingForBite) {
                var biteResult = fishManager.TryGetBite();
                switch (biteResult) {
                    case BiteResult.Invalid:
                    break;

                    case BiteResult.SmallBiteSuccess:
                    buttonIndicatorAnimator.SetTrigger("GoodPress");
                    rodBehaviour.HandleInput(RodAction.SmallBite);
                    break;

                    case BiteResult.BiteFail:
                    buttonIndicatorAnimator.SetTrigger("BadPress");
                    rodBehaviour.HandleInput(RodAction.SmallBite);
                    break;

                    case BiteResult.BigBiteSuccess:
                    mashMode = true;
                    _currentMashValue = 0f;
                    _mashTimer = mashMaxDuration;
                    buttonIndicatorAnimator.SetTrigger("MashTime");
                    rodBehaviour.HandleInput(RodAction.BigBite);
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            buttonIndicatorAnimator.SetTrigger("Reset");
            if (rodBehaviour.currentState == RodState.WaitingForBite) {
                rodBehaviour.HandleInput(RodAction.Reel);
                fishManager.HandleReel();
            }
            reticleMover.gameObject.SetActive(true);
        }
    }

    void FailCatch() {
        bobberBehaviour.isInWater = false;
        mashMode = false;
        buttonIndicatorAnimator.SetTrigger("BadPress");
        fishManager.EndBiteSequence(BiteResult.BiteFail);
        DoReel(delay: 0.35f);
    }

    void CompleteCatch() {
        bobberBehaviour.isInWater = false;
        mashMode = false;
        fishManager.EndBiteSequence(BiteResult.BigBiteSuccess);
        DoReel();
    }

    void DoReel(float delay = 0f) {
        if (delay > 0f) {
            _reelTimer = delay;
            return;
        }
        if (rodBehaviour.currentState == RodState.WaitingForBite) {
            rodBehaviour.HandleInput(RodAction.Reel);
            fishManager.HandleReel();
        }
        buttonIndicatorAnimator.SetTrigger("Reset");
        reticleMover.gameObject.SetActive(true);
    }

    void StartMashMode() {
        mashMode = true;
        _currentMashValue = 0f;
        _mashTimer = mashMaxDuration;
    }
}
