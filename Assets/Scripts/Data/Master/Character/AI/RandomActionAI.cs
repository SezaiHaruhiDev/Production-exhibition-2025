using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ランダムに行動を決定するシンプルなAI
/// </summary>
[CreateAssetMenu(menuName = "Master/EnemyAI/RandomAction")]
public class RandomActionAI : EnemyLogicSO
{
    public override IEnumerator ExecuteTurn(BattleUnit actor, TurnManager turnManager)
    {
        // 少し考える時間を演出
        yield return new WaitForSeconds(0.5f);

        // スキル選択
        // 現状は所持スキルから等確率でランダム
        List<int> skillIds = actor.Data.skillIds;
        if (skillIds == null || skillIds.Count == 0)
        {
            Debug.LogWarning($"Enemy {actor.Data.name} has no skills!");
            yield break;
        }

        int selectedSkillId = skillIds[Random.Range(0, skillIds.Count)];

        // TurnManagerからSkillDataを取得
        SkillData skill = turnManager.GetSkillData(selectedSkillId);
        if (skill == null)
        {
            Debug.LogWarning($"Enemy {actor.Data.name} tried to use invalid skill ID {selectedSkillId}");
            yield break;
        }

        // 対象選択
        List<BattleUnit> candidates = new List<BattleUnit>();
        SkillTargetType targetType = skill.GetEffectiveTargetType(null); // 敵は感情カードを使わない想定（null）

        switch (targetType)
        {
            case SkillTargetType.Self:
                candidates.Add(actor);
                break;
            case SkillTargetType.SingleEnemy:
            case SkillTargetType.AllEnemies:
                // 敵にとっての敵＝プレイヤー（Ally）
                candidates.AddRange(turnManager.UnitManager.AllUnits.Where(u => u.Data.isAlly && u.Data.currentHp > 0));
                break;
            case SkillTargetType.SingleAlly:
            case SkillTargetType.AllAllies:
                // 敵にとっての味方＝敵（Not Ally）
                candidates.AddRange(turnManager.UnitManager.AllUnits.Where(u => !u.Data.isAlly && u.Data.currentHp > 0));
                break;
        }

        List<BattleUnit> targets = new List<BattleUnit>();

        if (candidates.Count > 0)
        {
            if (targetType == SkillTargetType.AllEnemies || targetType == SkillTargetType.AllAllies)
            {
                targets.AddRange(candidates);
            }
            else
            {
                // 単体対象ならランダムに1体
                targets.Add(candidates[Random.Range(0, candidates.Count)]);
            }
        }

        // 実行
        Debug.Log($"Enemy Action: {skill.displayName} on {string.Join(",", targets.Select(t => t.Data.name))}");

        // 非同期（演出込み）で実行
        yield return turnManager.StartCoroutine(SkillExecutor.ExecuteAsync(actor, targets, skill, null, turnManager.PresentationManager));
    }
}
