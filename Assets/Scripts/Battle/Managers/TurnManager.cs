using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

/// <summary>
/// ターン制戦闘の管理
/// </summary>
public class TurnManager : MonoBehaviour
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Won,
        Lost
    }

    [Header("References")]
    [SerializeField] private BattleDatabaseSO battleDatabaseSO;
    [SerializeField] private UnitManager unitManager;

    [Header("State")]
    [SerializeField] private BattleState state;
    public BattleState CurrentState => state;

    private void Awake()
    {
        if (unitManager == null) unitManager = FindFirstObjectByType<UnitManager>();
        
        Assert.IsNotNull(battleDatabaseSO, "TurnManager: BattleDatabaseSO is not assigned.");
        Assert.IsNotNull(unitManager, "TurnManager: UnitManager is not assigned or found.");
    }

    private void Start()
    {
        // LoadManagerに設定された次のバトルIDを取得して開始
        int battleID = -1;
        if (LoadManager.Instance != null)
        {
            battleID = LoadManager.Instance.nextBattleId;
        }

        if (battleID != -1)
        {
            StartCoroutine(SetupBattle(battleID));
        }
        else
        {
            Debug.LogWarning("TurnManager: Invalid Battle ID (-1).");
        }
    }

    private IEnumerator SetupBattle(int battleId)
    {
        state = BattleState.Start;
        
        // バトルデータ取得
        BattleSO battleData = battleDatabaseSO.GetById(battleId);
        if (battleData == null)
        {
            Debug.LogError($"TurnManager: Battle Data not found for ID {battleId}");
            yield break;
        }

        // 味方生成
        if (PartyManager.Instance != null)
        {
            var partyIds = PartyManager.Instance.PartyMemberIds;
            foreach (int charId in partyIds)
            {
                if (charId >= 0)
                {
                    unitManager.AddUnit(charId);
                }
            }
        }

        // 敵生成 (Phase 0 のみ仮実装)
        if (battleData.phases != null && battleData.phases.Length > 0)
        {
            var phase = battleData.phases[0];
            foreach (var enemyMaster in phase.allEnemy)
            {
                if (enemyMaster != null)
                {
                     // UnitManager.AddUnitはCharacterIDを受け取る仕様のため、EnemyMasterのIDを渡す
                     // EnemyMasterSOもCharacterMasterSOを継承しているのでIDを持っているはず
                     unitManager.AddUnit(enemyMaster.id);
                }
            }
        }

        yield return new WaitForSeconds(1f);

        state = BattleState.PlayerTurn;
        StartCoroutine(PlayerTurn());
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("Player Turn Started");
        
        // TODO: UI入力待ちなど
        yield return new WaitForSeconds(1f);

        // ターン終了（仮）
        state = BattleState.EnemyTurn;
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn Started");

        // TODO: AI行動など
        yield return new WaitForSeconds(1f);

        // ターン終了（仮）
        state = BattleState.PlayerTurn;
        StartCoroutine(PlayerTurn());
    }
}
