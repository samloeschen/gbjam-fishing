using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolAfterTime : MonoBehaviour {
    public float time = 0f;
    [HideInInspector] public float timer;
    void OnEnable() {
        timer = time;
    }
    void Update() {
        timer -= Time.deltaTime;
        if (timer <= 0f) {
            PoolManager.PoolDestroy(gameObject);
        }
    }
}
