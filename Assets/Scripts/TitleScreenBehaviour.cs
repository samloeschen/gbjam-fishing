using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenBehaviour : MonoBehaviour {

    public Transform startTransform;
    public Transform howToPlayTransform;
    public Transform creditsTransform;
    public Transform cursorTransform;
    TitleSelection currentSelection => _selectionOptions[_selectionIndex];
    public TitleScreen currentScreen;
    public float cursorDamping;

    public TransitionManager transitionManager;

    [Header("SFX")]
    public AudioClip cursorOneShot;
    public AudioClip selectOneShot;
    public AudioClip backOneShot;


    public SpriteRenderer tutorialSpriteRenderer;
    public Sprite[] tutorialSlides;
    int _tutorialIndex;


    public SpriteRenderer creditsSpriteRenderer;

    TitleSelection[] _selectionOptions = {
        TitleSelection.Start, TitleSelection.HowToPlay, TitleSelection.Credits
    };
    Transform[] _selectionTransforms;
    int _selectionIndex = 0;

    void Start() {
        _selectionTransforms = new Transform[] {
            startTransform, howToPlayTransform, creditsTransform
        };
        currentScreen = TitleScreen.Main;
    }

    void Update() {
        HandleInput();
        Transform targetTransform = _selectionTransforms[_selectionIndex];
        cursorTransform.position = MathUtil.Damp(cursorTransform.position, targetTransform.position.withX(0f), cursorDamping, Time.deltaTime);
        tutorialSpriteRenderer.sprite = tutorialSlides[_tutorialIndex];
    }

    void HandleSelection(TitleSelection selection) {
        switch (selection) {
        case TitleSelection.Start:
            StartGame();
        break;

        case TitleSelection.HowToPlay:
            ShowTutorial();
        break;

        case TitleSelection.Credits:
            ShowCredits();
        break;
        }
    }

    void StartGame() {
        transitionManager.TransitionToScene(2);
        this.enabled = false;
    }

    void ShowTutorial() {
        currentScreen = TitleScreen.Tutorial;
        _tutorialIndex = 0;
        tutorialSpriteRenderer.enabled = true;
    }
    void HideTutorial() {
        currentScreen = TitleScreen.Main;
        tutorialSpriteRenderer.enabled = false;
    }

    void ShowCredits() {
        currentScreen = TitleScreen.Credits;
        creditsSpriteRenderer.enabled = true;
    }

    void HideCredits() {
        currentScreen = TitleScreen.Main;
        creditsSpriteRenderer.enabled = false;
    }

    void MutateSelection(int offset) {
        int index = _selectionIndex;
        index += offset;
        Debug.Log(index);
        if (index < 0) {
            index += _selectionOptions.Length;
        }
        else if (index >= _selectionOptions.Length) {
            index -= _selectionOptions.Length;
        }
        _selectionIndex = index;
        OneShotManager.PlayOneShot(cursorOneShot);
    }

    void HandleInput() {
        switch (currentScreen) {
        case TitleScreen.Main:
            if (Input.GetKeyDown(KeyCode.Z)) {
                HandleSelection(currentSelection);
                OneShotManager.PlayOneShot(selectOneShot);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                MutateSelection(-1);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                MutateSelection(1);
            }
        break;

        case TitleScreen.Tutorial:
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                if (_tutorialIndex > 0) {
                    _tutorialIndex--;
                    OneShotManager.PlayOneShot(cursorOneShot);
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                if (_tutorialIndex < tutorialSlides.Length - 1) {
                    _tutorialIndex++;
                    OneShotManager.PlayOneShot(cursorOneShot);
                }
            }
            if (Input.GetKeyDown(KeyCode.X)) {
                HideTutorial();
                OneShotManager.PlayOneShot(backOneShot);
            }
        break;

        case TitleScreen.Credits:
            if (Input.GetKeyDown(KeyCode.X)) {
                HideCredits();
                OneShotManager.PlayOneShot(backOneShot);
            }
        break;
        }
    }
}

public enum TitleSelection {
    Start, HowToPlay, Credits
}

public enum TitleScreen {
    Main, Tutorial, Credits
}
