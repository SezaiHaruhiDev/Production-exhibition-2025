using UnityEngine;
using System.Collections;

/// <summary>
/// サウンド管理（BGM/SE）
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SoundManager");
                _instance = go.AddComponent<SoundManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private AudioSource _seSource;
    private AudioSource _bgmSource;

    private const string PathSE = "se/";
    private const string PathBGM = "bgm/";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        if (_seSource == null) _seSource = gameObject.AddComponent<AudioSource>();
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// 初期化コルーチン
    /// </summary>
    public IEnumerator InitializeCoroutine()
    {
        SetupAudioSources();
        yield break;
    }

    /// <summary>
    /// SE再生 (Resources/SE)
    /// </summary>
    public void PlaySE(string seName, float volume = 1f)
    {
        var clip = Resources.Load<AudioClip>(PathSE + seName);
        if (clip != null)
        {
            _seSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: SE '{seName}' not found.");
        }
    }

    /// <summary>
    /// SE再生 (Clip直接指定)
    /// </summary>
    public void PlaySE(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _seSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    public void PlayBGM(string bgmName, float volume = 1f, float transitionTime = 0.5f)
    {
        var clip = Resources.Load<AudioClip>(PathBGM + bgmName);
        if (clip == null)
        {
            Debug.LogError($"SoundManager: BGM '{bgmName}' not found.");
            return;
        }

        if (_bgmSource.clip == clip && _bgmSource.isPlaying)
        {
            StartCoroutine(FadeVolume(volume, transitionTime));
            return;
        }

        StartCoroutine(PlayBGMCoroutine(clip, volume, transitionTime));
    }

    public void StopBGM(float fadeTime = 0.5f)
    {
        StartCoroutine(StopBGMCoroutine(fadeTime));
    }

    public void ResumeBGM(float fadeTime = 0.5f)
    {
        if (_bgmSource.clip != null && !_bgmSource.isPlaying)
        {
            _bgmSource.volume = 0f;
            _bgmSource.Play();
            StartCoroutine(FadeVolume(1f, fadeTime));
        }
    }

    public void SetBGMVolume(float targetVolume, float fadeTime = 0.5f)
    {
        StartCoroutine(FadeVolume(targetVolume, fadeTime));
    }

    public void PauseBGM(float fadeTime = 0.5f)
    {
        StartCoroutine(PauseBGMCoroutine(fadeTime));
    }

    public void UnpauseBGM(float fadeTime = 0.5f)
    {
        StartCoroutine(UnpauseBGMCoroutine(fadeTime));
    }

    private IEnumerator PlayBGMCoroutine(AudioClip newClip, float targetVolume, float fadeTime)
    {
        if (_bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(0f, fadeTime));
            _bgmSource.Stop();
        }

        _bgmSource.clip = newClip;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        yield return StartCoroutine(FadeVolume(targetVolume, fadeTime));
    }

    private IEnumerator StopBGMCoroutine(float fadeTime)
    {
        if (_bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(0f, fadeTime));
            _bgmSource.Stop();
        }
    }

    private IEnumerator PauseBGMCoroutine(float fadeTime)
    {
        yield return StartCoroutine(FadeVolume(0f, fadeTime));
        _bgmSource.Pause();
    }

    private IEnumerator UnpauseBGMCoroutine(float fadeTime)
    {
        _bgmSource.UnPause();
        yield return StartCoroutine(FadeVolume(1f, fadeTime));
    }

    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        _bgmSource.volume = targetVolume;
    }
}
