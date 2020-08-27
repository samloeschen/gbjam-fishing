using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteScaler : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    [Range (0f, 1f)]
    public float scale;

    public Sprite[] sprites;

    void OnValidate() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable() {
        if (!spriteRenderer) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (!spriteRenderer) {
                this.enabled = false;
            }
        }
    }

    void Update() {
        int idx = (int)(Mathf.Clamp01(scale) * (sprites.Length - 1));
        spriteRenderer.sprite = sprites[idx];
    }
}
