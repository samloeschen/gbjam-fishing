﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFishDataObject", menuName = "ScriptableObjects/FishDataObject")]
public class FishDataObject : RefObject<FishData> { }

[System.Serializable]
public struct FishData {
    public string name;
    public Sprite profileSprite;

    [TextArea, Tooltip("The text that shows up on the fish's profile")]
    public string profileText;

    [Range(0, 100), Tooltip("The base catch percentage of the fish")]
    public float basePercentage;

    [Tooltip("Time ranges that this fish can appear during. Represented in 24 hour time")]
    public List<TimeRange> timeRanges;

    [Tooltip("The list of the fish's favorite baits")]
    public List<BaitBoostData> favoriteBait;

    [System.NonSerialized]
    public SerializedFishData saveData;
}

[System.Serializable]
public struct BaitBoostData {
    public BaitDataObject baitDataObject;
    [Range(0, 100)]
    public float percentageBoost;
}

[System.Serializable]
public struct SerializedFishData {
    public bool unlocked;
    public int numberCaught;
    public int numberMissed;
    public int timeFirstCaughtHours;
    public int timeFirstCaughtMinutes;
}

[System.Serializable]
public struct TimeRange {
    public int min;
    public int max;
    public bool ContainsTime(int value) {
        if (min <= max) {
            return value >= min && value <= max;
        } else {
            return value >= min || value <= max;
        }
    }
}