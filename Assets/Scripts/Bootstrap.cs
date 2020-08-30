using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    public GameStateManager gameStateManager;
    public List<EnvironmentDataObject> environmenList;
    public EnvironmentDataObject defaultEnvironment;

    public bool useCustomEnvironment;
    public EnvironmentDataObject customEnvironment;


    [HideInInspector]
    public EnvironmentDataObject currentEnvironment;

    void OnEnable() {
        gameStateManager.LoadGame();
        currentEnvironment = defaultEnvironment;
        for (int i = 0; i < environmenList.Count; i++) {
            if (gameStateManager.gameState.currentEnvironmenName == environmenList[i].data.name) {
                currentEnvironment = environmenList[i];
                break;
            }
        }
#if UNITY_EDITOR
        if (useCustomEnvironment) {
            currentEnvironment = customEnvironment ?? currentEnvironment;
        }
#endif
        LoadEnvironment(currentEnvironment.data);
    }

    void LoadEnvironment(EnvironmentData environmentData) {
        Vector3 position = environmentData.prefab.GetComponent<Transform>().position;
        GameObject.Instantiate(environmentData.prefab, position, Quaternion.identity);
    }

    void OnDisable() {
        gameStateManager.gameState.currentEnvironmenName = currentEnvironment.data.name;
        gameStateManager.SaveGame();
    }
}
