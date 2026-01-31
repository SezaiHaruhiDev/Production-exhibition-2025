using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// パーティ共有MPのゲージ表示を管理するUIクラス
/// </summary>
public class SharedMPBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private float animationDuration = 0.3f;
    private bool _isFirstUpdate = true;
    private float _fullWidth;

    private void Awake()
    {
        if (fillImage != null)
        {
            // 幅を取得しておく
            _fullWidth = fillImage.rectTransform.rect.width;
            // はみ出た部分を隠すために、親にMaskがついていることを前提とするスライド式
        }
    }

    /// <summary>
    /// MPの表示を更新する
    /// </summary>
    public void UpdateView(int current, int max)
    {
        if (max <= 0 || fillImage == null) return;

        float ratio = (float)current / max;
        // 左に隠れた状態 (-width) から、ピッタリ重なる状態 (0) までスライド
        float targetX = -_fullWidth * (1f - ratio);

        if (_isFirstUpdate)
        {
            fillImage.rectTransform.anchoredPosition = new Vector2(targetX, 0);
            _isFirstUpdate = false;
        }
        else
        {
            fillImage.rectTransform.DOKill();
            fillImage.rectTransform.DOAnchorPosX(targetX, animationDuration).SetEase(Ease.OutQuad);
        }

        // テキストの更新
        if (mpText != null)
        {
            mpText.text = $"{current} / {max}";
        }
    }
}
