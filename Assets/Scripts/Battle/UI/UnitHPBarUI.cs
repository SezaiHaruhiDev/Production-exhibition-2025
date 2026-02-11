using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 各ユニットの足元に表示されるHPバーを管理するクラス
/// </summary>
public class UnitHPBarUI : MonoBehaviour
{
    [System.Serializable]
    public struct HPBarSprites
    {
        public Sprite background;
        public Sprite mask;
        public Sprite fill;
    }

    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image maskImage;
    [SerializeField] private Image fillImage;
    [Header("Positioning")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Side Config")]
    [SerializeField] private HPBarSprites allySprites;
    [SerializeField] private HPBarSprites enemySprites;

    private float _fullWidth;
    private bool _isInitialized = false;

    private void Start()
    {
        if (!_isInitialized) Initialize();
    }

    private void Initialize()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }

        if (fillImage != null)
        {
            _fullWidth = fillImage.rectTransform.rect.width;
        }

        // 足元に強制配置（Scaleが1以上だと巨大化するので0.01に調整）
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localPosition = positionOffset; // 設定されたオフセットを使用
            rt.localScale = Vector3.one * 0.01f;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 味方か敵かに応じた見た目に切り替える
    /// </summary>
    public void SetSide(bool isAlly)
    {
        if (!_isInitialized) Initialize();

        HPBarSprites sprites = isAlly ? allySprites : enemySprites;

        if (backgroundImage != null && sprites.background != null) backgroundImage.sprite = sprites.background;
        if (maskImage != null && sprites.mask != null) maskImage.sprite = sprites.mask;
        if (fillImage != null && sprites.fill != null) fillImage.sprite = sprites.fill;
    }

    private void LateUpdate()
    {
        // 常にカメラの方を向かせる（ビルボード）
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    /// <summary>
    /// HPの表示を更新する
    /// </summary>
    public void UpdateHP(int current, int max)
    {
        if (!_isInitialized) Initialize();
        if (max <= 0 || fillImage == null) return;

        float ratio = Mathf.Clamp01((float)current / max);
        float targetX = -_fullWidth * (1f - ratio);

        fillImage.rectTransform.DOKill();
        fillImage.rectTransform.DOAnchorPosX(targetX, animationDuration).SetEase(Ease.OutQuad);
        
        // 残りHPに応じて色を変える演出などもここに追加可能
        // if (ratio < 0.2f) fillImage.color = Color.red; 
    }
}
