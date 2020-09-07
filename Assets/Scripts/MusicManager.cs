using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MusicManager: MonoBehaviour {
    public List<AudioClip> chordsClips;
    public List<AudioClip> melodiesClips;
    public List<AudioClip> arpbassClips;
    public int medleyTransitionDuration = 14;
    public GameObject audioSourcePrefab;

    [System.Serializable]
    public struct Medley {
        public AudioSource chordsSource;
        public AudioSource melodiesSource;
        public AudioSource arpBassSource;

        public void Play() {
            chordsSource.Play();
            melodiesSource.Play();
            arpBassSource.Play();
        }

        public float remainingTime => Mathf.Min(chordsSource.GetRemainingTime(), Mathf.Min(melodiesSource.GetRemainingTime(), arpBassSource.GetRemainingTime()));
        public bool isPlaying => chordsSource.isPlaying || melodiesSource.isPlaying || arpBassSource.isPlaying;
    }
    List<Medley> _inFlightMedleys;
    Medley CreateNewMedley(bool play = true) {
        GameObject clone;
        Medley medley = new Medley();
        clone = PoolManager.PoolInstantiate(audioSourcePrefab, Vector3.zero, Quaternion.identity);
        clone.transform.SetParent(this.transform);
        clone.TryGetComponent<AudioSource>(out medley.chordsSource);
        medley.chordsSource.clip = chordsClips.GetRandomWithSwapback();

        clone = PoolManager.PoolInstantiate(audioSourcePrefab, Vector3.zero, Quaternion.identity);
        clone.transform.SetParent(this.transform);
        clone.TryGetComponent<AudioSource>(out medley.melodiesSource);
        medley.melodiesSource.clip = melodiesClips.GetRandomWithSwapback();

        clone = PoolManager.PoolInstantiate(audioSourcePrefab, Vector3.zero, Quaternion.identity);
        clone.transform.SetParent(this.transform);
        clone.TryGetComponent<AudioSource>(out medley.arpBassSource);
        medley.arpBassSource.clip = arpbassClips.GetRandomWithSwapback();

        if (play) { medley.Play(); }
        return medley;
    }

    void OnEnable() {
        _inFlightMedleys = new List<Medley>(4);
        _inFlightMedleys.Add(CreateNewMedley(play: true));
    }

    void PoolMedley(Medley medley) {
        PoolManager.PoolDestroy(medley.chordsSource.gameObject);
        PoolManager.PoolDestroy(medley.melodiesSource.gameObject);
        PoolManager.PoolDestroy(medley.arpBassSource.gameObject);
    }

    void Update() {
        for (int i = 0; i < _inFlightMedleys.Count; i++) {
            if (_inFlightMedleys[i].remainingTime < medleyTransitionDuration && _inFlightMedleys.Count < 2) {
                _inFlightMedleys.Add(CreateNewMedley(play: true));
            }
            if (!_inFlightMedleys[i].isPlaying) {
                PoolMedley(_inFlightMedleys[i]);
                _inFlightMedleys.RemoveAt(i);
                i--;
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void MusicBootstrap() {
        const int MUSIC_SCENE_INDEX = 1;
        SceneManager.LoadScene(MUSIC_SCENE_INDEX, LoadSceneMode.Additive);
    }
}

public static class AudioSourceExtensions {
    public static float GetRemainingTime(this AudioSource audioSource) {
        return audioSource.clip.length - audioSource.time;
    }
}

