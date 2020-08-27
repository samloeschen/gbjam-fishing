using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour {

    public int maxFishCount;
    public GameObject fishPrefab;
    public Transform fishParentTransform;

    [Header("Fish Behaviour")]
    public float newTargetPositionRadiusMin;
    public float newTargePositionRadiusMax;
    public float newDistanceThreshold = 0.01f;
    public float radiusHeightFactor = 0.5f;
    public float randomMoveChance = 0.05f;
    public Rect moveRegion;

    [Header("Fish Spawning")]
    public Rect spawnRegion;
    public LayerMask spawnAvoidLayers;
    public float spawnSafeRadius = 0.1f;


    [Header("Initial Spawn")]
    public int initialFishCount;
    public float initialSpawnDelay;
    public float perFishDelay;
    public float spawnDelayJitter;

    public List<FishBehaviour> fishBehaviours;
    public List<ActiveFishData> activeFish;
    public List<ActiveFishData> oldFish;

    void Awake() {
        activeFish = new List<ActiveFishData>(16);
    }

    void OnEnable() {
        activeFish.Clear();
        oldFish.Clear();
        StartCoroutine(InitialSpawnRoutine());
    }

    void OnDisable() {
        
    }

    void Update() {
        ActiveFishData fishData;
        for (int i = 0; i < activeFish.Count; i++) {
            fishData = activeFish[i];
            UpdateFish(ref fishData);
            activeFish[i] = fishData;
        }
    }


    void OnDrawGizmos() {
            Gizmos.color = Color.red;
            moveRegion.DrawGizmos();

            Gizmos.color = Color.white;
            spawnRegion.DrawGizmos();
    }

    private IEnumerator InitialSpawnRoutine() {
        yield return new WaitForSeconds(initialSpawnDelay);
        for (int i = 0; i < initialFishCount; i++) {
            activeFish.Add(SpawnFish());
            yield return new WaitForSeconds(perFishDelay + Random.value * spawnDelayJitter);
        }
        yield return null;
    }

    void UpdateFish(ref ActiveFishData fish) {
        float totalDistance = (fish.startPosition - fish.targetPosition).magnitude;
        float currentDistance = ((Vector2)fish.transform.localPosition - fish.targetPosition).magnitude; 
        float moveSpeed = fish.behaviour.moveSpeedCurve.Evaluate(1f - (currentDistance / totalDistance));
        fish.transform.localPosition = Vector2.MoveTowards(fish.transform.localPosition, fish.targetPosition, Time.deltaTime * moveSpeed);
        if (currentDistance < newDistanceThreshold) {
            SetNewTargetPosition(ref fish);
        }

        Debug.DrawLine(fish.startPosition, fish.targetPosition, Color.green);
    }

    void DespawnFish(ref ActiveFishData fishData) {
        oldFish.Add(fishData);
    }

    void SetNewTargetPosition(ref ActiveFishData fish) {
        const int newPositionCandidateCount = 20;
        float radius = Random.Range(newTargetPositionRadiusMin, newTargePositionRadiusMax);
        float maxDist = 0f;
        Vector2 center = spawnRegion.center;
        fish.startPosition = fish.transform.localPosition;

        if (Random.value < randomMoveChance) {
            fish.targetPosition = moveRegion.RandomInRect();
        } else {
            float angleOffset = Random.value * Mathf.PI * 2f;
            for (int i = 0; i < newPositionCandidateCount; i++) {
                float angle = (i / (float)newPositionCandidateCount) * Mathf.PI * 2f + angleOffset;
                Vector2 candidatePos = (Vector2)fish.transform.localPosition + new Vector2 {
                    x = Mathf.Cos(angle) * radius,
                    y = Mathf.Sin(angle) * radiusHeightFactor
                };

                if (!moveRegion.Contains(candidatePos)) { continue; }
                if (Physics2D.OverlapCircle(candidatePos, spawnSafeRadius, spawnAvoidLayers)) { continue; }

                float totalDist = 0f;
                for (int j = 0; j < activeFish.Count; j++) {
                    if (activeFish[j].id == fish.id) { continue; }
                    totalDist += (candidatePos - (Vector2)activeFish[j].transform.localPosition).magnitude;
                    totalDist += (candidatePos - activeFish[j].targetPosition).magnitude;
                }
                totalDist += (candidatePos - fish.lastTargetPosition).magnitude * 1.5f;
                if (totalDist >= maxDist) {
                    maxDist = totalDist;
                    fish.targetPosition = candidatePos;
                }
            }
        }
        fish.lastTargetPosition = fish.transform.localPosition;
    }

    void SetInitialPosition(ref ActiveFishData fish) {
        const int initialPositionCandidateCount = 3;
        float maxDist = 0f;
        for (int i = 0; i < initialPositionCandidateCount; i++) {
            Vector2 candidatePos = spawnRegion.RandomInRect();

            // check overlaps
            if (Physics2D.OverlapCircle(candidatePos, spawnSafeRadius, spawnAvoidLayers)) { continue; }

            float totalDist = 0f;
            for (int j = 0; j < activeFish.Count; j++) {
                if (activeFish[j].id == fish.id) { continue; }
                totalDist += (candidatePos - (Vector2)activeFish[j].transform.localPosition).magnitude;
                totalDist += (candidatePos - activeFish[j].targetPosition).magnitude;
            }
            if (totalDist >= maxDist) {
                maxDist = totalDist;
                fish.startPosition = candidatePos;
                fish.transform.localPosition = candidatePos;
                fish.targetPosition = candidatePos;
            }
        }
    }

    int _fishID;
    public ActiveFishData SpawnFish() {
        var clone = PoolManager.PoolInstantiate(fishPrefab, Vector2.zero, Quaternion.identity);
        ActiveFishData fishData = new ActiveFishData();

        clone.TryGetComponent<Transform>(out fishData.transform);
        clone.TryGetComponent<Animator>(out fishData.animator);

        fishData.transform.SetParent(fishParentTransform, worldPositionStays: true);

        SetInitialPosition(ref fishData);
        SetNewTargetPosition(ref fishData);
        fishData.behaviour = fishBehaviours.GetRandomWithSwapback();

        fishData.id = _fishID++;
        return fishData;
    }
}

[System.Serializable]
public struct ActiveFishData {
    public Transform transform;
    public Animator animator;
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public Vector2 lastTargetPosition;
    public FishBehaviour behaviour;
    public int id;

}

[System.Serializable]
public struct FishBehaviour {
    public float lifeTime;
    public AnimationCurve moveSpeedCurve;
}
