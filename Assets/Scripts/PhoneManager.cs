using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager : MonoBehaviour {
    public Animator phoneAnimator;
    public GameObject scrollViewItemPrefab;
    public Transform scrollViewContentTransform;
    public BobberAimBehaviour aimBehaviour;
    public Dictionary<FishDataObject, FishProfileCell> cellDict;


    [System.NonSerialized]
    public bool phoneEnabled;
    public void Initialize(List<FishDataObject> fishList) {

        // set up scroll view items
        cellDict = new Dictionary<FishDataObject, FishProfileCell>(fishList.Count);
        for (int i = 0; i < fishList.Count; i++) {
            GameObject clone = GameObject.Instantiate(
                scrollViewItemPrefab,
                Vector3.zero,
                Quaternion.identity,
                scrollViewContentTransform
            );
            if (clone.TryGetComponent<FishProfileCell>(out var cell)) {
                cellDict.Add(fishList[i], cell);
                cell.Initialize(fishList[i]);
            }
        }
    }

    void OnEnable() {
        phoneEnabled = false;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!phoneEnabled) {
                phoneEnabled = true;
                phoneAnimator.SetTrigger("PhoneTransition");
                phoneAnimator.SetBool("PhoneEnabled", true);
                aimBehaviour.enabled = false;
            } else {
                phoneEnabled = false;
                phoneAnimator.SetTrigger("PhoneTransition");
                phoneAnimator.SetBool("PhoneEnabled", false);
                aimBehaviour.enabled = true;
            }
        }
    }
}
