using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager : MonoBehaviour {
    public Animator phoneAnimator;
    public GameObject scrollViewItemPrefab;
    public Transform scrollViewContentTransform;

    public Dictionary<FishDataObject, FishProfileCell> cellDict;
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
}
