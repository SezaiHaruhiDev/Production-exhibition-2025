using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 感情カードのデッキ・手札・捨て札を管理するマネージャー
/// </summary>
public class EmotionDeckManager : MonoBehaviour
{
    [Header("Deck Config")]
    [SerializeField] private List<EmotionCardData> debugDeckSource;
    [SerializeField] private int initialDrawCount = 5;

    private Queue<EmotionCardData> _deck = new Queue<EmotionCardData>();
    private List<EmotionCardData> _hand = new List<EmotionCardData>();
    private List<EmotionCardData> _discard = new List<EmotionCardData>();

    public IReadOnlyList<EmotionCardData> Hand => _hand;

    public event System.Action OnHandChanged;

    /// <summary>
    /// デッキを構築してシャッフルする
    /// </summary>
    /// <param name="source">デッキの元となるカードリスト</param>
    public void InitializeDeck(List<EmotionCardData> source)
    {
        _deck.Clear();
        _hand.Clear();
        _discard.Clear();

        if (source == null || source.Count == 0)
        {
            Debug.LogWarning("EmotionDeckManager: Source deck is empty. Using debug source.");
            source = debugDeckSource;
            if (source == null || source.Count == 0) return;
        }

        var tempDeck = new List<EmotionCardData>(source);
        ShuffleList(tempDeck);

        foreach (var card in tempDeck)
        {
            _deck.Enqueue(card);
        }

        Debug.Log($"Deck Initialized. Count: {_deck.Count}");

        Draw(initialDrawCount);
    }

    /// <summary>
    /// 山札からカードを引く
    /// </summary>
    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_deck.Count == 0)
            {
                ReshuffleDiscardToDeck();
                if (_deck.Count == 0) break;
            }

            var card = _deck.Dequeue();
            _hand.Add(card);
        }

        NotifyHandChanged();
    }

    /// <summary>
    /// 手札から特定のカードを除外する（捨て札には送らない。スロット移動用）
    /// </summary>
    public void RemoveCardFromHand(EmotionCardData card)
    {
        if (_hand.Contains(card))
        {
            _hand.Remove(card);
            NotifyHandChanged();
        }
    }

    /// <summary>
    /// 指定されたカードを手札に加える（スカスカからの返却など）
    /// </summary>
    public void AddCard(EmotionCardData card)
    {
        if (card != null)
        {
            _hand.Add(card);
            NotifyHandChanged();
        }
    }

    /// <summary>
    /// カードを使用する（手札から削除して捨て札へ）
    /// </summary>
    public void UseCard(EmotionCardData card)
    {
        if (_hand.Contains(card))
        {
            _hand.Remove(card);
            _discard.Add(card);
            NotifyHandChanged();
        }
    }

    private void NotifyHandChanged()
    {
        OnHandChanged?.Invoke();
        Debug.Log($"Hand Updated. Count: {_hand.Count}");
    }

    private void ReshuffleDiscardToDeck()
    {
        if (_discard.Count == 0) return;

        Debug.Log("Reshuffling Discard pile to Deck...");
        ShuffleList(_discard);
        foreach (var card in _discard)
        {
            _deck.Enqueue(card);
        }
        _discard.Clear();
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
