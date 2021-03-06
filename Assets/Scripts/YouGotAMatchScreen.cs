﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class YouGotAMatchScreen : MonoBehaviour {
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer profileSpriteRenderer;
    public TextMeshPro messageTextMeshPro;
    public TextMeshPro nameTextMeshPro;
    public CharArray messageCharArray;
    public CharArray nameCharArray;
    public PhoneManager phoneManager;

    public ParticleSystem successParticles;
    public BobberAimBehaviour bobberAimBehaviour;

    public event Action<CatchScreenAnimationEvent> onAnimationEvent;

    bool _goToPhone;

    static readonly string[] numericSuffixes = {
        "th",
        "st",
        "nd",
        "rd",
        "th",
    };

    const string message = " time's the charm!";

    string GetNumericSuffix(int number) {
        int index = (int)Mathf.Clamp(number % 10, 0, numericSuffixes.Length);
        return numericSuffixes[index];
    }

    void Awake() {
        messageCharArray = new CharArray(256);
        spriteRenderer.enabled = false;
        profileSpriteRenderer.enabled = false;
        messageTextMeshPro.enabled = false;

        onAnimationEvent += (CatchScreenAnimationEvent e) => {
            switch(e) {
                case CatchScreenAnimationEvent.CatchAnimationComplete:
                if (_goToPhone) {
                    phoneManager.ShowPhone(PhoneScreen.NewMatch);
                } else {
                    bobberAimBehaviour.enabled = true;
                }
                break;
            }
        };
    }

    public void ShowSuccess(FishDataObject fish, bool goToPhone = false) {
        bobberAimBehaviour.enabled = false;
        animator.SetTrigger("ShowSuccess");
        profileSpriteRenderer.sprite = fish.data.profileSprite;
        messageCharArray.Clear();
        messageCharArray.Append(fish.data.saveData.numberCaught);
        messageCharArray.Append(GetNumericSuffix(fish.data.saveData.numberCaught));
        messageCharArray.Append(message);
        messageTextMeshPro.SetCharArray(messageCharArray.GetArray(), 0, messageCharArray.count);

        // nameCharArray.Clear();
        // nameCharArray.Append(fish.data.name);
        _goToPhone = goToPhone;
    }

    public void ShowFail() {
        _goToPhone = false;
        bobberAimBehaviour.enabled = false;
        animator.SetTrigger("ShowFail");
    }

    public FishDataObject testFishObject;
    public void CatchScreenAnimationHook(CatchScreenAnimationEvent e) {
        if (onAnimationEvent != null) { onAnimationEvent(e); }
    }

    public void PlayParticles() {
        successParticles.Play();
    }
}


public enum CatchScreenAnimationEvent {
    CatchAnimationComplete
}
