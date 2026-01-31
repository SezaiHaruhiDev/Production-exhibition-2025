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
    public string skillId;
    public string displayName;
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
}
