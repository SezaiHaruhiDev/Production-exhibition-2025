using UnityEngine;
using System.Linq;

/// <summary>
/// 敵全滅勝利条件
/// </summary>
[CreateAssetMenu(menuName = "Battle/VictoryCondition/Annihilation")]
public class AnnihilationVictorySO : VictoryConditionSO
{
    /// <summary>
    /// 現在の戦況から勝利・敗北を判定する
    /// </summary>
    public override TurnManager.BattleState CheckVictory(TurnManager manager)
    {
        bool anyAllyAlive = manager.UnitManager.AllUnits
            .Any(u => u.Data.isAlly && u.Data.currentHp > 0);

        if (!anyAllyAlive)
        {
            return TurnManager.BattleState.Lost;
        }

        bool anyEnemyAlive = manager.UnitManager.AllUnits
            .Any(u => !u.Data.isAlly && u.Data.currentHp > 0);

        if (!anyEnemyAlive)
        {
            return TurnManager.BattleState.Won;
        }

        return TurnManager.BattleState.Start;
    }
}
