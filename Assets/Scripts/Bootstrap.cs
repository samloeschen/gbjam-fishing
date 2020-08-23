using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    public EnvironmentDataListObject environmentsListObject;

    bool overrideEnvironmentObject;
    public int customEnvironmentIndex;
    public int defaultEnvironmentIndex;
    public int currentEnvironmentIndex;
    const string ENVIRONMENT_INDEX_KEY = "ENVIRONMENT_INDEX";
    public void Awake() {
        for (int i = 0; i < environmentsListObject.reference.Count; i++) {
            environmentsListObject.reference[i].reference.index = i;
        }

        EnvironmentDataObject environmentDataObject = null;
        int environmentIndex;
        if (PlayerPrefs.HasKey(ENVIRONMENT_INDEX_KEY)) {
            environmentIndex = PlayerPrefs.GetInt(ENVIRONMENT_INDEX_KEY);
        } else {
            environmentIndex = defaultEnvironmentIndex;
        }
        environmentDataObject = environmentsListObject.reference[environmentIndex];

#if UNTIY_ENGINE
        if (overrideEnvironmentObject) {
            environmentDataObject = environmentsListObject.reference[customEnvironmentIndex];
        }
#endif
    }

    void OnEnable() {
        
    }

    void OnDisable() {
        SaveEnvironment();
    }
    void OnDestroy() {
        SaveEnvironment();
    }

    public void SaveEnvironment() {
        PlayerPrefs.SetInt("ENVIRONMENT_ID", currentEnvironmentIndex);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() {
        var bootstrapObject = new GameObject("Bootstrap");
        bootstrapObject.AddComponent<Bootstrap>();
    }
}
