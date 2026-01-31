using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ドラッグ可能な感情カードUI
/// </summary>
public class BattleEmotionCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI emotionNameText;
    [SerializeField] private Image cardIcon;
    [SerializeField] private Image frameImage;
    [SerializeField] private CanvasGroup canvasGroup;

    public EmotionCardData Data { get; private set; }
    private Transform _originalParent;
    private Vector3 _startPosition;

    public void Setup(EmotionCardData data)
    {
        Data = data;
        if (emotionNameText != null)
        {
            emotionNameText.text = data.emotionName;
        }

        if (cardIcon != null && data.cardSprite != null)
        {
            cardIcon.sprite = data.cardSprite;
        }

        if (frameImage != null)
        {
            frameImage.color = data.cardThemeColor;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        _startPosition = transform.position;

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    private bool _isConsumed = false;

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!_isConsumed)
        {
            transform.position = _startPosition;
        }
    }

    /// <summary>
    /// スロットにドロップ成功した時に呼ばれる
    /// </summary>
    public void OnConsumedBySlot()
    {
        _isConsumed = true;
        Destroy(gameObject); // 手札から消滅
    }
}
