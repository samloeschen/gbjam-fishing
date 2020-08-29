using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchSystem : MonoBehaviour {

    bool mashMode = false;

    public float mashDecayRate;
    public float mashDeltaPerPress;
    public float mashMaxDuration;
    public FishManager fishManager;
    float _currentMashValue;
    float _mashTimer;

    void OnEnable() {
        _currentMashValue = 0f;
    }

    void Update() {
        if (mashMode) {
            if (Input.GetKeyDown("KeyCode.X")) {
                _currentMashValue += mashDeltaPerPress;
            }
        }
    }

    public void StartMash() {
        _mashTimer = mashMaxDuration;
    }
}
