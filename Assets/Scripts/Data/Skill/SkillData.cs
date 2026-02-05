using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// スキルのマスターデータ
/// </summary>
[CreateAssetMenu(menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public int skillId;
    public string displayName;
    public Sprite skillIcon;
    [TextArea] public string description;

    public SkillCategory category;
    public SkillType skilltype;

    [Header("Cost")]
    public int imaginationCost;
    public bool canChargeUltimate;
    public int ultimateChargeValue;

    [Header("BaseEffect")]
    public int basePower;
    public SkillTargetType targetType;

    [Header("EmotionEffect")]
    public List<EmotionEffectSet> emotionEffectSet;

    [Header("Performance")]
    public SkillPerformanceSO performanceData;

    /// <summary>
    /// 感情カードを考慮した最終的なターゲットタイプを取得する
    /// </summary>
    public SkillTargetType GetEffectiveTargetType(EmotionCardData card)
    {
        if (card == null) return targetType;

        // 感情とレベルが一致するセットを探す
        var set = emotionEffectSet.Find(s => s.emotion == card.emotion && s.level == card.level);
        if (set != null && set.effectDataList != null && set.effectDataList.Count > 0)
        {
            // 最初のエフェクトデータのターゲットタイプを優先する
            return set.effectDataList[0].skillTargetType;
        }

        return targetType;
    }
    /// <summary>
    /// 感情カードを考慮した効果リストを取得する
    /// </summary>
    public List<EffectData> GetEffectiveEffects(EmotionCardData card)
    {
        if (card != null)
        {
            var set = emotionEffectSet.Find(s => s.emotion == card.emotion && s.level == card.level);
            if (set != null && set.effectDataList != null && set.effectDataList.Count > 0)
            {
                return set.effectDataList;
            }
        }
        return new List<EffectData>();
    }

    /// <summary>
    /// 感情カードを考慮した演出データを取得する
    /// </summary>
    public SkillPerformanceSO GetEffectivePerformance(EmotionCardData card)
    {
        if (card != null)
        {
            var set = emotionEffectSet.Find(s => s.emotion == card.emotion && s.level == card.level);
            if (set != null && set.performanceOverride != null)
            {
                return set.performanceOverride;
            }
        }
        return performanceData;
    }

    /// <summary>
    /// 感情カードを考慮して、このスキルが「蘇生」を含むかどうかを判定する
    /// </summary>
    public bool IsReviveSkill(EmotionCardData card)
    {
        if (card != null)
        {
            var set = emotionEffectSet.Find(s => s.emotion == card.emotion && s.level == card.level);
            if (set != null && set.effectDataList != null)
            {
                return set.effectDataList.Exists(e => e.skillCategory == SkillCategory.Revive);
            }
        }
        return category == SkillCategory.Revive;
    }
}
