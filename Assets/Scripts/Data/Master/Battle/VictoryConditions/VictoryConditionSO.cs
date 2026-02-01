using UnityEngine;

/// <summary>
/// 勝利条件を定義する基底クラス (Strategy Pattern)
/// </summary>
public abstract class VictoryConditionSO : ScriptableObject
{
    [TextArea]
    public string description;

    /// <summary>
    /// 現在の戦況から勝利・敗北を判定する
    /// </summary>
    public abstract TurnManager.BattleState CheckVictory(TurnManager manager);
}
