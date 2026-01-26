using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// キャラクターマスターデータの基底クラス（味方・敵共通）
/// </summary>
public abstract class CharacterMasterSO : ScriptableObject
{
    public int id;
    public string characterName;

    [FormerlySerializedAs("CharacterMiniSprite")]
    public Sprite characterMiniSprite;

    [FormerlySerializedAs("CharacterBigSprite")]
    public Sprite characterBigSprite;

    public int hp;
    public int mp;
    public int atk;
    public int def;
    public int exp;
    public int level;
    public int speed;
    public List<int> skillId = new List<int>();
}
