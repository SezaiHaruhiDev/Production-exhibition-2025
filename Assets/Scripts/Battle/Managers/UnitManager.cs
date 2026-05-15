using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

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

    /// <summary>
    /// 生成された全てのユニットリスト
    /// </summary>
    public IEnumerable<BattleUnit> AllUnits => allbattleunits.Values;

    /// <summary>
    /// 現在生存している全ユニットリスト
    /// </summary>
    public List<BattleUnit> ActiveUnits => allbattleunits.Values
        .Where(u => u != null && u.Data.currentHp > 0).ToList();

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
        BattleUnit battleUnit = Instantiate(_battleunitPrefab, spawnPoint);
        battleUnit.transform.localPosition = Vector3.zero;
        battleUnit.transform.localRotation = Quaternion.identity;

        battleUnit.name = data.name;

        Sprite sprite = master.characterBattleSprite != null ? master.characterBattleSprite : master.characterBigSprite;

        battleUnit.Setup(data, sprite);
        allbattleunits.Add(data.unitId, battleUnit);
        if (isAlly) _currentAllyCount++; else _currentEnemyCount++;
        _nextUnitId++;
    }
    /// <summary>
    /// ユニットが死亡した際の処理
    /// </summary>
    public void OnUnitDead(BattleUnit unit)
    {


        if (unit.Data.isAlly)
        {
            var master = registry.GetById(unit.Data.characterId) as AllyMasterSO;
            Sprite downSprite = (master != null) ? master.downSprite : null;

            unit.SetDown(true, downSprite);
        }
        else
        {
            StartCoroutine(FadeOutExit(unit));
        }
    }

    private IEnumerator FadeOutExit(BattleUnit unit)
    {
        unit.IsFadingOut = true;
        // 0.5秒かけてフェードアウト
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            unit.SetAlpha(alpha);
            yield return null;
        }
        unit.gameObject.SetActive(false);
    }
}
