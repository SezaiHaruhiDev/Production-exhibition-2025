using System.Collections.Generic;

/// <summary>
/// レンタルキャラクターデータ（マスターデータに対する補正値を保持）
/// </summary>
[System.Serializable]
public class RentalCharacter
{
    public AllyMasterSO rentalAlly;
    // マスターデータの初期値に対する「差分」または「上書き用補正値」を保持する
    public int adjustmentHp;
    public int adjustmentMp;
    public int adjustmentAtk;
    public int adjustmentDef;
    public int adjustmentLevel;
    public int adjustmentSpeed;
    public List<int> skillID = new List<int>();
}
