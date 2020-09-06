using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaitManager : MonoBehaviour {
    public List<BaitDataObject> baitDataObjects;
    [HideInInspector] public BaitDataObject selectedBait;
    [HideInInspector] public int selectedBaitIndex;
    public SpriteRenderer spriteRenderer;
    public Animator baitParentAnimator;
    public Animator leftArrowAnimator;
    public Animator rightArrowAnimator;
    public Animator baitSpriteAnimator;
    public AudioSource baitAudioSource;

    public void Show() {
        this.enabled = true;
        baitParentAnimator.SetTrigger("Show");
    }

    public void Hide() {
        baitParentAnimator.SetTrigger("Hide");
        this.enabled = false;
    }

    public void Initialize(GameState gameState) {
        for (int i = 0; i < baitDataObjects.Count; i++) {
            if (baitDataObjects[i].data.name == gameState.lastBaitName) {
                SelectBait(i);
                break;
            }
        }
    }

    public void SelectBait(int index) {
        index = (int)Mathf.Repeat(index, baitDataObjects.Count);
        selectedBaitIndex = index;
        selectedBait = baitDataObjects[index];
        spriteRenderer.sprite = selectedBait.data.uiSprite;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            SelectBait(selectedBaitIndex - 1);
            leftArrowAnimator.SetTrigger("Press");
            baitSpriteAnimator.SetTrigger("Press");
            baitAudioSource.clip = selectedBait.data.oneShot;
            baitAudioSource.Play();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            SelectBait(selectedBaitIndex + 1);
            rightArrowAnimator.SetTrigger("Press");
            baitSpriteAnimator.SetTrigger("Press");
            baitAudioSource.clip = selectedBait.data.oneShot;
            baitAudioSource.Play();
        }
    }
}
