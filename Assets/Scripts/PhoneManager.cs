using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour {
    public Animator phoneAnimator;
    public GameObject scrollViewItemPrefab;
    public Transform scrollViewContentTransform;
    public BobberAimBehaviour aimBehaviour;
    public Dictionary<FishDataObject, FishProfileCell> cellDict;
    public List<FishProfileCell> cellList;
    public RectTransform contentTransform;
    public ScrollRect scrollRect;
    public RodBehaviour rodBehaviour;

    [System.NonSerialized]
    public bool phoneEnabled;

    [Header("Scroll Animation")]
    public AnimationCurve scrollCurve;
    public float scrollAnimationTime;
    bool _animatingScroll;
    float _scrollAnimationT;
    float _scrollStart;
    float _scrollTarget;

    [Header("Screens")]
    public GameObject matchesScreen;
    public GameObject newMatchScreen;
    public GameObject profileScreen;


    [System.NonSerialized] public PhoneState phoneState;
    float _showTimer;

    public void Initialize(List<FishDataObject> fishList) {

        // set up scroll view items
        cellDict = new Dictionary<FishDataObject, FishProfileCell>(fishList.Count);
        cellList = new List<FishProfileCell>(fishList.Count);
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
                cellList.Add(cell);
            }
        }
    }

    public void ScrollToFishProfile(FishDataObject fish, bool animate = true) {
        if (cellDict.TryGetValue(fish, out var profileCell)) {
            _scrollStart = scrollRect.verticalNormalizedPosition;
            _scrollTarget = scrollRect.GetTargetScrollValue(profileCell.rectTransform); 
            _scrollAnimationT = 0f;
            _animatingScroll = true;
        }
    }

    public void UpdateProfileCell(FishDataObject fishDataObject) {
        if (cellDict.TryGetValue(fishDataObject, out var profileCell)) {
            profileCell.Initialize(profileCell.fishDataObject);
        }
    }

    void OnEnable() {
        phoneEnabled = false;
    }

    public int testElementIndex;
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!phoneEnabled) {
                Show();
            } else {
                Hide();
            }
        }

        if (_showTimer >= 0f) {
            _showTimer -= Time.deltaTime;
            if (_showTimer >= 0f) {
                Show();
            }
        }

        if (_animatingScroll) {
            _scrollAnimationT += Time.deltaTime / scrollAnimationTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(_scrollStart, _scrollTarget, scrollCurve.Evaluate(_scrollAnimationT));
            if (_scrollAnimationT >= 1f) {
                _animatingScroll = false;
            }
        }
    }

    public void Show(float delay = 0f) {
        if (delay > 0f) {
            _showTimer = delay;
            return;
        }
        phoneEnabled = true;
        phoneAnimator.SetTrigger("PhoneTransition");
        phoneAnimator.SetBool("PhoneEnabled", true);
        aimBehaviour.enabled = false;
    }

    public void ShowNewMatchScreen() {
        matchesScreen.SetActive(false);
        profileScreen.SetActive(false);
        newMatchScreen.SetActive(true);
    }

    public void ShowProfileScreen() {
        matchesScreen.SetActive(false);
        newMatchScreen.SetActive(false);
        profileScreen.SetActive(true);
    }

    public void ShowMatches() {
        profileScreen.SetActive(false);
        newMatchScreen.SetActive(false);
        matchesScreen.SetActive(true);
    }

    public void Hide() {
        phoneEnabled = false;
        phoneAnimator.SetTrigger("PhoneTransition");
        phoneAnimator.SetBool("PhoneEnabled", false);
        aimBehaviour.enabled = true;
    }


    public void HandleInput(ButtonInput input) {
        switch(phoneState) {
            case PhoneState.Matches:
            if (input == ButtonInput.A) {

            }
            else if (input == ButtonInput.B) {

            }
            break;

            case PhoneState.NewMatch:
            if (input == ButtonInput.A) {

            }
            else if (input == ButtonInput.B) {

            }
            break;

            case PhoneState.Profile:
            if (input == ButtonInput.B) {

            }
            break;
        }
    }
}

public enum PhoneState {
    NewMatch, Matches, Profile
}



public static class UIExtensions {
    
    static Vector3[] corners = new Vector3[4];
    public static Bounds TransformBoundsTo(this RectTransform source, Transform target) {
        var bounds = new Bounds();
        if (source != null) {
            source.GetWorldCorners(corners);

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var matrix = target.worldToLocalMatrix;
            for (int j = 0; j < 4; j++) {
                Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
        }
        return bounds;
    }
    public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance) {
        // Based on code in ScrollRect's internal SetNormalizedPosition method
        var viewport = scrollRect.viewport;
        var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        var content = scrollRect.content;
        var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

        var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
        return distance / hiddenLength;
    }

    public static void CenterTransform(this ScrollRect scrollRect, RectTransform target) {
        scrollRect.verticalNormalizedPosition = scrollRect.GetTargetScrollValue(target);
    }

    public static float GetTargetScrollValue(this ScrollRect scrollRect, RectTransform target) {
        // The scroll rect's view's space is used to calculate scroll position
        var view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

        // Calcualte the scroll offset in the view's space
        var viewRect = view.rect;
        var elementBounds = target.TransformBoundsTo(view);
        var offset = viewRect.center.y - elementBounds.center.y;

        // Normalize and apply the calculated offset
        var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
        return Mathf.Clamp(scrollPos, 0f, 1f);
    }
}