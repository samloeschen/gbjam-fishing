using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteScoller : MonoBehaviour {
    public Vector2 scrollSpeed;
    public SpriteRenderer spriteRenderer;
    MaterialPropertyBlock _mpb;

    void OnEnable() {
        _mpb = new MaterialPropertyBlock();
    }
    void OnValidate() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update() {
        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetVector("_TextureSize", new Vector2(spriteRenderer.sprite.rect.width, spriteRenderer.sprite.rect.height));
        _mpb.SetVector("_ScrollSpeed", scrollSpeed);
        spriteRenderer.SetPropertyBlock(_mpb);
    }
}
