using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingLine : MonoBehaviour {

    public LineRenderer lineRenderer;
    public int pointCount;
    public Vector3[] points;
    public Vector3[] velocities;

    public float lerpSpeed;

    public Transform a;
    public Transform b;
    public AnimationCurve slackCurve;

    void Start() {
        points = new Vector3[pointCount];
        velocities = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++) {
            points[i] = Vector3.Lerp(a.position, b.position, i / (pointCount - 1f));
        }
    }

    void Update() {
        points[0] = a.position;
        points[pointCount - 1] = b.position;

        // you thought this was going to be a verlet integration
        for (int i = 1; i < pointCount - 1; i++) {
            Vector3 a = this.a.position;
            Vector3 b = this.b.position;
            Vector3 velocity = velocities[i];
            float t = i / (pointCount - 1f);
            Vector3 p = new Vector3 {
                x = Mathf.LerpUnclamped(a.x, b.x, t),
                y = Mathf.LerpUnclamped(a.y, b.y, slackCurve.Evaluate(t)),
                z = a.z
            };
            points[i] = Vector3.SmoothDamp(points[i], p, ref velocity, Time.deltaTime * lerpSpeed);
            velocities[i] = velocity;
        }
        lineRenderer.SetPositions(points);
    }
}
