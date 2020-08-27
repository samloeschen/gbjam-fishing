using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour {

    public int maxFishCount;
    public GameObject fishPrefab;

    [Header("Fish Behaviour")]
    public float newTargetPositionRadiusMin;
    public float newTargePositionRadiusMax;
    public float newDistanceThreshold = 0.01f;
    public float radiusHeightFactor = 0.5f;

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
        StartCoroutine(InitialSpawnRoutine());
    }

    void Update() {
        ActiveFishData fishData;
        for (int i = 0; i < activeFish.Count; i++) {
            fishData = activeFish[i];
            UpdateFish(ref fishData);
            activeFish[i] = fishData;
        }
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
        float currentDistance = (fish.transform.position - fish.targetPosition).magnitude / totalDistance; 
        float moveSpeed = fish.behaviour.moveSpeedCurve.Evaluate(currentDistance / totalDistance);
        fish.transform.position = Vector3.MoveTowards(fish.transform.position, fish.targetPosition, moveSpeed * Time.deltaTime);
        if (currentDistance < newDistanceThreshold) {
            SetNewTargetPosition(ref fish);
        }
    }

    void DespawnFish(ref ActiveFishData fishData) {
        oldFish.Add(fishData);
    }

    void SetNewTargetPosition(ref ActiveFishData fish) {
        const int newPositionCandidateCount = 10;
        float radius = Random.Range(newTargetPositionRadiusMin, newTargePositionRadiusMax);
        float maxDist = 0f;
        for (int i = 0; i < newPositionCandidateCount; i++) {
            float angle = Random.value * Mathf.PI * 2f;
            Vector3 candidatePos = fish.targetPosition + new Vector3 {
                x = Mathf.Cos(angle) * radius,
                y = Mathf.Sin(angle) * radiusHeightFactor
            };
            float totalDist = 0f;
            for (int j = 0; j < activeFish.Count; j++) {
                totalDist += (candidatePos - activeFish[j].transform.position).magnitude;
                totalDist += (candidatePos - activeFish[j].targetPosition).magnitude;
            }
            if (totalDist > maxDist) {
                maxDist = totalDist;
                fish.targetPosition = candidatePos;
            }
        }
    }

    void SetInitialPosition(ref ActiveFishData fish) {
        const int initialPositionCandidateCount = 10;
        float maxDist = 0f;
        for (int i = 0; i < initialPositionCandidateCount; i++) {
            Vector3 candidatePos = spawnRegion.RandomInRect();

            // check overlaps
            if (Physics2D.OverlapCircle(candidatePos, spawnSafeRadius, spawnAvoidLayers)) {
                continue;
            }

            float totalDist = 0f;
            for (int j = 0; j < activeFish.Count; j++) {
                totalDist += (candidatePos - activeFish[j].transform.position).magnitude;
                totalDist += (candidatePos - activeFish[j].targetPosition).magnitude;
            }
            if (totalDist > maxDist) {
                maxDist = totalDist;
                fish.startPosition = candidatePos;
            }
        }
    }

    public ActiveFishData SpawnFish() {
        var clone = PoolManager.PoolInstantiate(fishPrefab);
        ActiveFishData fishData = new ActiveFishData();

        clone.TryGetComponent<Transform>(out fishData.transform);
        clone.TryGetComponent<Animator>(out fishData.animator);

        SetInitialPosition(ref fishData);
        SetNewTargetPosition(ref fishData);
        fishData.behaviour = fishBehaviours.GetRandomWithSwapback();
        return fishData;
    }
}

[System.Serializable]
public struct ActiveFishData {
    public Transform transform;
    public Animator animator;
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public FishBehaviour behaviour;

}

[System.Serializable]
public struct FishBehaviour {
    public float lifeTime;
    public AnimationCurve moveSpeedCurve;
}
