using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverTest : MonoBehaviour {

    public float speed;
    public CircleMover mover;

    void FixedUpdate() {
        Vector2 delta = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) {
            delta += Vector2.up;
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            delta += Vector2.down;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            delta += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            delta += Vector2.right;
        }
        delta = delta.normalized * Time.deltaTime;
        mover.Move(delta);
    }

    void OnCollisionEnter2D(Collision collision) {
        Debug.Log(collision.collider.gameObject.name);
    }
} 



public interface IMover {
    Vector2 position { get; }
    void ResolveMovement(ref Vector2 delta, ref Vector2 position);
    void Move(Vector2 delta);
}


public interface IHitTest {
    RaycastHit2D GetHit(Vector2 position, Vector2 delta, LayerMask hitLayers);
}

[System.Serializable]
public struct CircleColliderHitTest: IHitTest {
    public CircleCollider2D circleCollider;
    public RaycastHit2D GetHit(Vector2 position, Vector2 delta, LayerMask hitLayers) {
        return Physics2D.CircleCast(
            position,
            circleCollider.radius,
            delta.normalized,
            delta.magnitude,
            hitLayers
        );
    }
}

public class GenericMover<THitTest> : MonoBehaviour, IMover where THitTest: IHitTest {
    public Vector2 position => rigidbody2D.position;
    public int resolveIterations = 10;
    public new Rigidbody2D rigidbody2D;
    public LayerMask hitLayers;
    public THitTest hitTest;
    public float contactEpsilon;
    public float resolveBounciness;

    public void Move(Vector2 delta) {
        Vector2 position = this.position;
        ResolveMovement(ref delta, ref position);
        rigidbody2D.MovePosition(position);
    }

    public void ResolveMovement(ref Vector2 delta, ref Vector2 position) {
        Vector3 startPosition = position;
        int resolveIterations = this.resolveIterations;
        RaycastHit2D hitInfo = default(RaycastHit2D);

        do {
            resolveIterations--;
            hitInfo = hitTest.GetHit(position, delta, hitLayers);
            if (hitInfo.collider != null) {
                float safeDistance = Mathf.Max(hitInfo.distance - contactEpsilon, 0f);
                position += delta.normalized * safeDistance;

                float d = Vector3.Dot(delta.normalized, hitInfo.normal);
                if (d > -0.9f) {
                    delta = Vector3.ProjectOnPlane(delta, hitInfo.normal).normalized * delta.magnitude;
                } else {
                    delta = Vector3.zero;
                }
                delta -= (Vector2)Vector3.Project(delta, hitInfo.normal) * (1f + resolveBounciness);
            } else if (resolveIterations > 0) {
                position += delta; // all done
            }
        } while(hitInfo.collider != null && resolveIterations > 0); 
        if (resolveIterations == 0) {
            position = startPosition;
            delta = Vector2.zero;
        }
    }
}
