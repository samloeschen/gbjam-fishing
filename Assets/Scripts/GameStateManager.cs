using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FullSerializer;

[CreateAssetMenu(fileName = "NewGameStateManager", menuName = "ScriptableObjects/GameStateManager")]
public class GameStateManager: ScriptableObject {
    public GameState gameState;
    public FishDataObjectList allFish;

    public bool LoadGame(out GameState gameSaveData) {
        gameSaveData = default(GameState);
        if (PlayerPrefs.HasKey(PLAYER_PREFS_KEY)) {
            string json = PlayerPrefs.GetString(PLAYER_PREFS_KEY);
            gameSaveData = (GameState)StringSerializationAPI.Deserialize(typeof(GameState), json);
            if (gameSaveData.serializedFishDataDict == null) {
                gameSaveData.serializedFishDataDict = new Dictionary<string, SerializedFishData>(allFish.data.Count);
            }
            ApplySerializedFishData(allFish, gameState);
            return true;
        }
        return false;
    }


    public void ApplySerializedFishData(FishDataObjectList masterList, GameState gameState) {
        var fishList = masterList.data;
        for (int i = 0; i < fishList.Count; i++) {
            if (gameState.serializedFishDataDict.TryGetValue(fishList[i].name, out var saveData)) {
                fishList[i].data.saveData = saveData;
            }
        }
    }

    public void SerializeFishData(FishDataObjectList masterList, GameState gameState) {
        FishData fishData;
        for (int i = 0; i < masterList.data.Count; i++) {
            fishData = masterList.data[i].data;
            var serializedData = fishData.saveData;
            if (gameState.serializedFishDataDict.ContainsKey(fishData.name)) {
                gameState.serializedFishDataDict[fishData.name] = serializedData;
            } else {
                gameState.serializedFishDataDict.Add(fishData.name, serializedData);
            }
        }
    }

    public bool LoadGame(out GameState gameState, GameState fallback) {
        gameState = default(GameState);
        if (LoadGame(out gameState)) {
            return true;
        } else {
            gameState = new GameState();
            gameState.Copy(fallback);
            return false;
        }
    }

    public bool LoadGame() {
        return LoadGame(out this.gameState, GameState.CreateDefault());
    }
    public const string PLAYER_PREFS_KEY = "GameSave";
    public void SaveGame() {
        string json = StringSerializationAPI.Serialize(typeof(GameState), gameState);
        PlayerPrefs.SetString(PLAYER_PREFS_KEY, json);
#if UNITY_EDITOR
        Debug.Log("Saved game to PlayerPrefs");
#endif
    }

}

[System.Serializable]
public struct GameState {
    public string lastEnvironmentName;
    public string lastBaitName;
    public Dictionary<string, SerializedFishData> serializedFishDataDict;
    public void Copy(GameState other) {
        lastEnvironmentName = string.Copy(other.lastEnvironmentName);
        serializedFishDataDict = new Dictionary<string, SerializedFishData>(other.serializedFishDataDict);
    }
    public static GameState CreateDefault() {
        return new GameState {
            lastEnvironmentName = "",
            serializedFishDataDict = new Dictionary<string, SerializedFishData>(32),
        };
    }
}
