using System;
using UnityEngine;
public class InputManager {

    public static ButtonInput buttonState;
    public static ButtonInput buttonDownState;
    public static Vector2 dpad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() {
        var gameObject = new GameObject("InputManager");
        GameObject.DontDestroyOnLoad(gameObject);
        gameObject.AddComponent(typeof(InputManager));
    }

    // no idea why i decided to do this
    void Update() { 
        
        buttonDownState = ButtonInput.None;
        buttonState = ButtonInput.None;

        if (Input.GetKey(KeyCode.A)) {
            buttonDownState |= ButtonInput.A;
        }
    }
}

[Flags]
public enum ButtonInput {
    None   = 0,
    A      = 1,
    B      = 2,
    L      = 4,
    R      = 8,
    Start  = 16,
    Select = 32,
}