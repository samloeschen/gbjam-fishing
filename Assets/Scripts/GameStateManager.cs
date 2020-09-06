using System.IO;
using System.Runtime.InteropServices;
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

    [DllImport("__Internal")]
    private static extern void SyncFiles();

    [DllImport("__Internal")]
    private static extern void WindowAlert(string message);

    void OnEnable() {
        SaveDir = Application.persistentDataPath + "/SAVES";
        SavePath = SaveDir + "/save.json";
    }
    public bool LoadGame(out GameState gameSaveData) {
        gameSaveData = default(GameState);
        if (PlayerPrefs.HasKey("GameSave")) {
            string json = PlayerPrefs.GetString("GameSave");
            if (StringSerializationAPI.TryDeserialize<GameState>(json, out gameSaveData)) {
                return true;
            }
        }
        gameSaveData = default(GameState);
        return false;
    }
    
    public bool LoadGame(out GameState gameState, GameState fallback) {
        if (LoadGame(out gameState)) {
            return true;
        } else {
            gameState = GameState.Copy(fallback);
            return false;
        }
    }

    public bool LoadGame() {
        var result = LoadGame(out var gameState, GameState.CreateDefault());
        if (gameState.serializedFishDataDict == null) {
            gameState.serializedFishDataDict = new List<SerializedFishData>(allFish.data.Count);
        }
        if (gameState.serializedFishNames == null) {
            gameState.serializedFishNames = new List<string>(allFish.data.Count);
        }
        this.gameState = gameState;
        ApplySerializedFishData(allFish, this.gameState);
        return result;
    }


    public void ApplySerializedFishData(FishDataObjectList masterList, GameState gameState) {
        var fishList = masterList.data;
        string fishName;
        SerializedFishData saveData;
        for (int i = 0; i < fishList.Count; i++) {
            fishName = fishList[i].data.name;
            if (gameState.serializedFishNames.Contains(fishName)) {
                int index = gameState.serializedFishNames.IndexOf(fishName);
                saveData = gameState.serializedFishDataDict[index];
                fishList[i].data.saveData = saveData;
            }
        }
    }

    public void SerializeFishData(FishDataObjectList masterList, GameState gameState) {
        FishData fishData;
        Debug.Log(gameState.serializedFishNames.Count + " " + gameState.serializedFishDataDict.Count);
        for (int i = 0; i < masterList.data.Count; i++) {
            fishData = masterList.data[i].data;
            var serializedData = fishData.saveData;
            if (gameState.serializedFishNames.Contains(fishData.name)) {
                int index = gameState.serializedFishNames.IndexOf(fishData.name);
                gameState.serializedFishDataDict[index] = serializedData;
            } else {
                gameState.serializedFishNames.Add(fishData.name);
                gameState.serializedFishDataDict.Add(serializedData);
            }
        }
    }


    public void SaveGame() {
       SerializeFishData(allFish, this.gameState);
        if (StringSerializationAPI.TrySerialize<GameState>(gameState, out var json)) {
            PlayerPrefs.SetString("GameSave", json);
            PlayerPrefs.Save();
        }

        // call the indexed db file sync to force saving
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            SyncFiles();
        }
#if UNITY_EDITOR
        // Debug.Log("Saved to " + SavePath);
#endif
    }

}

[System.Serializable]
public struct GameState {
    public string lastEnvironmentName;
    public string lastBaitName;

    // have to use two lists because full serializer doesn't support dictionaries
    public List<string> serializedFishNames;
    public List<SerializedFishData> serializedFishDataDict;


    public static GameState Copy(GameState other) {
        GameState result = new GameState();
        result.lastEnvironmentName = string.Copy(other.lastEnvironmentName);
        result.serializedFishDataDict = new List<SerializedFishData>(other.serializedFishDataDict);
        return result;
    }
    public static GameState CreateDefault() {
        return new GameState {
            lastEnvironmentName = "",
            serializedFishDataDict = new List<SerializedFishData>(32),
        };
    }
}
