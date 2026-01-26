using System.Collections;
using UnityEngine;
using Common;
using Novel.Data;

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
    private float focusYOffset = 500f;

    [Header("パララックス倍率")]
    public float parallax = 0.3f;
    private Vector2 baseCharPos;
    private Vector2 baseBgPos;
    private Vector2 currentOffset;

    private Vector3 baseZoomScale;
    private Coroutine zoomRoutine;
    void Awake()
    {
        if (characterManager == null)
            characterManager = FindFirstObjectByType<CharacterManager>();
    }

    void Start()
    {
        baseCharPos = characterParent.anchoredPosition;
        baseBgPos = backgroundImage.anchoredPosition;
        currentOffset = Vector2.zero;

        baseZoomScale = zoomRoot.localScale;
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
        Vector2 startOffset = currentOffset;
        Vector2 targetOffset = -targetPos; // キャラ位置に画面を寄せるため逆向きにズラす（カメラ自体が動くのではなく背景が動くため）

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = 1 - Mathf.Pow(1 - t, 3);

            currentOffset = Vector2.Lerp(startOffset, targetOffset, t);

            ApplyOffset(currentOffset);
            yield return null;
        }

        currentOffset = targetOffset;
        ApplyOffset(currentOffset);
    }
    private void ApplyOffset(Vector2 offset)
    {
        characterParent.anchoredPosition = baseCharPos + offset;
        backgroundImage.anchoredPosition = baseBgPos + offset * parallax;
    }
    /// <summary>
    /// 画面のズーム倍率を変更する
    /// </summary>
    public void SetZoom(float scale, float duration = 0.3f)
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomCoroutine(scale, duration));
    }

    private IEnumerator ZoomCoroutine(float scale, float duration)
    {
        Vector3 start = zoomRoot.localScale;
        Vector3 target = baseZoomScale * scale; // 初期スケール × 倍率

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = 1 - Mathf.Pow(1 - t, 3);

            zoomRoot.localScale = Vector3.Lerp(start, target, t);

            yield return null;
        }

        zoomRoot.localScale = target;
    }

    /// <summary>
    /// 画面を揺らす（シェイク効果）
    /// </summary>
    /// <param name="shakeCount">揺れる回数</param>
    public void Shake(int shakeCount = 3)
    {
        StartCoroutine(ShakeCoroutine(shakeCount));
    }

    private IEnumerator ShakeCoroutine(int count)
    {
        float shakeAmount = 20f;  // 揺れ幅（ピクセル）
        float singleDuration = 0.05f;  // 1回揺れる時間
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
            if (float.TryParse(offStr, out float off))
                focusYOffset = off;

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
            ResetFocus(0.5f);
            return;
        }


        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Value, out string shake)&& int.TryParse (shake, out int shakeint))
        {
            Shake(shakeint);
            return;
        }

        if (!cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string name))
            return;

        if (characterManager == null)
        {
            Debug.LogError("CharacterManager is not assigned to CameraFocusManager!");
            return;
        }
        RectTransform characterImage = characterManager.GetCharacterRect(name);
        if (characterImage == null) return;

        // キャラ画像のローカル位置
        Vector2 localPos = characterImage.anchoredPosition;

        // 少し上を中心にフォーカスさせる場合は +500 など調整
        Vector2 targetPos = new Vector2(localPos.x, localPos.y + focusYOffset);

        FocusAt(targetPos, 0.5f);
    }
}
