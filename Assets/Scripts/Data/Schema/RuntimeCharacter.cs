using System.Collections.Generic;

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
    }
}
