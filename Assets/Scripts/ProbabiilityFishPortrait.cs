using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProbabiilityFishPortrait : MonoBehaviour {

    [Range(0f, 1f)]
    public float probability;
    public Sprite mysterySprite;
    public SpriteRenderer portraitSpriteRenderer;
    public Sprite unlockSprite;
    public Sprite lockSprite;
    public SpriteRenderer lockStateSpriteRenderer;
    public TextMeshPro probabilityTMPro;
    public CharArray charArray;

    public void SetFishDataObject(FishDataObject fishDataObject, GameState gameState, bool animate = true) {
        if (gameState.unlockedFishNames.Contains(fishDataObject.data.name)) {
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

    void Update () {
        // update text
        int textProbability = (int)(probability * 100);
        charArray.Clear();
        if (textProbability < 10) {
            charArray.Append(0);
        }
        charArray.Append(textProbability);
        charArray.Add('%');

        probabilityTMPro.SetCharArray(charArray.GetArray(), 0, charArray.count);
    }
}
