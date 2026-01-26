using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 全キャラクターマスターデータを管理するレジストリ
/// </summary>
[CreateAssetMenu(menuName = "Master/CharacterRegistry")]
public class CharacterRegistrySO : ScriptableObject
{
    [SerializeField] private List<CharacterMasterSO> allCharacters = new List<CharacterMasterSO>();
    public IReadOnlyList<CharacterMasterSO> AllCharacters => allCharacters;

    public IEnumerable<AllyMasterSO> AllyCharacters => allCharacters.OfType<AllyMasterSO>();
    public IEnumerable<EnemyMasterSO> EnemyCharacters => allCharacters.OfType<EnemyMasterSO>();

    private Dictionary<int, CharacterMasterSO> _dict;

    private void OnValidate()
    {
        CheckForDuplicates();
    }

    private bool CheckForDuplicates()
    {
        if (allCharacters == null) return false;

        HashSet<int> ids = new HashSet<int>();
        bool hasDuplicate = false;

        foreach (var c in allCharacters)
        {
            if (c == null) continue;
            if (ids.Contains(c.id))
            {
                Debug.LogError($"[CharacterRegistry] ID重複を検知しました: ID={c.id} (Name={c.characterName})", this);
                hasDuplicate = true;
            }
            ids.Add(c.id);
        }
        return hasDuplicate;
    }

    /// <summary>
    /// キャラクターデータのキャッシュディクショナリを初期化する
    /// </summary>
    public void Initialize()
    {
        _dict = new Dictionary<int, CharacterMasterSO>();

        foreach (var c in allCharacters)
        {
            if (c == null) continue;

            if (_dict.ContainsKey(c.id))
            {
                string errorMsg = $"致命的なエラー: Character ID {c.id} ({c.characterName}) が重複しています。レジストリを確認してください。";
                Debug.LogError(errorMsg);
                throw new Exception(errorMsg);
            }

            _dict.Add(c.id, c);
        }
    }

    /// <summary>
    /// 指定されたIDのキャラクターマスターデータを取得する
    /// </summary>
    /// <param name="id">キャラクターID</param>
    /// <returns>CharacterMasterSO（存在しない場合はnull）</returns>
    public CharacterMasterSO GetById(int id)
    {
        // -1などは「なし」の状態を表すため正常値として扱う
        if (id < 0) return null;

        if (_dict == null)
        {
            Initialize();
        }

        if (_dict.TryGetValue(id, out var character))
            return character;

        Debug.LogError($"Character ID が存在しません: {id}");
        return null;
    }
}
