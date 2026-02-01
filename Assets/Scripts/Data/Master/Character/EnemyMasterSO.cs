using UnityEngine;

/// <summary>
/// 敵キャラクターのマスターデータ
/// </summary>
[CreateAssetMenu(menuName = "Master/Enemy")]
public class EnemyMasterSO : CharacterMasterSO
{
    public EnemyLogicSO aiLogic;
}
