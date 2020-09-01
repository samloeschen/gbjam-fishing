using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    public GameStateManager gameStateManager;
    public List<EnvironmentDataObject> environmenList;
    public PhoneManager phoneManager;
    public bool useCustomEnvironment;
    public BaitManager baitManager;
    public FishManager fishManager;

    [Header("Environment")]
    public EnvironmentDataObject customEnvironment;

    [HideInInspector]
    public EnvironmentDataObject currentEnvironment;
    public FishDataObjectList allFish;

    void Awake() {
        gameStateManager.LoadGame();
        for (int i = 0; i < environmenList.Count; i++) {
            if (environmenList[i].data.name == gameStateManager.gameState.lastEnvironmentName) {
                environmenList.RemoveAt(i);
            }
        }

        // set up environment
        currentEnvironment = environmenList.GetRandomElement();
        for (int i = 0; i < environmenList.Count; i++) {
            if (gameStateManager.gameState.lastEnvironmentName == environmenList[i].data.name) {
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

        // initialize phone
        phoneManager.Initialize(allFish.data);

        // initialize game UI
        baitManager.Initialize(gameStateManager.gameState);

        fishManager.Initialize(currentEnvironment.data, gameStateManager.gameState);
    }

    void LoadEnvironment(EnvironmentData environmentData) {
        Vector3 position = environmentData.prefab.GetComponent<Transform>().position;
        GameObject.Instantiate(environmentData.prefab, position, Quaternion.identity);
    }

    void OnDisable() {
        gameStateManager.gameState.lastEnvironmentName = currentEnvironment.data.name;
        gameStateManager.SaveGame();
    }
}
