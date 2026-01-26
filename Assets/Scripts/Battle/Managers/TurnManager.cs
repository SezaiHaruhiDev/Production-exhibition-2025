using UnityEngine;
using System.Collections;

/// <summary>
/// ターン制戦闘の管理
/// </summary>
public class TurnManager : MonoBehaviour
{
    [SerializeField] public BattleDatabaseSO battleDatabaseSO;

    void Start()
    {
        // LoadManagerに設定された次のバトルIDを取得して開始（シーン跨ぎのデータ受け渡し）
        int battleID = LoadManager.Instance.nextBattleId;

        if (battleID != -1)
        {
            Initialize(battleID);
        }
    }

    private void Initialize(int battleId)
    {
        BattleSO bso = battleDatabaseSO.GetById(battleId);
    }
}
