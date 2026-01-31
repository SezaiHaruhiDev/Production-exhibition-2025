using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 感情カードのデッキ構成を定義するマスターデータ
/// </summary>
[CreateAssetMenu(menuName = "Battle/EmotionDeckSO")]
public class EmotionDeckSO : ScriptableObject
{
    public List<EmotionCardData> deckCards = new List<EmotionCardData>();
}
