using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour {

    public LayerMask bobberLayer;
    public int maxFishCount;
    public GameObject fishPrefab;
    public Transform fishParentTransform;
    public BaitManager baitManager;
    public PhoneManager phoneManager;


    [Header("Candidate Fish Generation")]
    public int maxCandidateFish = 3;
    public FishDataObjectList commonFishList;

    [HideInInspector]
    public List<FishDataObject> currentFishList;


    [Header("Fish Behaviour")]
    public float newTargetPositionRadiusMin;
    public float newTargePositionRadiusMax;
    public float newDistanceThreshold = 0.01f;
    public float radiusHeightFactor = 0.5f;
    public float randomMoveChance = 0.05f;

    public float avoidOtherFishWeight = 1f;
    public float avoidPreviousPositionWeight = 1f;
    public float avoidBitingFishWeight = 2f;

    [Header("UI")]
    public GameObject fishPortraitPrefab;
    public Transform fishPortraitParentTransform;
    public float fishPortraitSpacing;
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
    List<float> _spawnQueue;

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
    public float newFishShowPhoneDelay;
    float _buttonDisableTimer;
    public Vector2 bobberOffset;
    public Vector2 biteNotificationOffset;
    public Transform buttonTransform;
    public Vector2 buttonBobberOffset;
    public SpriteRenderer buttonSpriteRenderer;
    public Animator buttonAnimator;
    public GameObject smallBiteAnimationPrefab;
    public GameObject bigBiteAnimationPrefab;
    public YouGotAMatchScreen youGotAMatchScreen;
    

    [Header("Initial Spawn")]
    public int initialFishCount;
    public float initialSpawnDelay;
    public float perFishDelay;
    public float spawnDelayJitter;

    public List<FishBehaviour> fishBehaviours;
    public List<ActiveFishData> activeFish;

    public int bitingFishID = -1;

    List<ActiveFishData> oldFish;
    public float[] baseProbabilities;
    public float[] realProbabilities;
    public ProbabiilityFishPortrait[] fishPortraits;

    [Header("SFX")]
    public AudioClip catchUnlockOneShot;
    public AudioClip catchSuccessOneShot;
    public AudioClip catchFailOneShot;
    public AudioClip biteOneShot;

    [System.NonSerialized] public EnvironmentData environmentData;
    
    void Awake() {

    }

    public void Initialize(EnvironmentData environmentData, GameStateManager gameStateManager) {

        activeFish = new List<ActiveFishData>(16);
        oldFish = new List<ActiveFishData>(16);
        _spawnQueue = new List<float>(64);
        baseProbabilities = new float[maxCandidateFish];
        realProbabilities = new float[maxCandidateFish];
        _randomIndexes = new int[maxCandidateFish];
        fishPortraits = new ProbabiilityFishPortrait[maxCandidateFish];

        for (int i = 0; i < maxCandidateFish; i++) {
            _randomIndexes[i] = i;
        }

        this.environmentData = environmentData;
        currentFishList = new List<FishDataObject>(maxCandidateFish);
        PopulateAllCandidateFish(ref currentFishList, environmentData.fishSpawnList.data);

        // always start with one common
        currentFishList[Random.Range(0, currentFishList.Count)] = commonFishList.data.GetRandomWithSwapback();

        // spawn fish portraits
        Vector3 offset = Vector3.zero;
                fishPortraits = new ProbabiilityFishPortrait[maxCandidateFish];

        UpdateProbabilities();
        for (int i = 0; i < currentFishList.Count; i++) {
            var clone = GameObject.Instantiate(fishPortraitPrefab, fishPortraitParentTransform.position + offset, Quaternion.identity, fishPortraitParentTransform);
            offset += Vector3.down * fishPortraitSpacing;

            if (clone.TryGetComponent<ProbabiilityFishPortrait>(out var fishPortrait)) {
                fishPortrait.gameStateManager = gameStateManager;
                fishPortrait.SetFishDataObject(currentFishList[i], animate: false);
                fishPortraits[i] = fishPortrait;
                fishPortrait.targetProbability = fishPortrait.probability = realProbabilities[i];
            }
        }
    }

    int[] _randomIndexes;
    public int GetRandomFishIndex() {
        _randomIndexes.Shuffle();
        float r = Random.value;
        float cumulativeProbability = 0f;
        for (int i = 0; i < maxCandidateFish; i++) {
            int index = _randomIndexes[i];
            cumulativeProbability += realProbabilities[index];
            if (r < cumulativeProbability) {
                return index;
            }
        }
        return -1;
    }

    public void CatchFish(int fishIndex) {
        CatchFish(currentFishList[fishIndex]);
        _deferredChangeFishIndex = fishIndex;
        fishPortraits[fishIndex].SetFishDataObject(currentFishList[fishIndex]);
    }

    public void CatchFish(FishDataObject fish) {
        _recentlyCaughtFish.Add(fish);
        if (_recentlyCaughtFish.Count > 3) {
            _recentlyCaughtFish.RemoveAt(0);
        }

        fish.data.saveData.numberCaught++;
        if (!fish.data.saveData.unlocked) {
            fish.data.saveData.unlocked = true;
            fish.data.saveData.timeFirstCaughtHours = System.DateTime.Now.Hour;
            fish.data.saveData.timeFirstCaughtMinutes = System.DateTime.Now.Minute;
            // show phone and stuff
            phoneManager.UpdateProfileCell(fish);
            phoneManager.SetTargetProfile(fish);
            youGotAMatchScreen.ShowSuccess(fish, goToPhone: true);
            OneShotManager.PlayOneShot(catchUnlockOneShot);
        } else {
            youGotAMatchScreen.ShowSuccess(fish);
            OneShotManager.PlayOneShot(catchSuccessOneShot);
        }
    }

    public void MissFish(int fishIndex) {
        MissFish(currentFishList[fishIndex]);
        ChangeFish(fishIndex);
        youGotAMatchScreen.ShowFail();
        OneShotManager.PlayOneShot(catchFailOneShot);
    }
    public void MissFish(FishDataObject fish) {
        fish.data.saveData.numberMissed++;
    }

    public void ChangeFish(int fishIndex) {
        currentFishList.RemoveAt(fishIndex);
        List<FishDataObject> environmentList = environmentData.fishSpawnList.data;
        FishDataObject newFish = GetCandidateFish(currentFishList, environmentList);
        currentFishList.Insert(fishIndex, newFish);
        var portrait = fishPortraits[fishIndex];
        portrait.SetFishDataObject(newFish, animate: true);
    }

    [System.NonSerialized] int _deferredChangeFishIndex = -1;
    public void ChangeFishDeferred() {
        if (_deferredChangeFishIndex >= 0) {
            ChangeFish(_deferredChangeFishIndex);
        }
        _deferredChangeFishIndex = -1;
    }

    void OnEnable() {
        activeFish.Clear();
        buttonSpriteRenderer.enabled = false;
    }

    void Start() {
        SetupInitialSpawn();
    }

    void Update() {
        for (int i = 0; i < _spawnQueue.Count; i++) {
            float t = _spawnQueue[i];
            t -= Time.deltaTime;
            if (t <= 0f) {
                activeFish.Add(SpawnFish());
                _spawnQueue.RemoveAt(i);
                i--;
            } else {
                _spawnQueue[i] = t;
            }
        }

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
        if (activeFish.Count + _spawnQueue.Count < maxFishCount) {
            float shortInterval = Random.Range(shortSpawnIntervalMin, shortSpawnIntervalMax);
            float longInterval = Random.Range(longSpawnIntervalMin, longSpawnIntervalMax);
            float spawnInterval = shortInterval;
            if (activeFish.Count == 0) {
                spawnInterval = longInterval;
            } else if (Random.value < 0.5f) {
                spawnInterval = longInterval;
            }
            _spawnQueue.Add(spawnInterval);
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

        UpdateProbabilities();
        for (int i = 0; i < maxCandidateFish; i++) {
            fishPortraits[i].targetProbability = realProbabilities[i];
        }
    }

    void UpdateProbabilities() {
        // update fish percentages
        FishData fishData;
        float baseProbability = 0f;
        float totalProbability = 0f;
        for (int i = 0; i < maxCandidateFish; i++) {
            fishData = currentFishList[i].data;
            baseProbability = fishData.basePercentage;
            for (int j = 0; j < (fishData.favoriteBait?.Count ?? 0); j++) {
                if (fishData.favoriteBait[j].baitDataObject == baitManager.selectedBait) {
                    baseProbability += fishData.favoriteBait[j].percentageBoost;
                }
            }
            baseProbabilities[i] = baseProbability;
            totalProbability += baseProbability;
        }
        for (int i = 0; i < maxCandidateFish; i++) {
            realProbabilities[i] = baseProbabilities[i] / totalProbability;
        }
    }

    List<FishDataObject> _recentlyCaughtFish = new List<FishDataObject>(4);
    void PopulateAllCandidateFish(ref List<FishDataObject> candidateList, List<FishDataObject> environmentList) {
        candidateList.Clear();
        for (int i = 0; i < maxCandidateFish; i++) {
            var newFish = GetCandidateFish(candidateList, environmentList);
            if (newFish) {
                candidateList.Add(newFish);
            }
        }
        candidateList.Shuffle();
    }

    FishDataObject GetCandidateFish(List<FishDataObject> candidateList, List<FishDataObject> environmentList) {
        // try and get a fish from the environment list...
        var newCandidate = PickCandidateFish(environmentList, candidateList);
        
        // failing that, get a fish from the common list
        if (!newCandidate) {
            newCandidate = PickCandidateFish(commonFishList.data, candidateList);
        }

        // failing THAT, get a fish from the junk list (not yet implemented)...
        return newCandidate;
    }


    FishDataObject PickCandidateFish(List<FishDataObject> candidateList, List<FishDataObject> omitList) {
        int hour = System.DateTime.Now.Hour;
        FishDataObject result;
        candidateList.Shuffle();

        for (int i = 0; i < candidateList.Count; i++) {
            var current = candidateList[i];

            if (_recentlyCaughtFish.Contains(current)) { continue; }

            // reject any fish already present in the candidate list
            if (omitList.Contains(current)) { continue; }

            // iterate over current's time ranges, if one overlaps the current time
            // then this is a valid candidate and return
            if ((current.data.timeRanges?.Count ?? 0) == 0) {
                Debug.Log(current.data.name + " NO TIME RaNGE" );
                return current;
            }
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

    void SetupInitialSpawn() {
        float t = initialSpawnDelay;
        for (int i = 0; i < initialFishCount; i++) {
            _spawnQueue.Add(t);
            t += perFishDelay + Random.value * spawnDelayJitter;
        }
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
        float bigBiteChance = fish.behaviour.bigBiteChance;
        if (_successfulSmallBites > 10) {
            bigBiteChance += 0.1f;
        }
        bool isBigBite = _successfulSmallBites > fish.behaviour.minSmallBites && Random.value < bigBiteChance;
        if (isBigBite) {
            _bigBiteSweetSpotTimer = bigBiteSweetSpotDuration;
            biteAnimationPrefab = bigBiteAnimationPrefab;
            fish.targetBobber.animator.SetTrigger("BigBite");
        } else {
            _smallBiteSweetSpotTimer = smallBiteSweetSpotDuration;
            biteAnimationPrefab = smallBiteAnimationPrefab;
            fish.targetBobber.animator.SetTrigger("SmallBite");
        }
        buttonAnimator.SetTrigger("Show");
        PoolManager.PoolInstantiate(biteAnimationPrefab, fish.position + biteNotificationOffset, Quaternion.identity);
        SetNextBiteInterval(ref fish);
        OneShotManager.PlayOneShot(biteOneShot);
    }

    void SetNextBiteInterval(ref ActiveFishData fish) {
        fish.timer = Random.Range(biteIntervalMin, biteIntervalMax);
    }

    public GameStateManager gameStateManager;
    public void EndBiteSequence(CatchResult result) {
        float buttonDisableDelay = 0f;
        switch (result) {
        
        case CatchResult.Success:
            CatchFish(GetRandomFishIndex());
        break;

        case CatchResult.Fail:
            buttonDisableDelay = failButtonDisableDelay;
            MissFish(GetRandomFishIndex());
        break;
        }

        if (buttonDisableDelay == 0f) {
            buttonSpriteRenderer.enabled = false;
        } else {
            _buttonDisableTimer = buttonDisableDelay;
        }
        DespawnFish(bitingFishID);
        bitingFishID = -1;
        gameStateManager.SaveGame();
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

                dist = (candidatePos - (Vector2)activeFish[j].position).magnitude * 0.25f;
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
            id        = _fishID++,
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
    public enum CatchResult {
        Success, Fail
    }
    [HideInInspector] public int _successfulSmallBites;
    [HideInInspector] public int _failedBites = 0;
    public BiteResult TryGetBite() {
        BiteResult result;
        
        if (bitingFishID < 0) {
            return BiteResult.Invalid;
        }
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
                EndBiteSequence(CatchResult.Fail);
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


public struct FishChanceData {
    public FishDataObject fish;
    public float currentChance;
}
