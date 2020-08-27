using UnityEngine;
using System.Collections.Generic;
public static class RectExtensions {
	public static Vector2 RandomInRect(this Rect r) {
		float x = Random.Range(r.xMin, r.xMax);
		float y = Random.Range(r.yMin, r.yMax);
		return new Vector2(x,y);
	}

	public static Rect withCenter(this Rect r, Vector2 center) {
		Rect rect = new Rect(r);
		rect.center = center;
		return rect;
	}

	public static Rect Union(this Rect r, Rect other) {
		var xMin = Mathf.Min(r.xMin, other.xMin);
		var yMin = Mathf.Min(r.yMin, other.yMin);
		var xMax = Mathf.Max(r.xMax, other.xMax);
		var yMax = Mathf.Max(r.yMax, other.yMax);
		return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
	}

	public static Rect FromPoints (List<Vector2> points) {
		if (points.Count == 0) return new Rect();

		Vector2 p = points[0];
		Rect r = new Rect(p.x, p.y, 0, 0);
		for (int i = 0; i < points.Count; i++) {
			p = points[i];

			r.xMin = Mathf.Min(r.xMin, p.x);
            r.yMin = Mathf.Min(r.yMin, p.y);

            r.xMax = Mathf.Max(r.xMax, p.x);
            r.yMax = Mathf.Max(r.yMax, p.y);
		}
		return r;
	}

	public static Rect FromPoints (Vector2[] points, int pointCount) {
		pointCount = Mathf.Min(pointCount, points.Length);
		if (points.Length == 0) return new Rect();

		Vector2 p = points[0];
		Rect r = new Rect(p.x, p.y, 0, 0);
		for (int i = 0; i < pointCount; i++) {
			p = points[i];

			r.xMin = Mathf.Min(r.xMin, p.x);
            r.yMin = Mathf.Min(r.yMin, p.y);

            r.xMax = Mathf.Max(r.xMax, p.x);
            r.yMax = Mathf.Max(r.yMax, p.y);
		}
		return r;
	}


	public static void DrawGizmos(this Rect r) {
		Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0.01f), new Vector3(r.size.x, r.size.y, 0.01f));
	}
}
