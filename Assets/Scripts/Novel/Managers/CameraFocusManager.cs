using System.Collections;
using UnityEngine;
using Common;
using Novel.Data;
using UnityEngine.Assertions;

/// <summary>
/// カメラフォーカス、ズーム、シェイク効果を管理
/// </summary>
public class CameraFocusManager : MonoBehaviour
{
    [Header("対象レイヤー")]
    public RectTransform zoomRoot;
    public RectTransform characterParent;
    public RectTransform backgroundImage;
    [SerializeField] private CharacterManager characterManager;

    private float _focusYOffset = 500f;
    private const float Parallax = 0.3f;

    private Vector2 _baseCharPos;
    private Vector2 _baseBgPos;
    private Vector2 _currentOffset;
    private Vector3 _baseZoomScale;
    private Coroutine _zoomRoutine;

    private void Awake()
    {
        if (characterManager == null)
            characterManager = FindFirstObjectByType<CharacterManager>();

        Assert.IsNotNull(zoomRoot, "CameraFocusManager: ZoomRoot is not assigned.");
        Assert.IsNotNull(characterParent, "CameraFocusManager: CharacterParent is not assigned.");
        Assert.IsNotNull(backgroundImage, "CameraFocusManager: BackgroundImage is not assigned.");
        Assert.IsNotNull(characterManager, "CameraFocusManager: CharacterManager is not assigned.");
    }

    private void Start()
    {
        _baseCharPos = characterParent.anchoredPosition;
        _baseBgPos = backgroundImage.anchoredPosition;
        _currentOffset = Vector2.zero;
        _baseZoomScale = zoomRoot.localScale;
    }

    /// <summary>
    /// 指定した座標にカメラ（背景・キャラクター）をフォーカスさせる
    /// </summary>
    public void FocusAt(Vector2 targetPosition, float duration = 0.5f)
    {
        StartCoroutine(FocusCoroutine(targetPosition, duration));
    }

    /// <summary>
    /// カメラフォーカスを初期位置（中心）に戻す
    /// </summary>
    public void ResetFocus(float duration = 0.5f)
    {
        StartCoroutine(FocusCoroutine(Vector2.zero, duration));
    }

    private IEnumerator FocusCoroutine(Vector2 targetPos, float duration)
    {
        Vector2 startOffset = _currentOffset;
        Vector2 targetOffset = -targetPos; // 逆方向にずらしてフォーカスを表現

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            _currentOffset = Vector2.Lerp(startOffset, targetOffset, t);
            ApplyOffset(_currentOffset);
            yield return null;
        }

        _currentOffset = targetOffset;
        ApplyOffset(_currentOffset);
    }

    private void ApplyOffset(Vector2 offset)
    {
        characterParent.anchoredPosition = _baseCharPos + offset;
        backgroundImage.anchoredPosition = _baseBgPos + offset * Parallax;
    }

    /// <summary>
    /// 画面のズーム倍率を変更する
    /// </summary>
    public void SetZoom(float scale, float duration = 0.3f)
    {
        if (_zoomRoutine != null) StopCoroutine(_zoomRoutine);
        _zoomRoutine = StartCoroutine(ZoomCoroutine(scale, duration));
    }

    private IEnumerator ZoomCoroutine(float scale, float duration)
    {
        Vector3 start = zoomRoot.localScale;
        Vector3 target = _baseZoomScale * scale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            zoomRoot.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        zoomRoot.localScale = target;
    }

    /// <summary>
    /// 画面を揺らす（シェイク効果）
    /// </summary>
    public void Shake(int shakeCount = 3)
    {
        StartCoroutine(ShakeCoroutine(shakeCount));
    }

    private IEnumerator ShakeCoroutine(int count)
    {
        float shakeAmount = 20f;
        float singleDuration = 0.05f;
        Vector3 originalPos = zoomRoot.localPosition;

        for (int i = 0; i < count; i++)
        {
            zoomRoot.localPosition = originalPos + Vector3.right * shakeAmount;
            yield return new WaitForSeconds(singleDuration);
            zoomRoot.localPosition = originalPos + Vector3.left * shakeAmount;
            yield return new WaitForSeconds(singleDuration);
        }
        zoomRoot.localPosition = originalPos;
    }

    /// <summary>
    /// コマンドからカメラ操作（ズーム、オフセット、シェイク、フォーカス）を実行する
    /// </summary>
    public void Camera(Command cmd)
    {
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Offset, out string offStr))
        {
            if (float.TryParse(offStr, out float off)) _focusYOffset = off;
            return;
        }

        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Zoom, out string zoomStr))
        {
            if (zoomStr == GameConstants.NovelCommands.Reset)
                SetZoom(1f, 0.3f);
            else if (float.TryParse(zoomStr, out float sc))
                SetZoom(sc, 0.3f);
            return;
        }

        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Reset, out string _))
        {
            ResetFocus(0.3f);
            return;
        }

        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Value, out string shake) && int.TryParse(shake, out int shakeint))
        {
            Shake(shakeint);
            return;
        }

        if (!cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string name)) return;

        if (characterManager == null)
        {
            Debug.LogError("CameraFocusManager: CharacterManager is missing!");
            return;
        }

        RectTransform characterImage = characterManager.GetCharacterRect(name);
        if (characterImage == null) return;

        Vector2 localPos = characterImage.anchoredPosition;
        Vector2 targetPos = new Vector2(localPos.x, localPos.y + _focusYOffset);

        FocusAt(targetPos, 0.3f);
    }
}
