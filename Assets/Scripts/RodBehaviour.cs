using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodBehaviour : MonoBehaviour {
    public Animator animator;

    public void HandleInput(RodAction action) {
        switch (action) {
            case RodAction.Cast:
            animator.SetTrigger("CastStart");
            break;

            case RodAction.Bite:
            animator.SetTrigger("BiteStart");
            break;

            case RodAction.Reel:
            animator.SetTrigger("ReelStart");
            break;
        }
    }
}

public enum RodAction {
    Cast, Bite, Reel,
}
