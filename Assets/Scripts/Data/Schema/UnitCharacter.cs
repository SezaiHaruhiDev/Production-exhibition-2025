/// <summary>
/// 戦闘中のユニットデータ（味方・敵共通）
/// </summary>
[System.Serializable]
public class UnitCharacter
{
    public int unitId; // 戦闘中の一意なID
    public int characterId; // マスターデータ参照用ID
    public int currentHp;
    public int currentMp;
    public int maxHp;
    public int maxMp;
    public int atk;
    public int def;
    public int speed;

    /// <summary>
    /// シリアライゼーション用デフォルトコンストラクタ
    /// </summary>
    public UnitCharacter() { }

    /// <summary>
    /// 味方キャラクターからユニットを生成する
    /// </summary>
    public UnitCharacter(RuntimeCharacter runtime, int newUnitId)
    {
        unitId = newUnitId;
        characterId = runtime.id;
        maxHp = runtime.maxHp;
        currentHp = runtime.currentHp;
        maxMp = runtime.maxMp;
        currentMp = runtime.currentMp;
        atk = runtime.atk;
        def = runtime.def;
        speed = runtime.speed;
    }

    /// <summary>
    /// 敵キャラクターからユニットを生成する
    /// </summary>
    public UnitCharacter(EnemyMasterSO enemy, int newUnitId)
    {
        unitId = newUnitId;
        characterId = enemy.id;
        maxHp = enemy.hp;
        currentHp = enemy.hp;
        maxMp = enemy.mp;
        currentMp = enemy.mp;
        atk = enemy.atk;
        def = enemy.def;
        speed = enemy.speed;
    }
}
