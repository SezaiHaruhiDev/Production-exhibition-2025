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
}
