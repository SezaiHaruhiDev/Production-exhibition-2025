using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レンタルキャラクターデータ（マスターデータに対する補正値を保持）
/// </summary>
[System.Serializable]
public class RentalCharacter
{
    public AllyMasterSO baseAlly;
    public string name;
    public int level;
    public int maxHp;
    public int maxMp;
    [Range(0f, 1f)] public float hpRatio = 1f;
    [Range(0f, 1f)] public float mpRatio = 1f;
    public int atk;
    public int def;
    public int exp;
    public int speed;
    public List<int> skillId;
}
