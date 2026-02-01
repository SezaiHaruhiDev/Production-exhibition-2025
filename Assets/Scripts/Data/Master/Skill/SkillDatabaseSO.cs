using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// スキルデータのデータベース
/// </summary>
[CreateAssetMenu(menuName = "Game/SkillDatabase")]
public class SkillDatabaseSO : ScriptableObject
{
    [SerializeField] private List<SkillData> skillList = new List<SkillData>();

    /// <summary>
    /// IDからスキルデータを検索する
    /// </summary>
    public SkillData GetById(int id)
    {
        if (skillList == null) return null;
        return skillList.FirstOrDefault(s => s.skillId == id);
    }
}
