using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class TimeManager : MonoBehaviour {

    public bool useDebugT;
    public bool updateDebugT;
    [Range(0f, 1f)]
    public double debugT;
    public float debugTRate = 0.25f;

    [Header("Transforms")]
    public Transform sunTrasform;
    public Transform moonTransform;

    [Header("Animation")]
    public LightAnimator[] lightAnimationData;
    public AnimationCurve backgroundCurve;
    public AnimationCurve hueCurve;
    public AnimationCurve sunCurve;
    public AnimationCurve moonCurve;

    [Header("Components")]
    public Camera waterCamera;
    public int hueSteps = 4;
    public HueShift hueShift;

    const int SECONDS_IN_HOUR = 3600;
    const double MS_IN_DAY = 8.64e+7;
    void Update() {
        var time = System.DateTime.Now;
        int seconds = SECONDS_IN_HOUR * time.Hour + time.Second;
        double t = (seconds * 1000d + (double)time.Millisecond) / 8.64e+7;
        
#if UNITY_EDITOR
        if (useDebugT) {
            if (updateDebugT) {
                debugT += (double)Time.deltaTime * debugTRate;
                if (debugT > 1f) debugT -= 1f;
            }
            t = debugT;
        }
#endif
        LightAnimator lightAnimator;
        float tf = (float)t;
        for (int i = 0; i < lightAnimationData.Length; i++) {
            lightAnimator = lightAnimationData[i];
            UpdateLight(lightAnimator, tf);
        }
        waterCamera.backgroundColor = Color.Lerp(Color.black, Color.white, backgroundCurve.Evaluate(tf));
        
        Vector3 sunPos = sunTrasform.position;
        sunPos.y = sunCurve.Evaluate(tf);
        sunTrasform.position = sunPos;

        Vector3 moonPos = moonTransform.position;
        moonPos.y = moonCurve.Evaluate(tf);
        moonTransform.position = moonPos;

        float hueT = Mathf.Floor((1f - backgroundCurve.Evaluate(tf)) * hueSteps + 0.5f) / (hueSteps + 0.5f);
        hueShift.hueShiftMaterial.SetFloat("_ShiftDistance", -0.06f * hueT);
    }
    public void UpdateLight(LightAnimator animData, float t) {
        Vector3 euler = animData.lightTransform.eulerAngles;
        euler.x = animData.lightRotationCurve.Evaluate(t);
        animData.lightTransform.eulerAngles = Vector3.right * euler.x;
        animData.light.intensity = animData.lightIntensityCurve.Evaluate(t);
    }

    [System.Serializable]
    public struct LightAnimator {
        public Transform lightTransform;
        public Light light;
        public float startAngle;
        public float endAngle;
        public AnimationCurve lightIntensityCurve;
        public AnimationCurve lightRotationCurve;
    }
}
