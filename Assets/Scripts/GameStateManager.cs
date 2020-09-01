using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FullSerializer;

[CreateAssetMenu(fileName = "NewGameStateManager", menuName = "ScriptableObjects/GameStateManager")]
public class GameStateManager: ScriptableObject {
    public GameState gameState;
    public static string SaveDir;
    public static string SavePath;
    public FishDataObjectList allFish;

    void OnEnable() {
        SaveDir = Application.persistentDataPath + "/SAVES";
        SavePath = SaveDir + "/save.json";
    }
    public bool LoadGame(out GameState gameSaveData) {

        string SAVE_DIR = Application.persistentDataPath + "/SAVES";
        string SAVE_PATH = SAVE_DIR + "/save.json";

        gameSaveData = default(GameState);
        if (File.Exists(SAVE_PATH)) {
            string json = File.ReadAllText(SAVE_PATH);
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
            string json = File.ReadAllText(SavePath);
            gameState = (GameState)StringSerializationAPI.Deserialize(typeof(GameState), json);
            if (gameState.serializedFishDataDict == null) {
                gameState.serializedFishDataDict = new Dictionary<string, SerializedFishData>(32);
            }
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
    
    public void SaveGame() {
        string json = StringSerializationAPI.Serialize(typeof(GameState), gameState);
        if (!Directory.Exists(SaveDir)) {
            Directory.CreateDirectory(SaveDir);
        }
        File.WriteAllText(SavePath, json, System.Text.Encoding.UTF8);
#if UNITY_EDITOR
        Debug.Log("Saved to " + SavePath);
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
