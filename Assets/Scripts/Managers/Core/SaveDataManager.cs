using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// キャラクターの成長データを管理（JSON永続化）
/// </summary>
public class SaveDataManager
{
    private static SaveDataManager _instance;
    public static SaveDataManager Instance => _instance ??= new SaveDataManager();

    private Dictionary<int, CharacterData> _characterDict = new();
    public IReadOnlyDictionary<int, CharacterData> CharacterDict => _characterDict;

    private readonly string _savePath;
    private const string SaveFileName = "character_data_dictionary.json";

    private SaveDataManager()
    {
        _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    /// <summary>
    /// 全キャラクターデータの初期読み込みを行う
    /// </summary>
    public void Initialize(IEnumerable<CharacterMasterSO> masters)
    {
        Assert.IsNotNull(masters, "SaveDataManager: Masters list cannot be null.");
        LoadDictionary(masters);
    }

    public void LoadDictionary(IEnumerable<CharacterMasterSO> masters)
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                _characterDict = JsonConvert.DeserializeObject<Dictionary<int, CharacterData>>(json) 
                                 ?? new Dictionary<int, CharacterData>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveDataManager: Load failed. {e.Message}");
                _characterDict = new Dictionary<int, CharacterData>();
            }
        }
        else
        {
            _characterDict.Clear();
        }

        foreach (var master in masters)
        {
            if (!_characterDict.ContainsKey(master.id))
            {
                _characterDict[master.id] = CreateDefaultCharacterData();
            }
        }
        SaveDictionary();
    }

    public void SaveDictionary()
    {
        string json = JsonConvert.SerializeObject(_characterDict, Formatting.Indented);
        File.WriteAllText(_savePath, json);
    }

    /// <summary>
    /// 指定されたIDのキャラクターデータを取得する
    /// </summary>
    public CharacterData GetCharacter(int id)
    {
        if (!_characterDict.TryGetValue(id, out var data))
        {
            data = CreateDefaultCharacterData();
            _characterDict[id] = data;
        }
        return data;
    }

    public void SetCharacter(int id, CharacterData data)
    {
        _characterDict[id] = data;
        SaveDictionary();
    }

    public void ResetCharacter(int id)
    {
        _characterDict[id] = CreateDefaultCharacterData();
        SaveDictionary();
    }

    public void UnlockCharacter(int id)
    {
        var data = GetCharacter(id);
        data.IsOwned = true;
        SaveDictionary();
    }

    public void LockCharacter(int id)
    {
        var data = GetCharacter(id);
        data.IsOwned = false;
        SaveDictionary();
    }

    public List<int> GetOwnedCharacterIds()
    {
        List<int> ownedIds = new List<int>();

        foreach (var pair in _characterDict)
        {
            if (pair.Value.IsOwned)
            {
                ownedIds.Add(pair.Key);
            }
        }
        return ownedIds;
    }

    private CharacterData CreateDefaultCharacterData()
    {
        return new CharacterData
        {
            level = 0,
            exp = 0,
            hp = 0,
            mp = 0,
            atk = 0,
            def = 0,
            speed = 0,
            IsOwned = false,
            skillId = new List<int>()
        };
    }

    /// <summary>
    /// 全てのセーブデータを消去し、初期化する（デバッグ用）
    /// </summary>
    public void ResetAllData(IEnumerable<CharacterMasterSO> masters)
    {
        _characterDict.Clear();

        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log($"SaveDataManager: Deleted {_savePath}");
        }

        foreach (var master in masters)
        {
            _characterDict[master.id] = CreateDefaultCharacterData();
        }

        SaveDictionary();
    }
}
