using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム実行時のキャラクターデータ（マスターデータ + 成長データの合成結果）
/// </summary>
[System.Serializable]
public class RuntimeCharacter
{
    public int id;
    public string name;
    public int level;
    public int maxHp;
    public int currentHp;
    public int maxMp;
    public int currentMp;
    public int atk;
    public int def;
    public int exp;
    public int speed;
    public List<int> skillId;
    public int ultimateSkillId;

    /// <summary>
    /// シリアライゼーション用デフォルトコンストラクタ
    /// </summary>
    public RuntimeCharacter()
    {
        skillId = new List<int>();
    }

    /// <summary>
    /// マスターデータと成長データからランタイムキャラクターを生成する
    /// </summary>
    public RuntimeCharacter(CharacterMasterSO master, CharacterData data)
    {
        id = master.id;
        name = master.characterName;
        // マスターデータの基本値（不変）に、セーブデータ側の成長補正（変動値）を足し合わせる
        level = master.level + data.level;
        maxHp = master.hp + data.hp;
        currentHp = maxHp;
        maxMp = master.mp + data.mp;
        currentMp = maxMp;
        atk = master.atk + data.atk;
        def = master.def + data.def;
        exp = data.exp;
        speed = master.speed + data.speed;

        skillId = new List<int>();
        if (master.skillId != null) skillId.AddRange(master.skillId);
        if (data.skillId != null) skillId.AddRange(data.skillId);

        ultimateSkillId = master.ultimateSkillId;
    }

    /// <summary>
    /// レンタルデータからランタイムキャラクターを生成する（マスターデータをベースに指定値で上書き）
    /// </summary>
    public RuntimeCharacter(RentalCharacter rental)
    {
        if (rental.baseAlly == null) return;

        id = rental.baseAlly.id;
        name = string.IsNullOrEmpty(rental.name) ? rental.baseAlly.characterName : rental.name;
        level = rental.level;
        
        // ステータスが0でない場合はレンタル側の値を、0の場合はマスター側の値を使用する
        maxHp = rental.maxHp > 0 ? rental.maxHp : rental.baseAlly.hp;
        maxMp = rental.maxMp > 0 ? rental.maxMp : rental.baseAlly.mp;

        // 指定された割合(0.0〜1.0)に基づいて現在値を計算
        currentHp = Mathf.RoundToInt(maxHp * rental.hpRatio);
        currentMp = Mathf.RoundToInt(maxMp * rental.mpRatio);
        
        atk = rental.atk > 0 ? rental.atk : rental.baseAlly.atk;
        def = rental.def > 0 ? rental.def : rental.baseAlly.def;
        speed = rental.speed > 0 ? rental.speed : rental.baseAlly.speed;
        exp = rental.exp;

        skillId = new List<int>();
        if (rental.skillId != null && rental.skillId.Count > 0)
        {
            skillId.AddRange(rental.skillId);
        }
        else if (rental.baseAlly.skillId != null)
        {
            skillId.AddRange(rental.baseAlly.skillId);
        }

        ultimateSkillId = rental.baseAlly.ultimateSkillId;
    }
}
