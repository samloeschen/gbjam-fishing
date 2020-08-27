using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HueShift : MonoBehaviour {
    public Material hueShiftMaterial;
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        Graphics.Blit(src, dst, hueShiftMaterial, 0);
    }
}
