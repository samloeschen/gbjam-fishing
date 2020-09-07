using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour {
    public Animator animator;
    public event Action<TransitionAnimationEvent> onAnimationEvent;
    public static int _currentScene = -1;
    public static int _nextScene = -1;
    public void TransitionToScene(int sceneIndex) {
        if (_nextScene >= 0) { return; }
        _nextScene = sceneIndex;
        animator.SetTrigger("TransitionStart");
    }
    void Awake() {
        onAnimationEvent += (TransitionAnimationEvent e) => {
            switch (e) {
            case TransitionAnimationEvent.TransitionOutStart:
            break;

            case TransitionAnimationEvent.TransitionOutEnd:
            SceneManager.UnloadSceneAsync(_currentScene);
            SceneManager.LoadSceneAsync(TransitionManager._nextScene, LoadSceneMode.Additive);

            break;

            case TransitionAnimationEvent.TransitionInStart:
            break;

            case TransitionAnimationEvent.TransitionInEnd:
            break;
            }
        };
        _nextScene = -1;
        _currentScene = SceneManager.GetActiveScene().buildIndex;
    }

    public void AnimationEventHook(TransitionAnimationEvent e) {
        if (onAnimationEvent != null) { onAnimationEvent(e); }
    }


}

public static class SceneUnloadedCallbackHandler {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Setup () {
        Debug.Log("Setup");
        SceneManager.sceneUnloaded += SceneUnloadedCallback;
    }
    public static void SceneUnloadedCallback(Scene scene) {
        Debug.Log("callback");
        if (scene.buildIndex == TransitionManager._currentScene && TransitionManager._nextScene >= 0) {
            // SceneManager.LoadScene(TransitionManager._nextScene, LoadSceneMode.Additive);
        }
    }
}

public enum TransitionAnimationEvent {
    TransitionOutStart, TransitionOutEnd, TransitionInStart, TransitionInEnd
}
