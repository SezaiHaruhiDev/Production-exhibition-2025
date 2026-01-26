using UnityEngine;
using System.Collections;

/// <summary>
/// サウンドエフェクト（SE）の再生を管理
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
                GameObject go = new GameObject("SoundManager");
                _instance = go.AddComponent<SoundManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private AudioSource _seSource;
    private AudioSource _bgmSource;

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

        _seSource = gameObject.AddComponent<AudioSource>();
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
    }

    /// <summary>
    /// 初期化処理（BootManagerから呼び出す・コルーチン対応）
    /// </summary>
    public IEnumerator InitializeCoroutine()
    {
        // 明示的にインスタンス化・初期設定を行いたい場合に呼び出す
        // 現状はAwakeで処理が完結しているため重い処理はないが、
        // 将来的に設定ロード（非同期）などが入っても良いようにコルーチン化しておく
        if (_seSource == null || _bgmSource == null)
        {
            _seSource = gameObject.AddComponent<AudioSource>();
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }
        yield break;
    }

    /// <summary>
    /// Resources/SE フォルダ内のファイル名を指定してSEを再生
    /// </summary>
    public void PlaySE(string seName, float volume = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>("se/" + seName);
        if (clip != null)
        {
            _seSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"SE '{seName}' not found in Resources/SE/");
        }
    }

    /// <summary>
    /// AudioClipを直接指定してSEを再生
    /// </summary>
    public void PlaySE(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _seSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// BGMを再生（Resources/bgm/ 以下）
    /// transitionTime > 0 の場合はフェードイン/クロスフェードを行う
    /// </summary>
    public void PlayBGM(string bgmName, float volume = 1f, float transitionTime = 0.5f)
    {
        AudioClip clip = Resources.Load<AudioClip>("bgm/" + bgmName);
        if (clip == null)
        {
            Debug.LogError($"BGM '{bgmName}' not found in Resources/bgm/");
            return;
        }

        // すでに同じ曲が流れている場合は音量だけ調整（必要なら）して終了
        if (_bgmSource.clip == clip && _bgmSource.isPlaying)
        {
            StartCoroutine(FadeVolume(volume, transitionTime));
            return;
        }

        StartCoroutine(PlayBGMCoroutine(clip, volume, transitionTime));
    }

    /// <summary>
    /// BGMを停止（フェードアウト）
    /// </summary>
    public void StopBGM(float fadeTime = 0.5f)
    {
        StartCoroutine(StopBGMCoroutine(fadeTime));
    }

    /// <summary>
    /// 停止中のBGMをフェードインして再開（最初から再生）
    /// </summary>
    public void ResumeBGM(float fadeTime = 0.5f)
    {
        if (_bgmSource.clip != null && !_bgmSource.isPlaying)
        {
            _bgmSource.volume = 0f;
            _bgmSource.Play();
            StartCoroutine(FadeVolume(1f, fadeTime));
        }
    }

    /// <summary>
    /// BGMの音量を変更（フェード）
    /// </summary>
    public void SetBGMVolume(float targetVolume, float fadeTime = 0.5f)
    {
        StartCoroutine(FadeVolume(targetVolume, fadeTime));
    }

    /// <summary>
    /// BGMを一時停止する（位置を保持）
    /// </summary>
    public void PauseBGM(float fadeTime = 0.5f)
    {
        StartCoroutine(PauseBGMCoroutine(fadeTime));
    }

    /// <summary>
    /// 一時停止中のBGMを再開する（現在の位置から）
    /// </summary>
    public void UnpauseBGM(float fadeTime = 0.5f)
    {
        StartCoroutine(UnpauseBGMCoroutine(fadeTime));
    }

    private IEnumerator PlayBGMCoroutine(AudioClip newClip, float targetVolume, float fadeTime)
    {
        // 何か流れていたらフェードアウト
        if (_bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(0f, fadeTime));
            _bgmSource.Stop();
        }

        _bgmSource.clip = newClip;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        // フェードイン
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
