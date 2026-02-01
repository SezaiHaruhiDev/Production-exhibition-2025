using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// スキルの計算と実行を担当するクラス
/// </summary>
public static class SkillExecutor
{
    /// <summary>
    /// スキルを実行し、対象に効果を適用する
    /// </summary>
    public static void Execute(BattleUnit actor, List<BattleUnit> targets, SkillData skill, EmotionCardData emotion)
    {
        if (skill == null) return;
        
        Debug.Log($"[SkillExecutor] Executing {skill.displayName} (Card: {(emotion != null ? emotion.emotionName : "None")}) on {targets.Count} targets.");

        List<EffectData> effects = skill.GetEffectiveEffects(emotion);

        if (effects != null && effects.Count > 0)
        {
            foreach (var effect in effects)
            {
                ProcessEffect(actor, targets, effect);
            }
        }
        else
        {
            ProcessBaseSkill(actor, targets, skill);
        }
    }

    private static void ProcessBaseSkill(BattleUnit actor, List<BattleUnit> targets, SkillData skill)
    {
        if (skill.category == SkillCategory.Attack)
        {
            ApplyDamage(actor, targets, skill.basePower);
        }
        else if (skill.category == SkillCategory.Heal)
        {
            ApplyHeal(actor, targets, skill.basePower);
        }
        else if (skill.category == SkillCategory.Revive)
        {
            ApplyRevive(actor, targets, skill.basePower);
        }
        else
        {
             ApplyDamage(actor, targets, skill.basePower);
        }
    }

    private static void ProcessEffect(BattleUnit actor, List<BattleUnit> targets, EffectData effect)
    {
        switch (effect.skillCategory)
        {
            case SkillCategory.Attack:
                ApplyDamage(actor, targets, effect.power);
                break;
            case SkillCategory.Heal:
                ApplyHeal(actor, targets, effect.power);
                break;
            case SkillCategory.Revive:
                ApplyRevive(actor, targets, effect.power);
                break;
            default:
                Debug.LogWarning($"SkillExecutor: Unknown Category {effect.skillCategory}");
                break;
        }
    }

    private static void ApplyDamage(BattleUnit actor, List<BattleUnit> targets, int power)
    {
        foreach (var target in targets)
        {
            if (target == null || target.Data.currentHp <= 0) continue;

            // ダメージ計算: (攻撃者のATK + スキルの威力) - 対象のDEF
            // 最低ダメージは1とする
            int damage = Mathf.Max(1, (actor.Data.atk + power) - target.Data.def);
            
            target.Data.currentHp = Mathf.Max(0, target.Data.currentHp - damage); 
            target.RefreshHPBar(); 
            target.ShowDamage(damage); 


        }
    }

    private static void ApplyHeal(BattleUnit actor, List<BattleUnit> targets, int power)
    {
        foreach (var target in targets)
        {
            if (target == null || target.Data.currentHp <= 0) continue; 

            // 回復計算: 攻撃者のATK + スキルの威力
            int heal = actor.Data.atk + power;
            target.Data.currentHp = Mathf.Min(target.Data.maxHp, target.Data.currentHp + heal);
            target.RefreshHPBar();
            target.ShowHeal(heal);
            

        }
    }

    private static void ApplyRevive(BattleUnit actor, List<BattleUnit> targets, int power)
    {
        foreach (var target in targets)
        {
            if (target == null || target.Data.currentHp > 0) continue;

            int heal = power;
            target.Data.currentHp = Mathf.Min(target.Data.maxHp, heal);
            
            target.SetDown(false);
            target.RefreshHPBar();
            target.ShowHeal(heal);
            

        }
    }
}
