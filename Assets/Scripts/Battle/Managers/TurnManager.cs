using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;

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
    [SerializeField] private CharacterRegistrySO registry;

    [Header("Managers")]
    [SerializeField] private BattleDatabaseSO battleDatabaseSO;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private BattleUIManager battleUI;
    [SerializeField] private EmotionDeckManager deckManager;

    [Header("State")]
    [SerializeField] private BattleState state;
    public BattleState CurrentState => state;

    [Header("Shared Party Resources")]
    [SerializeField] private int battleMaxMp;
    [SerializeField] private int battleCurrentMp;
    public int BattleMaxMP => battleMaxMp;
    public int BattleCurrentMP => battleCurrentMp;

    public event System.Action<int, int> OnMPChanged;

    private void Awake()
    {
        if (unitManager == null) unitManager = GetComponent<UnitManager>();
        if (battleUI == null) battleUI = FindFirstObjectByType<BattleUIManager>();
        if (deckManager == null) deckManager = GetComponent<EmotionDeckManager>();

        Assert.IsNotNull(battleDatabaseSO, "TurnManager: BattleDatabaseSO is not assigned.");
        Assert.IsNotNull(unitManager, "TurnManager: UnitManager is not assigned or found.");
        if (battleUI == null) Debug.LogWarning("TurnManager: BattleUIManager not found.");
    }

    private void Start()
    {
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
            Debug.LogWarning("TurnManager: Invalid Battle ID (-1).Start debug Battle.");
            StartCoroutine(SetupBattle(0));
        }
    }

    private IEnumerator SetupBattle(int battleId)
    {
        state = BattleState.Start;

        BattleSO battleData = battleDatabaseSO.GetById(battleId);
        if (battleData == null)
        {
            Debug.LogError($"TurnManager: Battle Data not found for ID {battleId}");
            yield break;
        }

        // 味方の生成
        if (battleData.partyType == PartySourceType.Rental)
        {
            if (battleData.rentalParty != null)
            {
                foreach (var rental in battleData.rentalParty.rentalCharacter)
                {
                    RuntimeCharacter runtime = new RuntimeCharacter(rental);
                    unitManager.AddUnit(runtime, true);
                }
            }
        }
        else
        {
            // PartyManagerから現在のパーティーメンバーを取得して生成
            if (PartyManager.Instance != null)
            {
                var partyCharacters = PartyManager.Instance.GetPartyCharacters();
                foreach (var character in partyCharacters)
                {
                    unitManager.AddUnit(character, true);
                }
            }
            else
            {
                Debug.LogWarning("TurnManager: PartyManager.Instance is null. Cannot add player characters.");
            }
        }

        if (battleData.phases != null && battleData.phases.Length > 0)
        {
            var phase = battleData.phases[0];
            foreach (var enemyMaster in phase.allEnemy)
            {
                if (enemyMaster != null)
                {
                    unitManager.AddUnit(enemyMaster.id);
                }
            }
        }

        if (deckManager != null && battleData.deckConfig != null)
        {
            deckManager.InitializeDeck(battleData.deckConfig.deckCards);
        }

        InitializeSharedMP();

        yield return new WaitForSeconds(1f);

        state = BattleState.PlayerTurn; // ここも後で適切なState管理に変える必要あり
        StartCoroutine(RunBattleLoop());
    }

    private IEnumerator RunBattleLoop()
    {
        const float GOAL = 100000f;

        while (state != BattleState.Won && state != BattleState.Lost)
        {
            float minTime = unitManager.AllUnits.Min(u => u.Data.GetRemainingTime(GOAL));
            foreach (var unit in unitManager.AllUnits)
            {
                unit.Data.AdvanceGauge(minTime);
            }

            var actionCharacter = unitManager.AllUnits.FirstOrDefault(u => u.Data.currentActionGauge >= GOAL - 0.01f);

            if (actionCharacter != null)
            {
                yield return StartCoroutine(UnitTurn(actionCharacter));
                actionCharacter.Data.currentActionGauge = 0f;
            }
        }
    }

    public enum CommandState
    {
        SelectSkill,
        SelectTarget,
        Confirmed
    }

    private IEnumerator UnitTurn(BattleUnit unit)
    {
        Debug.Log($"Turn Start: {unit.Data.characterId} (Ally:{unit.Data.isAlly})");

        if (unit.Data.isAlly)
        {
            state = BattleState.PlayerTurn;

            // 味方のターン開始時にカードを1枚ドロー
            if (deckManager != null)
            {
                deckManager.Draw(1);
            }

            CommandState commandState = CommandState.SelectSkill;
            SkillData currentSkill = null;
            EmotionCardData currentEmotion = null;
            List<BattleUnit> selectedTargets = new List<BattleUnit>();

            if (battleUI != null)
            {
                battleUI.ShowSkillPanel(unit.Data);

                while (commandState != CommandState.Confirmed)
                {
                    switch (commandState)
                    {
                        case CommandState.SelectSkill:
                            if (battleUI.IsSkillSelected)
                            {
                                currentSkill = battleUI.SelectedSkill;
                                currentEmotion = battleUI.SelectedEmotion;

                                // ターゲット選択可能状態にする
                                SetupTargetSelection(unit, currentSkill, currentEmotion, true);
                                commandState = CommandState.SelectTarget;
                            }
                            break;

                        case CommandState.SelectTarget:
                            // 常に最新の選択状態をUIから取っておく（感情カードの付け替えに対応）
                            if (battleUI.SelectedSkill != currentSkill || battleUI.SelectedEmotion != currentEmotion)
                            {
                                // 前の選択を解除
                                SetupTargetSelection(unit, currentSkill, currentEmotion, false);

                                currentSkill = battleUI.SelectedSkill;
                                currentEmotion = battleUI.SelectedEmotion;

                                // 新しい条件でセットアップ
                                if (currentSkill != null)
                                {
                                    SetupTargetSelection(unit, currentSkill, currentEmotion, true);
                                }
                            }

                            if (!battleUI.IsSkillSelected)
                            {
                                // スキル解除されたら選択状態をリセット
                                SetupTargetSelection(unit, currentSkill, currentEmotion, false);
                                currentSkill = null;
                                currentEmotion = null;
                                commandState = CommandState.SelectSkill;
                                break;
                            }

                            // クリックによる選択待ち
                            if (_lastClickedUnit != null)
                            {
                                selectedTargets.Clear(); // 選択をリセットしてやり直し

                                SkillTargetType effectiveType = currentSkill.GetEffectiveTargetType(currentEmotion);
                                switch (effectiveType)
                                {
                                    case SkillTargetType.Self:
                                        selectedTargets.Add(unit);
                                        break;
                                    case SkillTargetType.SingleEnemy:
                                    case SkillTargetType.SingleAlly:
                                        selectedTargets.Add(_lastClickedUnit);
                                        break;
                                    case SkillTargetType.AllEnemies:
                                        selectedTargets.AddRange(unitManager.AllUnits.Where(u => !u.Data.isAlly && u.Data.currentHp > 0));
                                        break;
                                    case SkillTargetType.AllAllies:
                                        selectedTargets.AddRange(unitManager.AllUnits.Where(u => u.Data.isAlly && u.Data.currentHp > 0));
                                        break;
                                }

                                SetupTargetSelection(unit, currentSkill, currentEmotion, false);
                                commandState = CommandState.Confirmed;
                            }
                            break;
                    }
                    yield return null;
                }

                battleUI.HideSkillPanel();

                string emoText = currentEmotion != null ? currentEmotion.emotionName : "無し";
                string targetsText = string.Join(", ", selectedTargets.Select(t => t.Data.name));
                Debug.Log($"Player Action: {currentSkill.displayName} on [{targetsText}] (Emotion: {emoText})");

                if (currentEmotion != null && deckManager != null)
                {
                    deckManager.UseCard(currentEmotion);
                }

                // ここでダメージ処理などを実行する
                foreach (var target in selectedTargets)
                {
                    // 簡易的なダメージ計算（本来はSkillEffectクラスなどで処理）
                    int damage = currentSkill.basePower;
                    target.Data.currentHp = Mathf.Max(0, target.Data.currentHp - damage);
                    target.RefreshHPBar();
                    target.ShowDamage(damage); // ダメージ数字を表示
                    Debug.Log($"{target.Data.name} took {damage} damage! HP: {target.Data.currentHp}/{target.Data.maxHp}");
                }

                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            state = BattleState.EnemyTurn;
            Debug.Log("Enemy Action: AI processing...");
            yield return new WaitForSeconds(1f);
        }
    }

    private BattleUnit _lastClickedUnit;

    private void OnUnitClicked(BattleUnit unit)
    {
        _lastClickedUnit = unit;
    }

    private void SetupTargetSelection(BattleUnit actor, SkillData skill, EmotionCardData card, bool active)
    {
        _lastClickedUnit = null;
        if (skill != null && active)
        {
            SkillTargetType effectiveType = skill.GetEffectiveTargetType(card);
            Debug.Log($"[Targeting] Skill: {skill.displayName}, EffectiveTargetType: {effectiveType} (Card: {(card != null ? card.emotionName : "None")})");

            foreach (var unit in unitManager.AllUnits)
            {
                unit.OnSelected -= OnUnitClicked;
                bool isSelectable = false;
                switch (effectiveType)
                {
                    case SkillTargetType.Self:
                        isSelectable = (unit == actor);
                        break;
                    case SkillTargetType.SingleEnemy:
                    case SkillTargetType.AllEnemies:
                        isSelectable = !unit.Data.isAlly;
                        break;
                    case SkillTargetType.SingleAlly:
                    case SkillTargetType.AllAllies:
                        isSelectable = unit.Data.isAlly;
                        break;
                }

                unit.SetSelectable(isSelectable);
                if (isSelectable)
                {
                    unit.OnSelected += OnUnitClicked;
                }
            }
        }
        else
        {
            // 全解除
            foreach (var unit in unitManager.AllUnits)
            {
                unit.OnSelected -= OnUnitClicked;
                unit.SetSelectable(false);
            }
        }
    }


    private void Update()
    {
        // デバッグ用: NキーでMPを100消費、Pキーで100回復
        if (Input.GetKeyDown(KeyCode.N))
        {
            ConsumeMP(100);
            Debug.Log($"Debug: Consumed 100 MP. Current: {battleCurrentMp}");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            battleCurrentMp = Mathf.Min(battleMaxMp, battleCurrentMp + 100);
            OnMPChanged?.Invoke(battleCurrentMp, battleMaxMp);
            Debug.Log($"Debug: Restored 100 MP. Current: {battleCurrentMp}");
        }

        // デバッグ用: Hキーで味方全員のHP減少、Jキーで回復
        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (var unit in unitManager.AllUnits.Where(u => u.Data.isAlly))
            {
                unit.Data.currentHp = Mathf.Max(0, unit.Data.currentHp - 10);
                unit.RefreshHPBar();
                unit.ShowDamage(10);
            }
            Debug.Log("Debug: Allies HP -10");
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            foreach (var unit in unitManager.AllUnits.Where(u => u.Data.isAlly))
            {
                unit.Data.currentHp = Mathf.Min(unit.Data.maxHp, unit.Data.currentHp + 10);
                unit.RefreshHPBar();
            }
            Debug.Log("Debug: Allies HP +10");
        }
    }

    private void InitializeSharedMP()
    {
        battleMaxMp = 0;
        battleCurrentMp = 0;

        foreach (var unit in unitManager.AllUnits)
        {
            if (unit.Data.isAlly)
            {
                battleMaxMp += unit.Data.maxMp;
                battleCurrentMp += unit.Data.currentMp;
            }
        }

        Debug.Log($"Shared MP Initialized: {battleCurrentMp}/{battleMaxMp}");
        OnMPChanged?.Invoke(battleCurrentMp, battleMaxMp);
    }

    /// <summary>
    /// 共有MPを消費する
    /// </summary>
    public void ConsumeMP(int amount)
    {
        battleCurrentMp = Mathf.Max(0, battleCurrentMp - amount);
        OnMPChanged?.Invoke(battleCurrentMp, battleMaxMp);
    }
}
