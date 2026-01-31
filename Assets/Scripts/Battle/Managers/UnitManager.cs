using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// 戦闘中のユニット（味方・敵）の生成と実体の管理を担当するマネージャー
/// </summary>
public class UnitManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Data References")]
    [SerializeField] private CharacterRegistrySO registry;

    [Header("Prefab Settings")]
    [SerializeField] private BattleUnit _battleunitPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] _allySpawnPoints;
    [SerializeField] private Transform[] _enemySpawnPoints;
    #endregion

    #region Private Fields
    private Dictionary<int, BattleUnit> allbattleunits = new();
    public List<BattleUnit> AllUnits => new List<BattleUnit>(allbattleunits.Values);
    private int _nextUnitId = 0;
    private int _currentAllyCount = 0;
    private int _currentEnemyCount = 0;
    #endregion

    private void Awake()
    {
        Assert.IsNotNull(registry, "UnitManager: Registry is not assigned!");
        Assert.IsNotNull(_battleunitPrefab, "UnitManager: BattleUnit Prefab is not assigned!");
        Assert.IsTrue(_allySpawnPoints.Length > 0, "UnitManager: Ally Spawn Points are empty!");
        Assert.IsTrue(_enemySpawnPoints.Length > 0, "UnitManager: Enemy Spawn Points are empty!");
    }

    /// <summary>
    /// 指定されたキャラクターIDのユニットを生成し、適切なポジションに配置する
    /// </summary>
    /// <param name="characterID">マスターデータ上のキャラクターID</param>
    public void AddUnit(int characterID)
    {
        if (registry == null) return;

        CharacterMasterSO master = registry.GetById(characterID);
        if (master == null)
        {
            Debug.LogWarning($"UnitManager: Character ID {characterID} not found in registry.");
            return;
        }

        UnitCharacter _unitData = null;
        bool isAlly = master is AllyMasterSO;

        if (isAlly)
        {
            RuntimeCharacter runtimeCharacter = LoadManager.Instance.GetRuntimeCharacter(characterID);
            if (runtimeCharacter == null) return;
            _unitData = new UnitCharacter(runtimeCharacter, _nextUnitId);
        }
        else if (master is EnemyMasterSO enemy)
        {
            _unitData = new UnitCharacter(enemy, _nextUnitId);
        }

        if (_unitData != null)
        {
            SpawnBattleUnit(_unitData, master, isAlly);
        }
    }

    /// <summary>
    /// 生成済みのRuntimeCharacterを元にユニットを生成・配置する（レンタルやイベント用）
    /// </summary>
    public void AddUnit(RuntimeCharacter character, bool isAlly)
    {
        if (registry == null) return;

        CharacterMasterSO master = registry.GetById(character.id);
        if (master == null)
        {
            Debug.LogWarning($"UnitManager: Master Data for ID {character.id} not found.");
            return;
        }

        UnitCharacter unitData = new UnitCharacter(character, _nextUnitId);

        SpawnBattleUnit(unitData, master, isAlly);
    }

    private void SpawnBattleUnit(UnitCharacter data, CharacterMasterSO master, bool isAlly)
    {
        Transform[] targetPoints = isAlly ? _allySpawnPoints : _enemySpawnPoints;
        int spawnIndex = isAlly ? _currentAllyCount : _currentEnemyCount;

        if (spawnIndex >= targetPoints.Length)
        {
            Debug.LogError($"UnitManager: No more spawn points for {(isAlly ? "Ally" : "Enemy")} (Index: {spawnIndex})");
            return;
        }

        Transform spawnPoint = targetPoints[spawnIndex];
        // 親をspawnPointに設定して生成
        BattleUnit battleUnit = Instantiate(_battleunitPrefab, spawnPoint);
        battleUnit.transform.localPosition = Vector3.zero;
        battleUnit.transform.localRotation = Quaternion.identity;
        
        // オブジェクトの名前をキャラクター名にする
        battleUnit.name = data.name;

        // バトル用のスプライトを優先し、なければBigスプライトをフォールバックとして使う
        Sprite sprite = master.characterBattleSprite != null ? master.characterBattleSprite : master.characterBigSprite;
        
        battleUnit.Setup(data, sprite);
        allbattleunits.Add(data.unitId, battleUnit);
        if (isAlly) _currentAllyCount++; else _currentEnemyCount++;
        _nextUnitId++;
    }
}
