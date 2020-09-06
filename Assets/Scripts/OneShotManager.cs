using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotManager : MonoBehaviour {
    public GameObject oneShotPrefab;
    static OneShotManager _singleton;

    public void Awake() {
        _singleton = this;
    }
    public static void PlayOneShot(AudioClip audioClip, float volume = 1f) {
        if (!audioClip) { return; }
        var instance = PoolManager.PoolInstantiate(_singleton.oneShotPrefab, Vector3.zero, Quaternion.identity);
        if (instance.TryGetComponent<AudioOneShotBehaviour>(out var audioOneShotBehaviour)) {
            audioOneShotBehaviour.Play(audioClip, volume);
        }
    }
}
