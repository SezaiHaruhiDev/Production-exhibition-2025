using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// キャラクターの成長データを管理（JSON永続化）
/// </summary>
public class SaveDataManager
{
    private static SaveDataManager _instance;
    public static SaveDataManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveDataManager();
            return _instance;
        }
    }

    private Dictionary<int, CharacterData> _characterDict = new();
    public IReadOnlyDictionary<int, CharacterData> CharacterDict => _characterDict;

    private string path;

    private SaveDataManager()
    {
        path = Path.Combine(Application.persistentDataPath, "character_data_dictionary.json");
    }

    /// <summary>
    /// 全キャラクターデータの初期読み込みを行う
    /// </summary>
    public void Initialize(IEnumerable<CharacterMasterSO> masters)
    {
        LoadDictionary(masters);
    }

    /// <summary>
    /// JSONファイルから辞書データを読み込み、マスターデータとの整合性を取る
    /// </summary>
    public void LoadDictionary(IEnumerable<CharacterMasterSO> masters)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                _characterDict =
                    JsonConvert.DeserializeObject<Dictionary<int, CharacterData>>(json)
                    ?? new Dictionary<int, CharacterData>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load dictionary: {e.Message}");
                _characterDict = new Dictionary<int, CharacterData>();
            }
        }
        else
        {
            _characterDict.Clear();
        }

        // バージョンアップ等でマスターデータに新キャラが追加された場合、セーブデータ側にもデフォルト値として追記する（データ整合性の維持）
        foreach (var master in masters)
        {
            if (!_characterDict.ContainsKey(master.id))
            {
                _characterDict[master.id] = new CharacterData
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
        }
        SaveDictionary();
    }
    /// <summary>
    /// 現在のキャラクターデータをJSONファイルとして保存する
    /// </summary>
    public void SaveDictionary()
    {
        string json =
            JsonConvert.SerializeObject(_characterDict, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// 指定されたIDのキャラクターデータを取得する（存在しない場合はデフォルト値を生成して返す）
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

    /// <summary>
    /// キャラクターデータを更新し、即座に保存する
    /// </summary>
    public void SetCharacter(int id, CharacterData data)
    {
        _characterDict[id] = data;
        SaveDictionary();
    }

    /// <summary>
    /// 指定キャラクターのデータを初期状態にリセットする
    /// </summary>
    public void ResetCharacter(int id)
    {
        _characterDict[id] = CreateDefaultCharacterData();
        SaveDictionary();
    }

    /// <summary>
    /// キャラクターを所持状態（アンロック）にする
    /// </summary>
    public void UnlockCharacter(int id)
    {
        var data = GetCharacter(id);
        data.IsOwned = true;
        SaveDictionary();
    }

    /// <summary>
    /// キャラクターを未所持（ロック）状態にする
    /// </summary>
    public void LockCharacter(int id)
    {
        var data = GetCharacter(id);
        data.IsOwned = false;
        SaveDictionary();
    }

    /// <summary>
    /// 所持している全キャラクターのIDリストを取得する
    /// </summary>
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

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("character_data_dictionary.json を削除しました: " + path);
        }

        foreach (var master in masters)
        {
            _characterDict[master.id] = CreateDefaultCharacterData();
        }

        SaveDictionary();
    }
}
