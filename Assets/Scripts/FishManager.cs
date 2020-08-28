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

    public float avoidOtherFishWeight = 1f;
    public float avoidPreviousPositionWeight = 1f;
    public float avoidBitingFishWeight = 2f;
    public float avoidPierWeight = 2f;


    public Rect moveRegion;

    [Header("Fish Spawning")]
    public Rect spawnRegion;
    public LayerMask spawnAvoidLayers;
    public float spawnSafeRadius = 0.1f;

    [Header("Bite Behaviour")]
    public float biteIntervalMin;
    public float biteIntervalMax;
    public float bobberLookRadius;
    public Vector2 bobberOffset;
    public Vector2 biteNotificationOffset;
    
    public GameObject smallBiteAnimationPrefab;
    public GameObject bigBiteAnimationPrefab;
    

    [Header("Initial Spawn")]
    public int initialFishCount;
    public float initialSpawnDelay;
    public float perFishDelay;
    public float spawnDelayJitter;

    public List<FishBehaviour> fishBehaviours;
    public List<ActiveFishData> activeFish;
    public List<ActiveFishData> oldFish;

    public int bitingFishID = -1;

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

    Collider2D[] _circleCastResults = new Collider2D[8];
    void UpdateFish(ref ActiveFishData fish) {
        switch (fish.state) {
            case FishState.Moving:
            float totalDistance = (fish.startPosition - fish.targetPosition).magnitude;
            float currentDistance = ((Vector2)fish.position - fish.targetPosition).magnitude; 
            float moveSpeed = 0f;
            if (totalDistance > Mathf.Epsilon) {
                moveSpeed = fish.behaviour.moveSpeedCurve.Evaluate(1f - (currentDistance / totalDistance));
            }
            fish.position = Vector2.MoveTowards(fish.position, fish.targetPosition, Time.deltaTime * moveSpeed);
            if (currentDistance < newDistanceThreshold) {
                if (fish.targetBobber) {
                    fish.state = FishState.Biting;
                    SetNextBiteInterval(ref fish);
                } else {
                    SetNewTargetPosition(ref fish);
                    if (Random.value < 0.5f) {
                        fish.state = FishState.Idle;
                        fish.timer = Random.Range(fish.behaviour.idleTimerMin, fish.behaviour.idleTimerMax);
                    }
                }
            }
            break;
            case FishState.Idle:
                fish.timer -= Time.deltaTime;
                if (fish.timer <= 0f) {
                    fish.state = FishState.Moving;
                }
            break;

            case FishState.Biting:
            fish.timer -= Time.deltaTime;
            if (fish.timer <= 0f) {
                DoBite(ref fish);
            }
            break;
        }

        // check and see if fish is near a bobber
        if (bitingFishID < 0) {
            int overlapCount = Physics2D.OverlapCircleNonAlloc(fish.position, bobberLookRadius, _circleCastResults);
            for (int i = 0; i < overlapCount; i++) {
                if (_circleCastResults[i].TryGetComponent<BobberBehaviour>(out var bobberBehaviour)) {
                    if (!bobberBehaviour.isInWater) { continue; }
                    fish.state = FishState.Moving;
                    fish.targetBobber = bobberBehaviour;

                    Debug.Log("Found bobber " + bobberBehaviour.position);
                    SetNewTargetPosition(ref fish, bobberBehaviour.position + bobberOffset);
                    bitingFishID = fish.id;
                    break;
                }
            }
        }
        Debug.DrawLine(fish.startPosition, fish.targetPosition, Color.green);
    }

    void DespawnFish(ref ActiveFishData fishData) {
        oldFish.Add(fishData);
    }

    void DoBite(ref ActiveFishData fish) {
        fish.biteCount++;
        bool isBigBite = fish.biteCount > fish.behaviour.minSmallBites && 
                         Random.value < fish.behaviour.bigBiteChance;
        GameObject biteAnimationPrefab  = isBigBite ? bigBiteAnimationPrefab : smallBiteAnimationPrefab;
        if (isBigBite) {
            fish.targetBobber.animator.SetTrigger("BigBite");
        } else {
            fish.targetBobber.animator.SetTrigger("SmallBite");
        }

        PoolManager.PoolInstantiate(biteAnimationPrefab, fish.position + biteNotificationOffset, Quaternion.identity);
        SetNextBiteInterval(ref fish);
    }

    void SetNextBiteInterval(ref ActiveFishData fish) {
        fish.timer = Random.Range(biteIntervalMin, biteIntervalMax);
    }

    public bool GetFishByID(int id, out ActiveFishData fish) {
        if (id < 0) { 
            fish = default(ActiveFishData);
            return false;
        }
        for (int i = 0; i < activeFish.Count; i++) {
            if (activeFish[i].id == id) {
                fish = activeFish[i];
                return true;
            }
        }
        fish = default(ActiveFishData);
        return false;
    }

    const int newPositionCandidateCount = 20;
    Vector2 GetNewTargetPosition(ActiveFishData fish) {
        float radius = Random.Range(newTargetPositionRadiusMin, newTargePositionRadiusMax);
        Vector2 center = spawnRegion.center;
        Vector2 result = fish.position;
        float bestScore = -Mathf.Infinity;
        float angleOffset = Random.value * Mathf.PI * 2f;
        for (int i = 0; i < newPositionCandidateCount; i++) {
            float currentScore = 0f;
            float angle = (i / (float)newPositionCandidateCount) * Mathf.PI * 2f + angleOffset;
            Vector2 candidatePos = (Vector2)fish.position + new Vector2 {
                x = Mathf.Cos(angle) * radius,
                y = Mathf.Sin(angle) * radiusHeightFactor
            };
            // candidatePos = moveRegion.RandomInRect();

            // reject any positions outside of the safe area
            if (!moveRegion.Contains(candidatePos)) { continue; }

            // reject any paths that go under the pier
            if (Physics2D.Linecast(fish.position, candidatePos, spawnAvoidLayers)) {
                continue;
            }

            // score distance to other fish
            float totalDist = 0f;
            for (int j = 0; j < activeFish.Count; j++) {
                if (activeFish[j].id == fish.id) { continue; }
                float dist = (candidatePos - (Vector2)activeFish[j].position).magnitude;
                dist += (candidatePos - (Vector2)activeFish[j].targetPosition).magnitude;
                totalDist += dist;
            }
            currentScore += totalDist / (activeFish.Count - 1f) * avoidOtherFishWeight;

            // score to avoid previous position
            currentScore += (candidatePos - fish.lastTargetPosition).magnitude * avoidPreviousPositionWeight;
            if (GetFishByID(bitingFishID, out var bitingFish)) {
                currentScore += (candidatePos - bitingFish.position).magnitude * avoidBitingFishWeight;
            }

            // remember our best score and candidate position
            if (currentScore >= bestScore) {
                bestScore = currentScore;
                result = candidatePos;
            }
        }
        return result;
    }

    void SetNewTargetPosition(ref ActiveFishData fish, Vector2 targetPosition) {
        fish.startPosition = fish.position;
        fish.targetPosition = targetPosition;
        fish.lastTargetPosition = fish.position;
    }

    void SetNewTargetPosition(ref ActiveFishData fish) {
        Vector2 targetPosition = GetNewTargetPosition(fish);
        SetNewTargetPosition(ref fish, targetPosition);
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
                totalDist += (candidatePos - (Vector2)activeFish[j].position).magnitude;
                totalDist += (candidatePos - activeFish[j].targetPosition).magnitude;
            }
            if (totalDist >= maxDist) {
                maxDist = totalDist;
                fish.startPosition = candidatePos;
                fish.position = candidatePos;
                fish.targetPosition = candidatePos;
            }
        }
    }

    int _fishID;
    public ActiveFishData SpawnFish() {
        ActiveFishData fishData = new ActiveFishData {
            state     = FishState.Idle,
            behaviour = fishBehaviours.GetRandomWithSwapback(),
            id        = _fishID++
        };
        var clone = PoolManager.PoolInstantiate(fishPrefab, Vector2.zero, Quaternion.identity);
        clone.TryGetComponent<Transform>(out fishData.transform);
        clone.TryGetComponent<Animator>(out fishData.animator);

        fishData.transform.SetParent(fishParentTransform, worldPositionStays: true);

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
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public Vector2 lastTargetPosition;
    public Vector2 position {
        get => transform.localPosition;
        set { transform.localPosition = value; }
    }
    public FishBehaviour behaviour;

    public FishState state;
    public int id;
    public int biteCount;
    public float timer;

    public BobberBehaviour targetBobber;
}


public enum FishState {
    Moving, Idle, Biting, Despawning
}

[System.Serializable]
public struct FishBehaviour {
    public float lifeTime;
    public float idleTimerMin;
    public float idleTimerMax;
    public int minSmallBites;
    [Range(0f, 1f)]
    public float bigBiteChance;
    public AnimationCurve moveSpeedCurve;
}
