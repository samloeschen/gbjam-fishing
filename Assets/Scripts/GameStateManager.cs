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
            return true;
        }
        return false;
    }

    public bool LoadGame(out GameState gameState, GameState fallback) {
        gameState = default(GameState);
        if (LoadGame(out gameState)) {
            string json = File.ReadAllText(SavePath);
            gameState = (GameState)StringSerializationAPI.Deserialize(typeof(GameState), json);
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

    public static void CreateDefaultGameState() {

    }
}

[System.Serializable]
public struct GameState {
    public string currentEnvironmenName;
    public void Copy(GameState other) {

    }

    public static GameState CreateDefault() {
        return new GameState {

        };
    }
}
