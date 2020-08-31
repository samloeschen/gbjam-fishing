using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishProfileCell : MonoBehaviour {
    public Sprite lockedSprite;
    public Image image;

    // [System.NonSerialized]
    public FishDataObject fishDataObject;
    public void Initialize(FishDataObject fishDataObject) {
        this.fishDataObject = fishDataObject;
        image.sprite = fishDataObject.data.profileSprite;
        // image.sprite = fishDataObject.data.unlocked ? fishDataObject.data.profileSprite : lockedSprite;
    }
}
