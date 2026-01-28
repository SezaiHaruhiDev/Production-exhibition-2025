using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

/// <summary>
/// ランタイムキャラクターデータの管理（マスター + 成長データの合成）
/// </summary>
public class LoadManager : MonoBehaviour
{
    public int nextBattleId; // 次戦闘のSO ID（戦闘終了後は-1にリセット）
    public static LoadManager Instance { get; private set; }
    private bool _initialized = false;

    [SerializeField] private CharacterRegistrySO registry;
    [SerializeField] private List<RuntimeCharacter> _runtimeCharacterList = new List<RuntimeCharacter>();
    public IReadOnlyList<RuntimeCharacter> RuntimeCharacterList => _runtimeCharacterList;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        Assert.IsNotNull(registry, "LoadManager: registry is not assigned!");
    }

    /// <summary>
    /// ランタイムデータの初期化（非同期）
    /// セーブデータとマスターデータをマージしてランタイムキャラクターを作成する
    /// </summary>
    public IEnumerator InitializeCoroutine()
    {
        if (_initialized) yield break;
        _initialized = true;

        if (registry == null)
        {
            Debug.LogError("LoadManager: Registry is not assigned!");
            yield break;
        }

        registry.Initialize();

        if (SaveDataManager.Instance != null)
        {
            SaveDataManager.Instance.Initialize(registry.AllyCharacters);
        }

        _runtimeCharacterList.Clear();

        foreach (var master in registry.AllyCharacters)
        {
            var data = SaveDataManager.Instance.GetCharacter(master.id);
            RuntimeCharacter runtime = new RuntimeCharacter(master, data);
            _runtimeCharacterList.Add(runtime);

            yield return null;
        }
    }

    /// <summary>
    /// ランタイムデータの変更タイプ
    /// </summary>
    public enum RuntimeChangeType
    {
        LevelUp,
        HealHp,
        DamageHp
    }

    /// <summary>
    /// 指定されたキャラクターのランタイムデータを更新する
    /// </summary>
    /// <param name="id">キャラクターID</param>
    /// <param name="type">変更内容のタイプ</param>
    public void UpdateRuntimeData(int id, RuntimeChangeType type)
    {
        RuntimeCharacter data = _runtimeCharacterList.FirstOrDefault(r => r.id == id);
        if (data == null)
        {
            Debug.LogError($"LoadManager: RuntimeCharacter not found (ID: {id})");
            return;
        }

        switch (type)
        {
            case RuntimeChangeType.LevelUp:
                data.level++;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 現在のランタイムデータをセーブデータ用辞書に反映し、永続化する
    /// </summary>
    /// <param name="id">キャラクターID</param>
    /// <param name="data">反映元のランタイムデータ</param>
    public void SaveToDictionary(int id, RuntimeCharacter data)
    {
        if (registry == null || SaveDataManager.Instance == null) return;

        var master = registry.GetById(id);
        if (master == null) return;

        var target = SaveDataManager.Instance.GetCharacter(id);
        if (target == null) return;

        target.level = data.level - master.level;
        target.hp = data.maxHp - master.hp;
        target.mp = data.maxMp - master.mp;
        target.atk = data.atk - master.atk;
        target.def = data.def - master.def;
        target.speed = data.speed - master.speed;

        SaveDataManager.Instance.SaveDictionary();
    }

    /// <summary>
    /// 指定IDのランタイムキャラクターデータを取得する
    /// </summary>
    /// <param name="id">キャラクターID</param>
    /// <returns>ランタイムキャラクター（存在しない場合はnull）</returns>
    public RuntimeCharacter GetRuntimeCharacter(int id)
    {
        return _runtimeCharacterList.FirstOrDefault(c => c.id == id);
    }
}