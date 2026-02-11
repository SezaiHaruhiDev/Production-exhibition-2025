using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 感情カードをセットするスロット（使用待機エリア）
/// カードを受け入れ、クリックで解除する機能を持つ
/// </summary>
public class CardSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private CanvasGroup _canvasGroup;
    [Header("Empty State")]
    [SerializeField] private GameObject _visualRoot; // カード表示部分のルート
    [SerializeField] private Sprite _emptySlotSprite; // カードがない時に表示する画像
    [SerializeField] private Color _emptyColor = new Color(1, 1, 1, 0.5f); // 空の時の色（少し薄くするなど）

    public EmotionCardData CurrentCard { get; private set; }
    public System.Action<EmotionCardData> OnCardSet;     // カードがセットされた時
    public System.Action<EmotionCardData> OnCardRemoved; // カードが外された（手札に戻る）時

    private void Awake()
    {
        Clear();
    }

    /// <summary>
    /// 手札からドラッグされたカードがドロップされた時の処理
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var cardUI = eventData.pointerDrag?.GetComponent<BattleEmotionCard>();
        if (cardUI != null)
        {
            // 重要：先にカードを「消費済み」としてマークし、UIオブジェクトを非アクティブにする。
            var cardData = cardUI.Data;
            cardUI.OnConsumedBySlot();

            // すでにカードがある場合は入れ替える（古い方を戻す）
            if (CurrentCard != null)
            {
                var oldCard = CurrentCard;
                Clear(); // 確実にスロットを空にする
                OnCardRemoved?.Invoke(oldCard);
            }

            SetCard(cardData);
            OnCardSet?.Invoke(cardData);
        }
    }

    /// <summary>
    /// クリックされたらカードを手札に戻す
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentCard != null)
        {
            var returningCard = CurrentCard;
            Clear();
            OnCardRemoved?.Invoke(returningCard);
        }
    }

    public void SetCard(EmotionCardData card)
    {
        CurrentCard = card;
        if (_visualRoot != null) _visualRoot.SetActive(true);
        
        if (_iconImage != null && card != null)
        {
            _iconImage.sprite = card.cardSprite;
            _iconImage.color = Color.white;
        }
    }

    public void Clear()
    {
        CurrentCard = null;
        
        if (_emptySlotSprite != null)
        {
            // 空スロット用の画像がある場合は、ルートを表示したまま画像だけ差し替える
            if (_visualRoot != null) _visualRoot.SetActive(true);
            if (_iconImage != null)
            {
                _iconImage.sprite = _emptySlotSprite;
                _iconImage.color = _emptyColor;
            }
        }
        else
        {
            // 空スロット用の画像がない場合は、従来通り非表示にする
            if (_visualRoot != null) _visualRoot.SetActive(false);
            if (_iconImage != null) _iconImage.sprite = null;
        }
    }
}
