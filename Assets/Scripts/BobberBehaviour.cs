using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberBehaviour : MonoBehaviour {
    public Transform physicsTransform;
    public Transform anchorTransform;
    public Vector3 targetPosition;
    public float physicsT;
    public Vector2 castT;
    public new Rigidbody2D rigidbody2D;
    void Update() {
        Vector3 rootPos = Vector3.Lerp(physicsTransform.position, anchorTransform.position, physicsT);
        Vector3 currentCastPos = new Vector3 {
            x = Mathf.LerpUnclamped(rootPos.x, targetPosition.x, castT.x),
            y = Mathf.LerpUnclamped(rootPos.y, targetPosition.y, castT.y),
            z = transform.position.z
        };
        rigidbody2D.MovePosition(currentCastPos);
    }
}
