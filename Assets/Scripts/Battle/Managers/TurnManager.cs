using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine.InputSystem;

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
    [SerializeField] private SkillDatabaseSO skillDatabaseSO;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private BattleUIManager battleUI;
    [SerializeField] private EmotionDeckManager deckManager;
    [SerializeField] private BattlePresentationManager presentationManager;

    [Header("Victory Condition")]
    [SerializeField] private VictoryConditionSO defaultVictoryCondition;
    private VictoryConditionSO _currentVictoryCondition;

    [Header("State")]
    [SerializeField] private BattleState state;
    public BattleState CurrentState => state;

    [Header("Shared Party Resources")]
    [SerializeField] private int battleMaxMp;
    [SerializeField] private int battleCurrentMp;
    public int BattleMaxMP => battleMaxMp;
    public int BattleCurrentMP => battleCurrentMp;

    [Header("Ultimate System")]
    [SerializeField] private float battleUltimateGauge; // 0.0 to 100.0
    public float BattleUltimateGauge => battleUltimateGauge;
    public bool IsUltimateReady => battleUltimateGauge >= 100f;
    private bool _isInterrupting = false;
    private struct UltimateAction
    {
        public BattleUnit actor;
        public List<BattleUnit> targets;
    }
    private Queue<UltimateAction> _ultimateQueue = new Queue<UltimateAction>();
    private bool _skipTurnRequested = false;
    private const int SKIP_TURN_MP_RECOVERY = 20;

    public UnitManager UnitManager => unitManager;


    public event System.Action<int, int> OnMPChanged;
    public event System.Action<float, float> OnUltimateChanged;
    public event System.Action<int> OnTurnCountChanged;

    [Header("Time Based Turn")]
    [SerializeField] private float avPerTurn = 1000f; // 1000行動値ごとに1ターンとカウントする
    public const float ACTION_GAUGE_GOAL = 100000f;
    private float _totalAV = 0f;
    private int _currentTurnCount = 1;

    public int CurrentTurnCount => _currentTurnCount;

    private void Awake()
    {
        if (unitManager == null) unitManager = GetComponent<UnitManager>();
        if (battleUI == null) battleUI = FindFirstObjectByType<BattleUIManager>();
        if (deckManager == null) deckManager = GetComponent<EmotionDeckManager>();
        if (presentationManager == null) presentationManager = FindFirstObjectByType<BattlePresentationManager>();

        Assert.IsNotNull(battleDatabaseSO, "TurnManager: BattleDatabaseSO is not assigned.");
        Assert.IsNotNull(skillDatabaseSO, "TurnManager: SkillDatabaseSO is not assigned.");
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
        if (battleData != null && battleData.battleBGM != null)
        {
            SoundManager.Instance.PlayBGM(battleData.battleBGM);
        }
        if (battleData == null)
        {
            Debug.LogError($"TurnManager: Battle Data not found for ID {battleId}");
            yield break;
        }

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

        _currentVictoryCondition = battleData.victoryCondition;
        if (_currentVictoryCondition == null)
        {
            _currentVictoryCondition = defaultVictoryCondition;
        }

        // --- 演出開始 ---
        if (presentationManager != null)
        {
            yield return StartCoroutine(presentationManager.PlayBattleStartSequence());
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        state = BattleState.PlayerTurn;
        battleUltimateGauge = 0;
        OnUltimateChanged?.Invoke(battleUltimateGauge, 100f);
        
        StartCoroutine(RunBattleLoop());
        StartCoroutine(UltimateInterruptionProcessor());
    }

    private IEnumerator RunBattleLoop()
    {
        // const float GOAL = 100000f; // Replaced by ACTION_GAUGE_GOAL


        // 初期ターン通知
        OnTurnCountChanged?.Invoke(_currentTurnCount);

        while (state != BattleState.Won && state != BattleState.Lost)
        {
            // RunBattleLoopからは必殺技の直接実行を削除（UltimateInterruptionProcessorが担当）

            if (_currentVictoryCondition != null)
            {
                BattleState result = _currentVictoryCondition.CheckVictory(this);
                if (result != BattleState.Start)
                {
                    state = result;
                    EndBattle();
                    yield break;
                }
            }

            var activeUnits = unitManager.ActiveUnits;
            if (activeUnits.Count == 0) yield break; 

            // 一番早くターンが回ってくるまでの時間（Action Value）を計算
            float minTime = activeUnits.Min(u => u.Data.GetRemainingTime(ACTION_GAUGE_GOAL));
            
            // 時間を進める
            foreach (var unit in activeUnits)
            {
                unit.Data.AdvanceGauge(minTime);
            }

            // グローバルな経過時間（Action Value）を加算し、ターン数を更新
            _totalAV += minTime;
            int nextTurnCount = Mathf.FloorToInt(_totalAV / avPerTurn) + 1;
            if (nextTurnCount > _currentTurnCount)
            {
                _currentTurnCount = nextTurnCount;
                OnTurnCountChanged?.Invoke(_currentTurnCount);
                Debug.Log($"[TurnManager] Global Turn Advanced: {_currentTurnCount} (Total AV: {_totalAV})");
            }

            // 目標値に達したユニットを探す
            // 複数いる場合は、目標値を「より大きく超えた（=溢れた）」順に処理する
            var readyUnits = activeUnits
                .Where(u => u.Data.currentActionGauge >= ACTION_GAUGE_GOAL - 0.01f)
                .OrderByDescending(u => u.Data.currentActionGauge)
                .ToList();

            foreach (var actionCharacter in readyUnits)
            {
                if (state == BattleState.Won || state == BattleState.Lost) break;
                if (!unitManager.ActiveUnits.Contains(actionCharacter)) continue; // 途中で死亡した場合など

                while (_isInterrupting) yield return null;
                yield return StartCoroutine(UnitTurn(actionCharacter));
                
                // 目標値を「引く」ことで、オーバーした分の速度を次回のターンに持ち越す
                actionCharacter.Data.currentActionGauge -= ACTION_GAUGE_GOAL;
            }

            yield return null; // Safety yield to prevent infinite loop/freeze
        }
    }

    private void EndBattle()
    {
        Debug.Log($"Battle Ended! Result: {state}");
        SoundManager.Instance.StopBGM();
        if (state == BattleState.Won)
        {
            Debug.Log("YOU WIN!");
        }
        else if (state == BattleState.Lost)
        {
            Debug.Log("YOU LOSE...");
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
        unit.SetTurnActive(true);
        _skipTurnRequested = false;
        while (_isInterrupting) yield return null;


        if (unit.Data.isAlly)
        {
            state = BattleState.PlayerTurn;

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

                                SetupTargetSelection(unit, currentSkill, currentEmotion, true);
                                commandState = CommandState.SelectTarget;
                            }
                            else if (!battleUI.IsSkillPanelActive && !_isInterrupting)
                            {
                                // 必殺技選択などでパネルが閉じられた場合に再表示する
                                battleUI.ShowSkillPanel(unit.Data);
                            }
                            break;

                        case CommandState.SelectTarget:
                            if (battleUI.SelectedSkill != currentSkill || battleUI.SelectedEmotion != currentEmotion)
                            {
                                SetupTargetSelection(unit, currentSkill, currentEmotion, false);

                                currentSkill = battleUI.SelectedSkill;
                                currentEmotion = battleUI.SelectedEmotion;

                                if (currentSkill != null)
                                {
                                    SetupTargetSelection(unit, currentSkill, currentEmotion, true);
                                }
                            }

                            if (!battleUI.IsSkillSelected)
                            {
                                SetupTargetSelection(unit, currentSkill, currentEmotion, false);
                                currentSkill = null;
                                currentEmotion = null;
                                commandState = CommandState.SelectSkill;
                                break;
                            }

                            if (_lastClickedUnit != null)
                            {
                                selectedTargets.Clear();

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
                                        selectedTargets.AddRange(unitManager.AllUnits.Where(u => !u.Data.isAlly));
                                        break;
                                    case SkillTargetType.AllAllies:
                                        selectedTargets.AddRange(unitManager.AllUnits.Where(u => u.Data.isAlly));
                                        break;
                                }

                                SetupTargetSelection(unit, currentSkill, currentEmotion, false);
                                commandState = CommandState.Confirmed;
                            }
                            break;
                    }
                    if (_skipTurnRequested)
                    {
                        Debug.Log("Turn skipped by player.");
                        AddMP(SKIP_TURN_MP_RECOVERY);
                        
                        // 全てのターゲットマークを消去
                        foreach (var u in unitManager.AllUnits)
                        {
                            u.SetSelectable(false);
                            u.OnSelected -= OnUnitClicked;
                        }

                        commandState = CommandState.Confirmed;
                        currentSkill = null;
                        currentEmotion = null;
                        selectedTargets.Clear();
                        break;
                    }
                    yield return null;
                }

                battleUI.HideSkillPanel();

                string emoText = currentEmotion != null ? currentEmotion.emotionName : "無し";
                string targetsText = string.Join(", ", selectedTargets.Select(t => t.Data.name));


                if (currentEmotion != null && deckManager != null)
                {
                    // スロットに入れた時点で手札からは抜けているので、
                    // ここでは単に「捨て札に送る」だけでよい。
                    // notifyHand: false にすることで、手札UIの再描画による意図しない2重消費を防ぐ。
                    deckManager.DiscardCard(currentEmotion, false);
                    
                    if (battleUI != null) battleUI.ClearSlot();
                }

                if (currentSkill != null)
                {
                    if (currentSkill.imaginationCost > 0)
                    {
                        ConsumeMP(currentSkill.imaginationCost);
                    }
                    AddUltimateCharge(currentSkill.ultimateChargeValue);
                    
                    // ここで非同期実行を待つ
                    yield return StartCoroutine(SkillExecutor.ExecuteAsync(unit, selectedTargets, currentSkill, currentEmotion, presentationManager));
                }

                yield return new WaitForSeconds(0.2f); // 少し短縮（演出が含まれるため）
            }
        }
        else
        {
            state = BattleState.EnemyTurn;
            
            var master = registry.GetById(unit.Data.characterId) as EnemyMasterSO;
            if (master != null && master.aiLogic != null)
            {
                yield return StartCoroutine(master.aiLogic.ExecuteTurn(unit, this));
            }
            else
            {
                Debug.LogWarning($"Enemy {unit.Data.name} has no AI Logic assigned or Master not found!");
                yield return null; // Safety yield
            }
        }

        // ターン終了時にスロットに残っている場合は手札に戻す
        if (battleUI != null) battleUI.ResetSlotToHand();
        unit.SetTurnActive(false);
    }

    /// <summary>
    /// IDからスキルデータを取得する（AIなどが使用）
    /// </summary>
    public SkillData GetSkillData(int id)
    {
        if (skillDatabaseSO != null) return skillDatabaseSO.GetById(id);
        return null;
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
                    case SkillTargetType.AllAllies:
                         isSelectable = unit.Data.isAlly;
                         
                         
                         break;
                    case SkillTargetType.SingleAlly:
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
            foreach (var unit in unitManager.AllUnits)
            {
                unit.OnSelected -= OnUnitClicked;
                unit.SetSelectable(false);
            }
        }
    }


    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            ConsumeMP(100);
        }
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            AddMP(100);
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            foreach (var unit in unitManager.AllUnits.Where(u => u.Data.isAlly))
            {
                unit.Data.currentHp = Mathf.Max(0, unit.Data.currentHp - 10);
                unit.RefreshHPBar();
                unit.ShowDamage(10);
            }
            Debug.Log("Debug: Allies HP -10");
        }
        if (Keyboard.current.jKey.wasPressedThisFrame)
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

    /// <summary>
    /// 共有MPを回復する
    /// </summary>
    public void AddMP(int amount)
    {
        battleCurrentMp = Mathf.Min(battleMaxMp, battleCurrentMp + amount);
        OnMPChanged?.Invoke(battleCurrentMp, battleMaxMp);
    }

    /// <summary>
    /// アルティメットゲージをチャージする
    /// </summary>
    public void AddUltimateCharge(float amount)
    {
        battleUltimateGauge = Mathf.Min(100f, battleUltimateGauge + amount);
        OnUltimateChanged?.Invoke(battleUltimateGauge, 100f);
    }

    /// <summary>
    /// アルティメット割り込みモードを開始または終了する
    /// </summary>
    public void TryActivateUltimateMode()
    {
        // すでに実行アニメーション中ならトグル（キャンセル）不可
        if (_ultimateQueue.Count > 0) return;

        if (_isInterrupting)
        {
            // 選択中であればキャンセル可能
            if (battleUI != null && battleUI.IsInUltimateSelection)
            {
                CancelUltimateMode();
                battleUI.CancelUltimateSelection();
                return;
            }
            return;
        }

        if (!IsUltimateReady) return;

        _isInterrupting = true;
        Time.timeScale = 0.1f; // スローモーション

        // UIにターゲット選択を促す（BattleUIManager側で処理）
        if (battleUI != null)
        {
            battleUI.EnterUltimateSelectionMode();
        }
    }

    /// <summary>
    /// 必殺技を予約する
    /// </summary>
    public void EnqueueUltimate(BattleUnit unit, List<BattleUnit> targets)
    {
        _ultimateQueue.Enqueue(new UltimateAction { actor = unit, targets = targets });
        Time.timeScale = 1.0f; 
        _isInterrupting = false;
        
        battleUltimateGauge = 0; 
        OnUltimateChanged?.Invoke(battleUltimateGauge, 100f);
    }

    /// <summary>
    /// メインループとは独立して必殺技を即時実行する監視コルーチン
    /// </summary>
    private IEnumerator UltimateInterruptionProcessor()
    {
        while (state != BattleState.Won && state != BattleState.Lost)
        {
            if (_ultimateQueue.Count > 0)
            {
                var action = _ultimateQueue.Dequeue();
                yield return StartCoroutine(ExecuteUltimate(action.actor, action.targets));
                
                // 全ての予約済み必殺技が終わったら、割り込みを解除
                if (_ultimateQueue.Count == 0)
                {
                    _isInterrupting = false;
                    Time.timeScale = 1.0f;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 指定したユニットで必殺技を実行（独立プロセッサから呼ばれる）
    /// </summary>
    private IEnumerator ExecuteUltimate(BattleUnit unit, List<BattleUnit> targets)
    {
        unit.SetTurnActive(true);
        SkillData ultSkill = skillDatabaseSO.GetById(unit.Data.ultimateSkillId);
        if (ultSkill != null)
        {
            yield return StartCoroutine(SkillExecutor.ExecuteAsync(unit, targets, ultSkill, null, presentationManager));
            yield return new WaitForSeconds(0.2f);
        }
        unit.SetTurnActive(false);
    }

    /// <summary>
    /// 割り込みをキャンセルする
    /// </summary>
    public void CancelUltimateMode()
    {
        Time.timeScale = 1.0f;
        _isInterrupting = false;
    }

    /// <summary>
    /// 現在のターンをスキップするリクエストを送る
    /// </summary>
    public void RequestTurnSkip()
    {
        if (state == BattleState.PlayerTurn && !_isInterrupting)
        {
            _skipTurnRequested = true;
        }
    }
}
