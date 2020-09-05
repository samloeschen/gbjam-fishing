using UnityEngine;
public static class MathUtil {
    public static float Damp(float a, float b, float k, float dt) {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-k * dt));
    }

    public static Vector3 Damp(Vector3 a, Vector3 b, float k, float dt) {
        return new Vector3 {
            x = Damp(a.x, b.x, k, dt),
            y = Damp(a.y, b.y, k, dt),
            z = Damp(a.z, b.z, k, dt),
        };
    }
}