using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenSpaceTransition : MonoBehaviour {
    public Material material;
    public new Camera camera;
    public Vector2 scrollDir;
    public float scrollSpeed;
    [Range(0f, 1f)]
    public float radiusAnimation;
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        material.SetVector("_ScrollSpeed", scrollDir.normalized * scrollSpeed);
        material.SetFloat("_RadiusAnimation", 1f - radiusAnimation);
        material.SetFloat("_Aspect", camera.aspect);
        Graphics.Blit(src, dst, material);
    }
}
