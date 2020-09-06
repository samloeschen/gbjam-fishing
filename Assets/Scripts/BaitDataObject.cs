using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBaitDataObject", menuName = "ScriptableObjects/BaitDataObject")]
public class BaitDataObject : RefObject<BaitData> { }

[System.Serializable]
public struct BaitData {
    public string name;
    public Sprite uiSprite;
    public AudioClip oneShot;
}

