using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// スプライト連番アニメーションを再生するクラス (UI用)
/// Animatorを使わずに軽量にパラパラ漫画を行う
/// </summary>
[RequireComponent(typeof(Image))]
public class UIFlipbook : MonoBehaviour
{
    [Tooltip("再生するスプライト連番")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("1秒あたりのフレーム数 (FPS)")]
    [SerializeField] private float fps = 30f;

    [Tooltip("ループするかどうか")]
    [SerializeField] private bool loop = false;

    [Tooltip("再生終了時に非表示にするか")]
    [SerializeField] private bool disableOnComplete = true;

    [Tooltip("再生終了時にGameObjectを破棄するか")]
    [SerializeField] private bool destroyOnComplete = false;

    [Tooltip("起動時に自動再生するか")]
    [SerializeField] private bool playOnAwake = true;

    private Image _targetImage;
    private Coroutine _currentCoroutine;

    private void Awake()
    {
        _targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    /// <summary>
    /// アニメーション再生開始
    /// </summary>
    public void Play()
    {
        if (frames == null || frames.Length == 0) return;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);

        gameObject.SetActive(true);
        _currentCoroutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        float waitTime = 1f / fps;

        while (true)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                _targetImage.sprite = frames[i];
                // NativeSizeに合わせたい場合はコメントアウト解除
                // _targetImage.SetNativeSize();
                yield return new WaitForSeconds(waitTime);
            }

            if (!loop)
            {
                break;
            }
        }

        if (disableOnComplete) gameObject.SetActive(false);
        if (destroyOnComplete) Destroy(gameObject);

        _currentCoroutine = null;
    }
}
