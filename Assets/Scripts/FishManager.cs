using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour {

    public LayerMask bobberLayer;
    public int maxFishCount;
    public GameObject fishPrefab;
    public Transform fishParentTransform;

    [Header("Candidate Fish Generation")]
    public int maxCandidateFish = 3;
    public EnvironmentDataObject activeEnvironmentDataObject;
    public FishDataObjectList commonFishList;

    [HideInInspector]
    public List<FishDataObject> currentPotentialFishList;


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
    public float despawnJitterMin = 0.1f;
    public float despawnJitterMax = 0.3f;

    public float shortSpawnIntervalMin;
    public float shortSpawnIntervalMax;

    public float longSpawnIntervalMin;
    public float longSpawnIntervalMax;

    float _queuedFishTimer;
    int _queuedFishCount;

    [Header("Fish Despawning")]
    public float poolDelay;
    public float bobberDespawnRadius;

    [Header("Bite Behaviour")]
    public float biteIntervalMin;
    public float biteIntervalMax;
    public float bobberLookRadius;
    public float smallBiteSweetSpotDuration;
    public float bigBiteSweetSpotDuration;
    public float failButtonDisableDelay;
    float _buttonDisableTimer;
    public Vector2 bobberOffset;
    public Vector2 biteNotificationOffset;
    public Transform buttonTransform;
    public Vector2 buttonBobberOffset;
    public SpriteRenderer buttonSpriteRenderer;
    
    public GameObject smallBiteAnimationPrefab;
    public GameObject bigBiteAnimationPrefab;
    

    [Header("Initial Spawn")]
    public int initialFishCount;
    public float initialSpawnDelay;
    public float perFishDelay;
    public float spawnDelayJitter;

    public List<FishBehaviour> fishBehaviours;
    public List<ActiveFishData> activeFish;

    public int bitingFishID = -1;

    List<ActiveFishData> oldFish;


    void Awake() {
        activeFish = new List<ActiveFishData>(16);
        oldFish = new List<ActiveFishData>(16);
    }

    void Start() {
        currentPotentialFishList = new List<FishDataObject>(maxCandidateFish);
        // PopulateAllCandidates(ref currentPotentialFishList);
    }

    void OnEnable() {
        activeFish.Clear();
        StartCoroutine(InitialSpawnRoutine());
        buttonSpriteRenderer.enabled = false;
    }

    void Update() {
        ActiveFishData fish;
        for (int i = 0; i < activeFish.Count; i++) {
            fish = activeFish[i];
            UpdateFish(ref fish);
            activeFish[i] = fish;
        }

        for (int i = 0; i < oldFish.Count; i++) {
            fish = oldFish[i];
            if (fish.timer > poolDelay && fish.timer - Time.deltaTime < poolDelay) { // lol hax
                fish.animator.SetTrigger("Despawn");
            }
            fish.timer -= Time.deltaTime;
            if (fish.timer <= 0f) {
                PoolManager.PoolDestroy(fish.transform.gameObject);
                oldFish.RemoveAt(i);
            } else {
                oldFish[i] = fish;
            }
        } 
        // spawn new fish if needed
        if (activeFish.Count < maxFishCount && _queuedFishTimer <= 0f) {
            float shortInterval = Random.Range(shortSpawnIntervalMin, shortSpawnIntervalMax);
            float longInterval = Random.Range(longSpawnIntervalMin, longSpawnIntervalMax);
            float spawnInterval = shortInterval;
            if (activeFish.Count + _queuedFishCount == 0) {
                spawnInterval = longInterval;
            } else if (Random.value < 0.5f) {
                spawnInterval = longInterval;
            }
            _queuedFishTimer += spawnInterval;
        }

        if (_queuedFishTimer > 0f) {
            _queuedFishTimer -= Time.deltaTime;
            if (_queuedFishTimer <= 0f) {
                activeFish.Add(SpawnFish());
            }
        }

        if (_bigBiteSweetSpotTimer > 0f) {
            _bigBiteSweetSpotTimer -= Time.deltaTime;
            _bigBiteSweetSpotTimer = Mathf.Max(0f, _bigBiteSweetSpotTimer);
        }
        if (_smallBiteSweetSpotTimer > 0f) {
            _smallBiteSweetSpotTimer -= Time.deltaTime;
            _smallBiteSweetSpotTimer = Mathf.Max(0f, _smallBiteSweetSpotTimer); 
        }

        if (_buttonDisableTimer > 0f) {
            _buttonDisableTimer -= Time.deltaTime;
            if (_buttonDisableTimer <= 0f) {
                buttonSpriteRenderer.enabled = false;
            }
        }
    }

    void PopulateAllCandidates(ref List<FishDataObject> candidateList) {
        candidateList.Clear();
        for (int i = 0; i < maxCandidateFish; i++) {
            PopulateCandidate(ref candidateList, i);
        }
    }

    void PopulateCandidate(ref List<FishDataObject> candidateList, int index) {
        // try and get a fish from the environment list...
        var newCandidate = GetCandidateFish(activeEnvironmentDataObject.data.fishSpawnList.data, candidateList);
        
        // failing that, get a fish from the common list
        if (!newCandidate) {
            newCandidate = GetCandidateFish(commonFishList.data, candidateList);
        }

        // failing THAT, get a fish from the junk list (not yet implemented)...


        if (newCandidate) {
            candidateList.Add(newCandidate);
        } else {
            Debug.LogError("COULDN'T FIND A CANDIDATE FISH! THIS SHOULDN'T HAPPEN");
        }
    }

    FishDataObject GetCandidateFish(List<FishDataObject> fishDataObjectList, List<FishDataObject> candidateList) {
        int hour = System.DateTime.Now.Hour;
        FishDataObject result;
        fishDataObjectList.Shuffle();
        for (int i = 0; i < fishDataObjectList.Count; i++) {
            var current = fishDataObjectList[i];

            // reject any fish already present in the candidate list
            if (candidateList.Contains(current)) { continue; }

            // iterate over current's time ranges, if one overlaps the current time
            // then this is a valid candidate and return
            for (int j = 0; j < current.data.timeRanges.Count; j++) {
                var timeRange = current.data.timeRanges[j];
                if (timeRange.ContainsTime(hour)) {
                    result = current;
                    return result;
                }
            }
        }
        return null;
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

    Collider2D[] _overlapResults = new Collider2D[8];
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
                if (fish.targetBobber && fish.targetBobber.isInWater) {
                    // set up bite minigame state, should prob be its own method...
                    // also should probably contain a lot of this state in the ActiveFishData struct
                    fish.state = FishState.Biting;
                    SetNextBiteInterval(ref fish);
                    buttonTransform.position = fish.targetBobber.position + buttonBobberOffset;
                    _smallBiteSweetSpotTimer = 0f;
                    _bigBiteSweetSpotTimer = 0f;
                    _successfulSmallBites = 0;
                    _failedBites = 0;
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
        if (bitingFishID < 0 && fish.state != FishState.Despawning) {
            int overlapCount = Physics2D.OverlapCircleNonAlloc(fish.position, bobberLookRadius, _overlapResults);
            for (int i = 0; i < overlapCount; i++) {
                if (_overlapResults[i].TryGetComponent<BobberBehaviour>(out var bobberBehaviour)) {
                    if (!bobberBehaviour.isInWater) { continue; }
                    SetNewTargetPosition(ref fish, bobberBehaviour.position + bobberOffset);
                    fish.state = FishState.Moving;
                    fish.targetBobber = bobberBehaviour;

                    Vector2 offset = bobberOffset;
                    offset.x = bobberBehaviour.position.x < 0f ? offset.x : -offset.x;
                    bitingFishID = fish.id;
                    break;
                }
            }
        }
        Debug.DrawLine(fish.startPosition, fish.targetPosition, Color.green);
    }

    void DespawnFish(int id) {
        if (TryGetFishByID(bitingFishID, out var index, out var fish)) {
            DespawnFish(ref fish);
            activeFish.RemoveAt(index);
        }
    }

    void DespawnFish(ref ActiveFishData fish, float extraDelay = 0f) {
        fish.state = FishState.Despawning;
        fish.timer = poolDelay + extraDelay;
        oldFish.Add(fish);
    }

    public bool HandleReel() {
        buttonSpriteRenderer.enabled = false;
        ActiveFishData fish;
        // despawn any extra fish near the bobber
        for (int i = 0; i < activeFish.Count; i++) {
            fish = activeFish[i];
            // if (fish.id == bitingFishID) { continue; }
            if (Physics2D.OverlapCircle(fish.position, bobberDespawnRadius, bobberLayer)) {
                activeFish.RemoveAt(i);
                i--;
                DespawnFish(ref fish, extraDelay: Random.Range(despawnJitterMin, despawnJitterMax));
            }
        }
        
        if (TryGetFishByID(bitingFishID, out var index, out fish)) {
            DespawnFish(ref fish);
            activeFish.RemoveAt(index);
        }

        bitingFishID = -1;
        return false;
    }

    void DoBite(ref ActiveFishData fish) {
        fish.biteCount++;
        buttonSpriteRenderer.enabled = true;
        GameObject biteAnimationPrefab;
        bool isBigBite = _successfulSmallBites > fish.behaviour.minSmallBites && Random.value < fish.behaviour.bigBiteChance;
        if (isBigBite) {
            _bigBiteSweetSpotTimer = bigBiteSweetSpotDuration;
            biteAnimationPrefab = bigBiteAnimationPrefab;
            fish.targetBobber.animator.SetTrigger("BigBite");
        } else {
            _smallBiteSweetSpotTimer = smallBiteSweetSpotDuration;
            biteAnimationPrefab = smallBiteAnimationPrefab;
            fish.targetBobber.animator.SetTrigger("SmallBite");
        }
        PoolManager.PoolInstantiate(biteAnimationPrefab, fish.position + biteNotificationOffset, Quaternion.identity);
        SetNextBiteInterval(ref fish);
    }

    void SetNextBiteInterval(ref ActiveFishData fish) {
        fish.timer = Random.Range(biteIntervalMin, biteIntervalMax);
    }

    public void EndBiteSequence(BiteResult result) {
        float buttonDisableDelay = 0f;
        if (result == BiteResult.BiteFail) {
            buttonDisableDelay = failButtonDisableDelay;
        }
        if (buttonDisableDelay == 0f) {
            buttonSpriteRenderer.enabled = false;
        } else {
            _buttonDisableTimer = buttonDisableDelay;
        }
        DespawnFish(bitingFishID);
        bitingFishID = -1;
    }

    public bool TryGetFishByID(int id, out int index, out ActiveFishData fish) {
        index = -1;
        if (id < 0) { 
            fish = default(ActiveFishData);
            return false;
        }
        for (int i = 0; i < activeFish.Count; i++) {
            if (activeFish[i].id == id) {
                fish = activeFish[i];
                index = i;
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
                // float dist = (candidatePos - (Vector2)activeFish[j].position).magnitude;
                float dist = (candidatePos - (Vector2)activeFish[j].targetPosition).magnitude;
                totalDist += dist;
            }
            currentScore += (totalDist / (activeFish.Count - 1f)) * avoidOtherFishWeight;

            // score to avoid previous position
            currentScore += (candidatePos - fish.lastTargetPosition).magnitude * avoidPreviousPositionWeight;
            if (TryGetFishByID(bitingFishID, out _, out var bitingFish)) {
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
        const int initialPositionCandidateCount = 10;
        float maxDist = 0f;
        int tries = 0;
        while(tries < initialPositionCandidateCount) {
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
            tries++;
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



    [HideInInspector] public float _smallBiteSweetSpotTimer;
    [HideInInspector] public float _bigBiteSweetSpotTimer;
    public enum BiteResult {
        SmallBiteSuccess, BigBiteSuccess, BiteFail, Invalid
    }
    [HideInInspector] public int _successfulSmallBites;
    [HideInInspector] public int _failedBites = 0;
    public BiteResult TryGetBite() {
        BiteResult result;
        if (bitingFishID < 0) { return BiteResult.Invalid; }
        else if (_bigBiteSweetSpotTimer > 0f) {
            result = BiteResult.BigBiteSuccess;
            if (TryGetFishByID(bitingFishID, out var index, out var fish)) {
                fish.state = FishState.OnHook;
                fish.timer = 0f;
                activeFish[index] = fish;
            }
        }
        else if (_smallBiteSweetSpotTimer > 0f) {
            result = BiteResult.SmallBiteSuccess;
            _successfulSmallBites++;
        } 
        else {
            result = BiteResult.BiteFail;
            _failedBites++;
            if (_failedBites == 3) {
                EndBiteSequence(result);
            }
        }
        return result;
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
    Moving, Idle, Biting, OnHook, Despawning
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
