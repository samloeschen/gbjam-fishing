using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOneShotBehaviour : MonoBehaviour {
    public AudioSource audioSource;
    bool _playing;

    void OnEnable() {
        _playing = false;
    }

    public void Play(AudioClip audioClip, float volume = 1f) {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        _playing = true;
    }
    void Update() {
        if (_playing && !audioSource.isPlaying) {
            PoolManager.PoolDestroy(this.gameObject);
        }
    }
}
