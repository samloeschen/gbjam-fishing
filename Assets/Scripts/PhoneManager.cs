﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public FishProfileCell selectedProfileCell;

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

    [Header("New Match Screen")]
    public Image newMatchScreenSprite;


    [Header("Profile Screen")]
    public Image profileScreenSprite;
    public TextMeshProUGUI nameTMP;
    public TextMeshProUGUI blurbTMP;
    public TextMeshProUGUI caughtValueTMP;
    public TextMeshProUGUI missedValueTMP;
    public TextMeshProUGUI timeTMP;

    CharArray _charArray;

    [System.NonSerialized] public PhoneScreen phoneScreen;
    float _showTimer;

    PhoneScreen _nextOpenScreen;

    [System.NonSerialized] FishDataObject _targetProfile;

    public event Action<PhoneAnimationEvent> onAnimationEvent;


    void Awake( ){
        _charArray = new CharArray(256);
        onAnimationEvent += (PhoneAnimationEvent e) => {
            switch (e) {
                case PhoneAnimationEvent.PhoneOpenStart:
                    switch (_nextOpenScreen) {
                    case PhoneScreen.Matches:
                        ShowMatches();
                    break;

                    case PhoneScreen.NewMatch:
                        ShowNewMatchScreen(_targetProfile);
                    break;

                    case PhoneScreen.Profile:
                        ShowProfileScreen(_targetProfile);
                    
                    break;
                }
                break;
            }
        };
    }

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
        ShowMatches();
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

    public void SetTargetProfile(FishDataObject profile) {
        _targetProfile = profile;
    }

    void LoadProfileData(FishDataObject fish) {
        profileScreenSprite.sprite = fish.data.profileSprite;
        nameTMP.text = fish.data.name;
        blurbTMP.text = fish.data.profileText;

        _charArray.Clear();
        _charArray.Append((int)Mathf.Clamp(fish.data.saveData.numberCaught, 0, 999));
        caughtValueTMP.SetCharArray(_charArray.GetArray(), 0, _charArray.count);

        _charArray.Clear();
        _charArray.Append((int)Mathf.Clamp(fish.data.saveData.numberMissed, 0, 999));
        missedValueTMP.SetCharArray(_charArray.GetArray(), 0, _charArray.count);

        _charArray.Clear();
        _charArray.Append(fish.data.saveData.timeFirstCaughtHours);
        _charArray.Append(':');
        _charArray.Append(fish.data.saveData.timeFirstCaughtMinutes);
        timeTMP.SetCharArray(_charArray.GetArray(), 0, _charArray.count);
    }

    void OnEnable() {
        phoneEnabled = false;
    }

    public int testElementIndex;
    void Update() {
        // if (Input.GetKeyDown(KeyCode.Space)) {
        //     if (!phoneEnabled) {
        //         ShowPhone(PhoneScreen.Matches);
        //     } else {
        //         HidePhone();
        //     }
        // }

        HandleInput();

        if (_showTimer >= 0f) {
            _showTimer -= Time.deltaTime;
            if (_showTimer >= 0f) {
                ShowPhone(PhoneScreen.NewMatch);
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

    public void PhoneAnimationEventHook(PhoneAnimationEvent e) {
        if (onAnimationEvent != null) { onAnimationEvent(e); }
    }

    public void ShowPhone(PhoneScreen screen, float delay = 0f) {
        if (delay > 0f) {
            _showTimer = delay;
            return;
        }
        _nextOpenScreen = screen;
        phoneEnabled = true;
        phoneAnimator.SetTrigger("PhoneTransition");
        phoneAnimator.SetBool("PhoneEnabled", true);
        aimBehaviour.enabled = false;
    }

    public void ShowNewMatchScreen(FishDataObject fishProfile) {
        phoneScreen = PhoneScreen.NewMatch;
        newMatchScreenSprite.sprite = fishProfile.data.profileSprite;
        matchesScreen.SetActive(false);
        profileScreen.SetActive(false);
        newMatchScreen.SetActive(true);
    }

    public void ShowProfileScreen(FishDataObject fishProfile) {
        phoneScreen = PhoneScreen.Profile;
        LoadProfileData(fishProfile);
        matchesScreen.SetActive(false);
        newMatchScreen.SetActive(false);
        profileScreen.SetActive(true);
    }

    public void ShowMatches() {
        phoneScreen = PhoneScreen.Matches;
        if (_targetProfile != null) {
            SnapToCell(_targetProfile);
        }
        profileScreen.SetActive(false);
        newMatchScreen.SetActive(false);
        matchesScreen.SetActive(true);
    }

    public void ShowMatches(FishDataObject profile) {
        _targetProfile = profile;
        ShowMatches();
    }

    public void SnapToCell(FishDataObject targetProfile) {

    }

    public void HidePhone() {
        phoneEnabled = false;
        phoneAnimator.SetTrigger("PhoneTransition");
        phoneAnimator.SetBool("PhoneEnabled", false);
        aimBehaviour.enabled = true;
    }

    public void HandleInput() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (phoneEnabled) {
                ShowPhone(PhoneScreen.Matches);
                _nextOpenScreen = PhoneScreen.Matches;
            } else {
                HidePhone();
            }
        }

        if (phoneEnabled) {
            switch(phoneScreen) {
            case PhoneScreen.Matches:
                if (Input.GetKeyDown(KeyCode.Z)) {
                    ShowProfileScreen(selectedProfileCell.fishDataObject);
                }
                else if (Input.GetKeyDown(KeyCode.X)) {
                    HidePhone();
                }
            break;

            case PhoneScreen.NewMatch:
                if (Input.GetKeyDown(KeyCode.Z)) {
                    ShowProfileScreen(_targetProfile);
                }
                else if (Input.GetKeyDown(KeyCode.X)) {
                    ShowMatches(_targetProfile);
                }
            break;

            case PhoneScreen.Profile:
                if (Input.GetKeyDown(KeyCode.X)) {
                    ShowMatches();
                }
            break;
            }
        }
    }
}

public enum PhoneScreen {
    NewMatch, Matches, Profile
}

public enum PhoneAnimationEvent {
    PhoneOpenStart
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