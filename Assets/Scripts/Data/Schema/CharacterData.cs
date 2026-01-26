using System;
using System.Collections.Generic;

/// <summary>
/// キャラクターの成長データ（セーブデータに保存される変動値）
/// </summary>
[Serializable]
public class CharacterData
{
    // 各ステータスは、マスターデータの初期値からの「増加量（変動値）」のみを保存する
    public int level;
    public bool IsOwned;
    public int hp;
    public int mp;
    public int exp;
    public int atk;
    public int def;
    public int speed;
    public List<int> skillId = new List<int>();
}
