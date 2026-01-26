using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 戦闘中のユニット（味方・敵）を管理
/// </summary>
public class UnitManager : MonoBehaviour
{
    private Dictionary<int, UnitCharacter> _unitMap = new();
    [SerializeField] private int _nextUnitId = 0;
    [SerializeField] private CharacterRegistrySO registry;

    /// <summary>
    /// 指定されたキャラクターIDのユニット（味方または敵）を生成し、リストに追加する
    /// </summary>
    public void AddUnit(int characterID)
    {
        CharacterMasterSO character = registry.GetById(characterID);
        if (character == null) return;


        UnitCharacter _unit = null;

        if (character is AllyMasterSO)
        {
            // 味方は成長データ（RuntimeCharacter）を反映した状態で生成する
            RuntimeCharacter runtimeCharacter = LoadManager.Instance.GetRuntimeCharacter(characterID);
            if (runtimeCharacter == null) return;
            _unit = new UnitCharacter(runtimeCharacter, _nextUnitId);
        }
        else if (character is EnemyMasterSO enemy)
        {
            // 敵は固有の成長データがないため、マスターデータから直接生成する
            _unit = new UnitCharacter(enemy, _nextUnitId);
        }

        if (_unit != null)
        {
            _unitMap.Add(_nextUnitId, _unit);
            _nextUnitId++;
        }
    }
}
