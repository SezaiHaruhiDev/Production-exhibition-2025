using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 行動順リストの個々のアイコン表示
/// </summary>
public class TurnOrderUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Color allyFrameColor = Color.blue;
    [SerializeField] private Color enemyFrameColor = Color.red;

    [Header("Scale Settings")]
    [SerializeField] private float normalScale = 0.8f;
    [SerializeField] private float nextScale = 1.1f;

    public void Setup(BattleUnit unit, Sprite sprite, bool isNext)
    {
        if (unit == null || unit.Data == null) return;
        
        if (sprite != null)
        {
            iconImage.sprite = sprite;
        }

        if (frameImage != null)
        {
            frameImage.color = unit.Data.isAlly ? allyFrameColor : enemyFrameColor;
        }

        // 初期スケール設定
        float targetScale = isNext ? nextScale : normalScale;
        transform.localScale = Vector3.one * targetScale;
    }

    /// <summary>
    /// 目標の座標とサイズへ滑らかに移動する
    /// </summary>
    public void AnimateTo(Vector3 localPos, bool isNext, float duration)
    {
        float targetScale = isNext ? nextScale : normalScale;
        
        transform.DOKill();
        transform.DOLocalMove(localPos, duration).SetEase(Ease.OutQuart);
        transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 行動終了時に画面外（左側など）へ消えていく演出
    /// </summary>
    public void PlayExitAnimation(float duration)
    {
        transform.DOKill();
        // 左に画面外へ、かつフェードアウト（CanvasGroupがあれば）
        transform.DOLocalMoveX(transform.localPosition.x - 200, duration).SetEase(Ease.InBack);
        
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.DOFade(0, duration).OnComplete(() => Destroy(gameObject));
    }
}
