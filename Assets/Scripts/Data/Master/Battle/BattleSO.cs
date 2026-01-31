using UnityEngine;

/// <summary>
/// 1つのバトルの設定データ（フェーズ構成、パーティタイプなど）
/// </summary>
[CreateAssetMenu(menuName = "Battle/BattleSO")]
public class BattleSO : ScriptableObject
{
    public int battleID;
    public PartySourceType partyType;
    public OneBattlePhase[] phases;
    public RentalPartySO rentalParty;
    public EmotionDeckSO deckConfig;
}
