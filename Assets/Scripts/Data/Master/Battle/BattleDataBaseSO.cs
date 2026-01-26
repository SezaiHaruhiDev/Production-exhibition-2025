using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全バトルデータを管理するデータベース
/// </summary>
[CreateAssetMenu(menuName = "Battle/BattleDatabase")]
public class BattleDatabaseSO : ScriptableObject
{
    [SerializeField]
    private List<BattleSO> battles = new List<BattleSO>();
    private Dictionary<int, BattleSO> _cache;

    /// <summary>
    /// 重複のない辞書（キャッシュ）を作成し、ID指定の検索を高速化(O(1))する
    /// </summary>
    public void Initialize()
    {
        _cache = new Dictionary<int, BattleSO>();
        foreach (var battle in battles)
        {
            if (_cache.ContainsKey(battle.battleID))
            {
                Debug.LogError($"BattleID duplicated: {battle.battleID}");
                continue;
            }
            _cache.Add(battle.battleID, battle);
        }
    }

    /// <summary>
    /// 指定されたIDのバトルデータを取得する
    /// </summary>
    /// <param name="id">BattleID</param>
    /// <returns>BattleSO（存在しない場合はnull）</returns>
    public BattleSO GetById(int id)
    {
        if (_cache == null)
        {
            Initialize();
        }

        if (_cache.TryGetValue(id, out var battle))
        {
            return battle;
        }

        Debug.LogError($"BattleID not found: {id}");
        return null;
    }
}
