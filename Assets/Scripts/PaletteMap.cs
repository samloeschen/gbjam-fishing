using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PaletteMap : MonoBehaviour {
    public Material paletteMapMaterial;
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        Graphics.Blit(src, dst, paletteMapMaterial, 0);
    }
}
