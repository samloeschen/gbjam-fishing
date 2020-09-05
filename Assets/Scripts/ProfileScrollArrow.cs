using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileScrollArrow : MonoBehaviour {
    public Image image;

    public Sprite pressedSprite;
    public Sprite defaultSprite;

    public float pressAnimationTime = 0.2f;
    float _timer = 0f;

    void OnEnable() {
        _timer = 0f;
        image.enabled = true;
    }

    void OnDisable() {
        image.enabled = false;
    }
    void Update() {
        _timer -= Time.deltaTime;
        _timer = Mathf.Max(0f, _timer);
        image.sprite = _timer > 0f ? pressedSprite : defaultSprite;
    }

    public void Press() {
        _timer = pressAnimationTime;
    }
}
