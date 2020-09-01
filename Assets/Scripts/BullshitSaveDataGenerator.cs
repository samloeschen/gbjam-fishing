using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullshitSaveDataGenerator : MonoBehaviour {
    public GameStateManager gameStateManager;

    public FishDataObjectList allFish;
    public FishDataObjectList targetFish;

    public void GenerateBullshitSaveData() {
        for (int i = 0; i < targetFish.data.Count; i++) {
            targetFish.data[i].data.saveData.unlocked = true;
        }
    }

    public void ClearSaveData() {
        for (int i = 0; i < allFish.data.Count; i++) {
            allFish.data[i].data.saveData = default(SerializedFishData);
        }
    }
}
