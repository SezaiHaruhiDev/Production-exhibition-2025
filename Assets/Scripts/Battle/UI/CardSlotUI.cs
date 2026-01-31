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
    [SerializeField] private GameObject _visualRoot; // カード表示部分のルート

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
            // すでにカードがある場合は入れ替える（古い方を戻す）
            if (CurrentCard != null)
            {
                OnCardRemoved?.Invoke(CurrentCard);
            }

            SetCard(cardUI.Data);
            
            // ドロップ元のカードUIに対して「セット成功」を伝える
            // （呼び出し元でDestroyなどが呼ばれる想定）
            cardUI.OnConsumedBySlot();

            OnCardSet?.Invoke(CurrentCard);
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
        if (_visualRoot != null) _visualRoot.SetActive(false);
        if (_iconImage != null) _iconImage.sprite = null;
    }
}
