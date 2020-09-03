using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProbabiilityFishPortrait : MonoBehaviour {

    [Range(0f, 1f)]
    public float probability;
    public float lerpSpeed;

    [System.NonSerialized] public float targetProbability;

    public Sprite mysterySprite;
    public SpriteRenderer portraitSpriteRenderer;
    public Sprite unlockSprite;
    public Sprite lockSprite;
    public SpriteRenderer lockStateSpriteRenderer;
    public TextMeshPro probabilityTMPro;
    public CharArray charArray;    
    public FishDataObject nextFishObject;

    public event Action<PortraitAnimationEvent> onAnimationEvent;
    [System.NonSerialized] public GameStateManager gameStateManager;
    public Animator animator;

    public void SetFishDataObject(FishDataObject fishDataObject, bool animate = false) {
        if (animate) {
            nextFishObject = fishDataObject;
            animator.SetTrigger("ChangeFish");
            return;
        }
        var gameState = gameStateManager.gameState;
        if (fishDataObject.data.saveData.unlocked) {
            portraitSpriteRenderer.sprite = fishDataObject.data.profileSprite;
            lockStateSpriteRenderer.sprite = unlockSprite;
        } else {
            portraitSpriteRenderer.sprite = mysterySprite;
            lockStateSpriteRenderer.sprite = lockSprite;
        }
    }

    void Awake() {
        charArray = new CharArray(4);
    }

    void Start() {
        onAnimationEvent += (PortraitAnimationEvent e) => {
            switch (e) {
                case PortraitAnimationEvent.ChangeFishObject:
                    SetFishDataObject(nextFishObject, animate: false);
                break;
            }
        };
    }

    void Update () {
        // update text
        probability = Mathf.Lerp(probability, targetProbability, Time.deltaTime * lerpSpeed);
        if (Mathf.Abs(probability - targetProbability) * 100 < 1f) {
            probability = targetProbability;
        }

        int textProbability = (int)Mathf.Clamp(probability * 100f, 0f, 99f);
        charArray.Clear();
        if (textProbability < 10) {
            charArray.Append(0);
        }
        charArray.Append(textProbability);
        charArray.Append('%');

        probabilityTMPro.SetCharArray(charArray.GetArray(), 0, charArray.count);
    }

    public void AnimationEventHook(PortraitAnimationEvent e) {
        if (onAnimationEvent != null) {
            onAnimationEvent(e);
        }
    }
}

public enum PortraitAnimationEvent {
    ChangeFishObject
}
