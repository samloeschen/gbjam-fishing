using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class PoolManager {

    public static Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();
    /*
	* Use this function for general purpose GameObject instantiation. It will instantiate the
	* a pooled instance immediately. If it doesn't find a pooled instance, it uses GetInstanceInactive()
	* to make a new one, and immediately instantiates and activates that. If the instance matches one already
	* in the pool (for example, one obtained from GetInstanceInactive), it just instantiates it.
	*/
    public static GameObject PoolInstantiate(GameObject prefab) {
        return PoolInstantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    public static GameObject PoolInstantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
        if(prefab == null) return null;
        GameObject tempObject = null;
        PoolPrefabTracker tracker = null;
        bool makeNew = false;
        if (pools.ContainsKey(prefab)) {
            if (pools[prefab].Count > 0) {
                //pool exists and has unused instances
                tempObject = pools[prefab][0];
                pools[prefab].RemoveAt(0);
                tempObject.transform.position = position;
                tempObject.transform.rotation = rotation;
                tempObject.transform.localScale = prefab.transform.localScale;
                tracker = tempObject.GetComponent<PoolPrefabTracker>();
                tracker.SetReleased();
                tempObject.SetActive(true);
                return tempObject;
            } else {
                //pool exists but is empty
                makeNew = true;
            }
        } else {
            //pool for this prefab does not yet exist
            pools.Add(prefab, new List<GameObject>());
            makeNew = true;
        }
        if (makeNew) {
            tempObject = GameObject.Instantiate(prefab, position, rotation);
            tracker = tempObject.AddComponent<PoolPrefabTracker>();
            tracker.myPrefab = prefab;
            tracker.SetReleased();
            return tempObject;
        }
        return tempObject;
    }
    static public GameObject PoolInstantiate (MonoBehaviour mb, Vector3 position, Quaternion rotation){
        return PoolInstantiate(mb.gameObject, position, rotation);
    }
    static public void PoolDestroy (GameObject target, bool removeParent = false) {
        if (!target) return;

        if (removeParent) {
            target.transform.parent = null;
        }

        PoolPrefabTracker tracker = target.GetComponent<PoolPrefabTracker>();
        if (tracker) {
            if (tracker.pooled) { return; }
            GameObject prefab = tracker.myPrefab;
            target.SetActive(false);
            if (!pools.ContainsKey(prefab)) return;
            pools[prefab].Add(target);
        } else {
            tracker = target.AddComponent<PoolPrefabTracker>();
            tracker.myPrefab = target;
            if (!pools.ContainsKey(target)) {
                pools.Add(target, new List<GameObject>());
            }
            PoolDestroy(target);
        }
        tracker.SetPooled();
    }
}
public class PoolPrefabTracker : MonoBehaviour {
    public delegate void PoolEventHandler();
    public event Action OnPooled; //removed from pool
    public event Action OnReleased; //returned to pool
    public GameObject myPrefab;
    public bool pooled;
    public int instanceID; 

    static int IDCounter;
    static int GetNext() {
        return IDCounter++;
    }

    public void SetPooled(){
        pooled = true;
        if(OnPooled != null) OnPooled();
    }
    public void SetReleased(){
        instanceID = GetNext();
        pooled = false;
        if(OnReleased != null) OnReleased();
    }
}



