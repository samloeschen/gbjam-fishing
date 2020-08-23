using UnityEngine;
[System.Serializable]
public struct EnvironmentData {
    public string name;
    public GameObject prefab;
    public Vector3 sunEulers;

    [System.NonSerialized]
    public int index;
}