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
    [SerializeField] private int maxHandSize = 10;
    public int MaxHandSize => maxHandSize;

    private Queue<EmotionCardData> _deck = new Queue<EmotionCardData>();
    private List<EmotionCardData> _hand = new List<EmotionCardData>();
    private List<EmotionCardData> _discard = new List<EmotionCardData>();

    /// <summary>
    /// 現在の手札
    /// </summary>
    public IReadOnlyList<EmotionCardData> Hand => _hand;

    /// <summary>
    /// 手札の内容が変更されたときに発行されるイベント
    /// </summary>
    public event System.Action OnHandChanged;

    /// <summary>
    /// デッキまたは捨て札にカードが残っているかどうか
    /// </summary>
    public bool CanDraw => _deck.Count > 0 || _discard.Count > 0;

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



        Draw(initialDrawCount);
    }

    /// <summary>
    /// 山札からカードを引く
    /// </summary>
    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_hand.Count >= maxHandSize) break;

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
    /// 2つの同じカードを合成してレベルアップさせる
    /// </summary>
    public bool Synthesize(EmotionCardData cardA, EmotionCardData cardB)
    {
        if (cardA == null || cardB == null) return false;

        // 同じ感情かつ同じレベル、かつ次のレベルのカードが存在する場合のみ合成可能
        if (cardA.emotion == cardB.emotion && cardA.level == cardB.level && cardA.nextLevelCard != null)
        {
            // 手札に両方が存在することを確認（同じインスタンスが複数ある場合も考慮）
            int count = _hand.Count(c => c == cardA);
            if (count < 2 && cardA == cardB)
            {
                return false;
            }

            _hand.Remove(cardA);
            _hand.Remove(cardB);
            _hand.Add(cardA.nextLevelCard);
            
            NotifyHandChanged();
            return true;
        }

        return false;
    }

    /// <summary>
    /// カードを使用する（手札から削除して捨て札へ）
    /// </summary>
    public void UseCard(EmotionCardData card)
    {
        if (card == null) return;

        if (_hand.Contains(card))
        {
            _hand.Remove(card);
        }
        
        DiscardCard(card, true);
    }

    /// <summary>
    /// カードを捨て札に送る（手札からの削除は行わない。スロット等ですでに手札から抜けている場合に用いる）
    /// 合成カード（Level > 1）の場合は分解して基礎カードとして捨て札へ送る
    /// </summary>
    public void DiscardCard(EmotionCardData card, bool notifyHand = true)
    {
        if (card == null) return;

        if (card.level > 1 && card.previousLevelCard != null)
        {
            // 分解：2枚の前のレベルのカードとして扱う
            DiscardCard(card.previousLevelCard, false);
            DiscardCard(card.previousLevelCard, false);
        }
        else
        {
            _discard.Add(card);
        }

        if (notifyHand) NotifyHandChanged();
    }

    private void NotifyHandChanged()
    {
        OnHandChanged?.Invoke();
    }

    private void ReshuffleDiscardToDeck()
    {
        if (_discard.Count == 0) return;


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
