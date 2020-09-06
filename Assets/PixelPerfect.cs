using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelPerfect : MonoBehaviour {
    public Material blitMaterial;
    public RenderTexture frame;
    public new Camera camera;
    void OnRenderImage(RenderTexture src, RenderTexture dst) {


        float pixelRatio = 1f;
        if (camera.pixelWidth < camera.pixelHeight) {
            pixelRatio = ((float)camera.pixelWidth / (float)frame.width);
        } else {
            pixelRatio = ((float)camera.pixelHeight / (float)frame.height);
        }
        float frameAspect = (float)frame.width / (float)frame.height;
        blitMaterial.SetFloat("_ViewportAspect", camera.aspect);
        blitMaterial.SetFloat("_FrameAspect", frameAspect);
        blitMaterial.SetTexture("_Frame", frame);
        blitMaterial.SetFloat("_PixelRatio", pixelRatio);
        blitMaterial.SetVector("_ViewportSize", new Vector2(camera.pixelWidth, camera.pixelHeight));
        blitMaterial.SetVector("_FrameSize", new Vector2(frame.width, frame.height));
        Graphics.Blit(src, dst, blitMaterial , 0);
    }
}
