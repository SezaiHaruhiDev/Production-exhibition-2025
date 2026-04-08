using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

/// <summary>
/// 戦闘UIの総括管理
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    [Header("Skill Panel")]
    [SerializeField] private List<BattleSkillButton> skillButtons;
    [SerializeField] private GameObject skillPanelRoot;

    [Header("Card Slot")]
    [SerializeField] private CardSlotUI cardSlot;

    [Header("Emotion Card UI")]
    [SerializeField] private GameObject emotionCardPrefab;
    [SerializeField] private Transform handArea;
    [SerializeField] private RectTransform deckPosition;
    [SerializeField] private RectTransform animationCenter;

    [Header("Shared Resources")]
    [SerializeField] private SharedMPBarUI mpBar;
    [SerializeField] private UltimateGaugeController ultGauge;
    [SerializeField] private Button ultimateButton;
    [SerializeField] private Button turnEndButton;
    [SerializeField] private Button drawButton;
    [SerializeField] private CanvasGroup generalInteractionGroup;
    [SerializeField] private TMPro.TextMeshProUGUI turnText;
    [SerializeField] private TMPro.TextMeshProUGUI promptText; // 行動指示メッセージ用
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private AudioClip targetSelectSE;
    [SerializeField] private AudioClip ultimateReadySE;

    [Header("Start Sequence")]
    [SerializeField] private CanvasGroup battleStartGroup;
    [SerializeField] private TMPro.TextMeshProUGUI battleStartText;

    private bool _wasUltimateReady = false;

    [Header("Skill Description")]
    [SerializeField] private SkillDescriptionPanel descriptionPanel;

    [Header("Data")]
    [SerializeField] private SkillDatabaseSO skillDatabase;

    private TurnManager _turnManager;
    private EmotionDeckManager _deckManager;
    private const int DRAW_MP_COST = 30;

    /// <summary>
    /// 現在選択されているスキル
    /// </summary>
    public SkillData SelectedSkill { get; private set; }

    /// <summary>
    /// 現在選択されている感情カード
    /// </summary>
    public EmotionCardData SelectedEmotion => cardSlot != null ? cardSlot.CurrentCard : null;

    /// <summary>
    /// スキルが選択されているかどうか
    /// </summary>
    public bool IsSkillSelected => SelectedSkill != null;
    public bool IsInUltimateSelection => _isInUltimateSelection;
    public bool IsSkillPanelActive => skillPanelRoot != null && skillPanelRoot.activeSelf;

    private bool _isInUltimateSelection = false;
    private BattleUnit _tempUltimateActor;
    private SkillData _tempUltimateSkill;

    private List<BattleEmotionCard> _activeCardUIs = new List<BattleEmotionCard>();
    private List<EmotionCardData> _cardsRequestingNoAnimation = new List<EmotionCardData>();

    private void Awake()
    {
        _deckManager = FindAnyObjectByType<EmotionDeckManager>();

        if (handArea != null && handArea.GetComponent<HandLayoutGroup>() == null)
        {
            handArea.gameObject.AddComponent<HandLayoutGroup>();
        }

        foreach (var btn in skillButtons)
        {
            btn.Initialize(this);
            btn.Disable();
        }
        if (battleStartGroup != null) battleStartGroup.alpha = 0f;
        skillPanelRoot.SetActive(false);

        if (cardSlot == null) cardSlot = FindAnyObjectByType<CardSlotUI>();

        _turnManager = FindAnyObjectByType<TurnManager>();
        if (_turnManager != null)
        {
            _turnManager.OnMPChanged += (current, max) =>
            {
                if (mpBar != null) mpBar.UpdateView(current, max);
                UpdateButtonsUsability();
            };

            _turnManager.OnUltimateChanged += (current, max) =>
            {
                if (ultGauge != null) ultGauge.UpdateView(current, max);

                bool isReady = (current >= max);
                if (ultimateButton != null) ultimateButton.interactable = isReady;

                // たまった瞬間だけSEを鳴らす
                if (isReady && !_wasUltimateReady)
                {
                    if (ultimateReadySE != null && SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlaySE(ultimateReadySE);
                    }
                }
                _wasUltimateReady = isReady;
            };

            _turnManager.OnTurnCountChanged += (turn) =>
            {
                if (turnText != null) turnText.text = $"TURN:{turn:D2}";
            };
        }

        if (_deckManager != null)
        {
            _deckManager.OnHandChanged += () =>
            {
                UpdateHandView(_deckManager.Hand);
                UpdateButtonsUsability(); // 手札枚数が変わったのでドローボタンの状態も更新
            };
        }

        if (cardSlot != null)
        {
            cardSlot.OnCardRemoved += (card) =>
            {
                if (_deckManager != null)
                {
                    _cardsRequestingNoAnimation.Add(card);
                    _deckManager.AddCard(card);
                }
                UpdateButtonsUsability();
                if (SelectedSkill != null) ShowSkillDescription(SelectedSkill);
            };
            cardSlot.OnCardSet += (card) =>
            {
                if (_deckManager != null) _deckManager.RemoveCardFromHand(card);
                UpdateButtonsUsability();
                if (SelectedSkill != null) ShowSkillDescription(SelectedSkill);
            };
        }

        if (ultimateButton != null)
        {
            ultimateButton.onClick.AddListener(() => _turnManager.TryActivateUltimateMode());
            ultimateButton.interactable = false;
        }

        if (turnEndButton != null)
        {
            turnEndButton.onClick.AddListener(() =>
            {
                ResetSlotToHand();
                _turnManager.RequestTurnSkip();
            });
            turnEndButton.gameObject.SetActive(false);
        }

        if (drawButton != null)
        {
            drawButton.onClick.AddListener(() =>
            {
                if (_deckManager != null && _turnManager != null)
                {
                    if (_turnManager.BattleCurrentMP >= DRAW_MP_COST &&
                        _deckManager.Hand.Count < _deckManager.MaxHandSize &&
                        _deckManager.CanDraw)
                    {
                        _turnManager.ConsumeMP(DRAW_MP_COST);
                        _deckManager.Draw(1);
                    }
                }
            });
            drawButton.gameObject.SetActive(false);
        }

        SetInteraction(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (_turnManager != null)
        {
            // 初期表示の同期
            if (mpBar != null) mpBar.UpdateView(_turnManager.BattleCurrentMP, _turnManager.BattleMaxMP);
            if (ultGauge != null) ultGauge.UpdateView(_turnManager.BattleUltimateGauge, 100f);

            UpdateButtonsUsability();
        }
    }

    /// <summary>
    /// 指定ユニットのスキルパネルを表示する
    /// </summary>
    public void ShowSkillPanel(UnitCharacter unit)
    {
        SelectedSkill = null;
        // 以前のスキルのターゲット選択状態などをリセットするが、
        // スロットのカードは継続して使えるように Clear() を外す
        skillPanelRoot.SetActive(true);
        if (turnEndButton != null) turnEndButton.gameObject.SetActive(true);
        if (drawButton != null) drawButton.gameObject.SetActive(true);
        if (promptText != null)
        {
            promptText.text = "スキルを選択して発動してください";
            promptText.gameObject.SetActive(true);
        }
        SetInteraction(true);
        UpdateButtonsUsability(); // 初期化時に状態を反映

        int count = Mathf.Min(unit.skillIds.Count, skillButtons.Count);
        int currentMP = _turnManager != null ? _turnManager.BattleCurrentMP : 0;

        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (i < count)
            {
                int sId = unit.skillIds[i];
                SkillData skill = skillDatabase.GetById(sId);

                if (skill != null)
                {
                    skillButtons[i].Configure(skill);
                }
                else
                {
                    skillButtons[i].Disable();
                }
            }
            else
            {
                skillButtons[i].Disable();
            }
        }
    }

    /// <summary>
    /// スキルパネルを非表示にする
    /// </summary>
    public void HideSkillPanel()
    {
        skillPanelRoot.SetActive(false);
        // ここでも、必殺技割り込みなどで閉じられた際にカードが消えないよう Clear() を呼ぶのをやめる
        if (turnEndButton != null) turnEndButton.gameObject.SetActive(false);
        if (drawButton != null) drawButton.gameObject.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
        SetInteraction(false);
        SelectedSkill = null; // スキル選択解除
        ForceHideDescription();
    }

    /// <summary>
    /// スロットを完全に空にする（手札に戻さない）
    /// </summary>
    public void ClearSlot()
    {
        if (cardSlot != null) cardSlot.Clear();
    }

    /// <summary>
    /// ターンの終了やリセット時に、スロットのカードを手札に戻して空にする
    /// </summary>
    public void ResetSlotToHand()
    {
        if (cardSlot != null && cardSlot.CurrentCard != null)
        {
            var card = cardSlot.CurrentCard;
            cardSlot.Clear();
            var deckManager = FindAnyObjectByType<EmotionDeckManager>();
            if (deckManager != null)
            {
                deckManager.AddCard(card);
            }
        }
    }

    /// <summary>
    /// スキル選択時の処理を行う
    /// </summary>
    public void OnSkillSelected(SkillData skill, EmotionCardData emotion = null)
    {
        if (SelectedSkill == skill)
        {
            SelectedSkill = null;
            HideDescription();
            if (promptText != null) promptText.text = "スキルを選択して発動してください";
        }
        else
        {
            SelectedSkill = skill;
            ShowSkillDescription(skill);
            if (promptText != null) promptText.text = "ターゲットを選択してください";
        }
    }

    public void UpdateHandView(IReadOnlyList<EmotionCardData> handData)
    {
        if (handArea == null) return;

        // データの重複を許容するリスト（現在の手札の状態）
        List<EmotionCardData> remainingNewData = new List<EmotionCardData>(handData);
        List<BattleEmotionCard> nextActiveUIs = new List<BattleEmotionCard>();

        // 1. 既存のカードの中から、新しい手札データにも存在し続けているものを残す
        foreach (var existingUI in _activeCardUIs)
        {
            // 消費された（すでにスロットに入った、または合成された）UIは再利用せず、破棄対象にする
            if (existingUI != null && !existingUI.IsConsumed && remainingNewData.Contains(existingUI.Data))
            {
                nextActiveUIs.Add(existingUI);
                remainingNewData.Remove(existingUI.Data); // 使ったデータは抜く
            }
            else if (existingUI != null)
            {
                // 手札から消えたカード、または無効なカードは破棄
                // すでに非表示にされている場合もあるが、ここで確実にGameObjectごと消す
                Destroy(existingUI.gameObject);
            }
        }

        // 2. 新しく増えたデータ分だけカードを生成し、ドローアニメーションを再生
        foreach (var newData in remainingNewData)
        {
            var obj = Instantiate(emotionCardPrefab, handArea);
            var card = obj.GetComponent<BattleEmotionCard>();
            if (card != null)
            {
                card.Setup(newData);
                nextActiveUIs.Add(card);

                // 山札の位置と中心位置が指定されていれば、そこを考慮してドローさせる
                // スロットからの返却など、アニメーション不要な場合は再生しない
                if (_cardsRequestingNoAnimation.Contains(newData))
                {
                    _cardsRequestingNoAnimation.Remove(newData);
                }
                else if (deckPosition != null && animationCenter != null)
                {
                    card.PlayDrawAnimation(deckPosition.position, animationCenter.position);
                }
                else if (deckPosition != null)
                {
                    card.PlayDrawAnimation(deckPosition.position, Vector3.zero);
                }
            }
        }

        _activeCardUIs = nextActiveUIs;

        // レイアウトグループの即時更新依頼
        var layout = handArea.GetComponent<HandLayoutGroup>();
        if (layout != null) layout.UpdateLayout();
    }

    private void UpdateButtonsUsability()
    {
        int currentMP = _turnManager != null ? _turnManager.BattleCurrentMP : 0;
        foreach (var btn in skillButtons)
        {
            if (btn.gameObject.activeSelf)
            {
                btn.UpdateUsability(currentMP);
            }
        }

        // ドローボタンの有効判定
        if (drawButton != null && _deckManager != null)
        {
            bool hasMp = currentMP >= DRAW_MP_COST;
            bool hasSpace = _deckManager.Hand.Count < _deckManager.MaxHandSize;
            bool hasCards = _deckManager.CanDraw;
            drawButton.interactable = hasMp && hasSpace && hasCards;
        }
    }

    /// <summary>
    /// 死亡している味方がいるかどうかを判定する
    /// </summary>
    public bool HasDeadAllies()
    {
        if (_turnManager == null || _turnManager.UnitManager == null) return false;
        return _turnManager.UnitManager.AllUnits.Any(u => u.Data.isAlly && u.Data.currentHp <= 0);
    }

    /// <summary>
    /// アルティメット発動用の選択モードに入る
    /// </summary>
    public void EnterUltimateSelectionMode()
    {
        _isInUltimateSelection = true;
        UIEffectSpawner.Instance?.Play();

        if (promptText != null)
        {
            promptText.text = "必殺技を発動する味方を選択してください";
            promptText.gameObject.SetActive(true);
        }

        // 既存の（通常ターンの）ターゲット選択をすべて解除
        if (_turnManager != null && _turnManager.UnitManager != null)
        {
            foreach (var u in _turnManager.UnitManager.AllUnits)
            {
                u.SetSelectable(false);
            }
        }

        ResetSlotToHand(); // スロットのカードを手札に戻す
        skillPanelRoot.SetActive(false); // HideSkillPanel()の代わりに直接消す（Clearを避けるため）

        // 味方を選択可能にする
        List<BattleUnit> allies = new List<BattleUnit>();
        foreach (var unit in _turnManager.UnitManager.AllUnits)
        {
            if (unit.Data.isAlly && unit.Data.currentHp > 0)
            {
                unit.SetSelectable(true);
                unit.OnSelected += OnUltimateUnitSelected;
                allies.Add(unit);
            }
            else
            {
                unit.SetSelectable(false);
            }
        }

        // アルティメット発動者選択の演出（味方全員を強調）
        _turnManager.PresentationManager?.SetTargetCandidatesTransparency(null, allies, 0.3f);
    }

    private void OnUltimateUnitSelected(BattleUnit unit)
    {
        if (!_isInUltimateSelection) return;

        PlayTargetSelectSE();

        if (promptText != null) promptText.text = "必殺技の対象を選択してください";

        // 選択された味方を保持
        _tempUltimateActor = unit;
        _tempUltimateSkill = _turnManager.GetSkillData(unit.Data.ultimateSkillId);

        if (_tempUltimateSkill == null)
        {
            Debug.LogError($"Ultimate Skill not found for {unit.Data.name}");
            CancelUltimateSelection();
            return;
        }

        // 必殺技の説明を表示
        ShowSkillDescription(_tempUltimateSkill);

        // 以前のリスナーを解除
        foreach (var u in _turnManager.UnitManager.AllUnits)
        {
            u.OnSelected -= OnUltimateUnitSelected;
        }

        // 次のステップ：ターゲット選択
        SetupUltimateTargetSelection(_tempUltimateActor, _tempUltimateSkill);
    }

    private void SetupUltimateTargetSelection(BattleUnit actor, SkillData skill)
    {
        SkillTargetType effectiveType = skill.targetType; // ウルトなのでEmotionCardは一旦なし

        // 全てのユニットの選択状態を一旦クリア
        foreach (var u in _turnManager.UnitManager.AllUnits)
        {
            u.OnSelected -= OnUltimateUnitSelected;
            u.OnSelected -= OnUltimateTargetSelected;
            u.SetSelectable(false);
        }

        // 全てのターゲットタイプにおいて、「クリックによる決定」を必須にする。
        List<BattleUnit> candidates = new List<BattleUnit>();
        foreach (var unit in _turnManager.UnitManager.AllUnits)
        {
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
                // 「クリックされたら、そのターゲットタイプに応じた全対象に実行する」というロジックにする
                unit.OnSelected += OnUltimateTargetSelected;
                candidates.Add(unit);
            }
        }

        // アルティメットのターゲット選択演出
        _turnManager.PresentationManager?.SetTargetCandidatesTransparency(actor, candidates, 0.3f);
    }

    private void OnUltimateTargetSelected(BattleUnit clickedTarget)
    {
        PlayTargetSelectSE();
        // クリックされたターゲットから、実際にスキルが及ぶ全対象をリストアップする
        List<BattleUnit> finalTargets = new List<BattleUnit>();
        SkillTargetType effectiveType = _tempUltimateSkill.targetType;

        switch (effectiveType)
        {
            case SkillTargetType.Self:
                finalTargets.Add(_tempUltimateActor);
                break;
            case SkillTargetType.SingleEnemy:
            case SkillTargetType.SingleAlly:
                finalTargets.Add(clickedTarget);
                break;
            case SkillTargetType.AllEnemies:
                finalTargets.AddRange(_turnManager.UnitManager.AllUnits.Where(u => !u.Data.isAlly));
                break;
            case SkillTargetType.AllAllies:
                finalTargets.AddRange(_turnManager.UnitManager.AllUnits.Where(u => u.Data.isAlly));
                break;
        }

        FinalizeUltimate(finalTargets);
    }

    private void FinalizeUltimate(List<BattleUnit> targets)
    {
        _isInUltimateSelection = false;
        ForceHideDescription();

        if (promptText != null) promptText.gameObject.SetActive(false);

        // すべてのリスナーと選択状態を解除
        foreach (var u in _turnManager.UnitManager.AllUnits)
        {
            u.OnSelected -= OnUltimateUnitSelected;
            u.OnSelected -= OnUltimateTargetSelected;
            u.SetSelectable(false);
        }

        // 予約（キュー追加）
        _turnManager.EnqueueUltimate(_tempUltimateActor, targets);

        _tempUltimateActor = null;
        _tempUltimateSkill = null;

        // ウルト選択が終わったら一旦UIロック（次のターン開始時に解除される）
        SetInteraction(false);
    }

    public void CancelUltimateSelection()
    {
        _isInUltimateSelection = false;

        if (promptText != null) promptText.gameObject.SetActive(false);

        foreach (var u in _turnManager.UnitManager.AllUnits)
        {
            u.OnSelected -= OnUltimateUnitSelected;
            u.OnSelected -= OnUltimateTargetSelected;
            u.SetSelectable(false);
        }

        _tempUltimateActor = null;
        _tempUltimateSkill = null;

        // 演出のリセット：通常の行動キャラ強調に戻す
        var actor = _turnManager.ActiveUnit;
        if (actor != null)
        {
            _turnManager.PresentationManager?.SetOtherUnitsTransparency(actor, null, 0.3f);
        }
    }

    /// <summary>
    /// 全般的なUI操作（スキル、カード等）の有効/無効を切り替える
    /// </summary>
    public void SetInteraction(bool interactable)
    {
        if (generalInteractionGroup != null)
        {
            generalInteractionGroup.interactable = interactable;
            generalInteractionGroup.blocksRaycasts = interactable;
        }
    }

    /// <summary>
    /// UI全体の表示/非表示を切り替える（演出用）
    /// </summary>
    public void SetUIVisibility(bool visible)
    {
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = visible ? 1f : 0f;
            rootCanvasGroup.interactable = visible;
            rootCanvasGroup.blocksRaycasts = visible;
        }
    }
    /// <summary>
    /// ターゲット選択時のSEを再生する
    /// </summary>
    public void PlayTargetSelectSE()
    {
        if (targetSelectSE != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(targetSelectSE);
        }
    }

    /// <summary>
    /// 戦闘開始の演出（スプラッシュ文字）を再生する
    /// </summary>
    public IEnumerator PlayBattleStartSplash(string text)
    {
        yield return StartCoroutine(PlayGenericSplash(text, 1.5f));
    }

    /// <summary>
    /// 戦闘結果（VICTORY / DEFEATなど）を表示する
    /// </summary>
    public IEnumerator PlayBattleEndSplash(string text)
    {
        // 終了時は長時間出しておきたいため、待ち時間を長めにする
        yield return StartCoroutine(PlayGenericSplash(text, 5.0f));
    }

    private IEnumerator PlayGenericSplash(string text, float waitDuration)
    {
        if (battleStartGroup == null || battleStartText == null) yield break;

        battleStartText.text = text;
        RectTransform rect = battleStartText.rectTransform;

        // 初期状態
        battleStartGroup.alpha = 0f;
        rect.anchoredPosition = new Vector2(-1200f, 0f);

        // スライドイン
        battleStartGroup.DOFade(1f, 0.2f);
        yield return rect.DOAnchorPos(Vector2.zero, 0.6f).SetEase(Ease.OutBack).WaitForCompletion();

        yield return new WaitForSeconds(waitDuration);

        // スライドアウト
        battleStartGroup.DOFade(0f, 0.3f);
        yield return rect.DOAnchorPos(new Vector2(1200f, 0f), 0.5f).SetEase(Ease.InBack).WaitForCompletion();

        rect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// スキルの説明を表示する
    /// </summary>
    public void ShowSkillDescription(SkillData skill)
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.ShowSkill(skill, SelectedEmotion);
        }
    }

    /// <summary>
    /// カードの説明を表示する
    /// </summary>
    public void ShowCardDescription(EmotionCardData card)
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.ShowCard(card);
        }
    }

    /// <summary>
    /// 説明パネルを非表示にする（スキル選択中ならその説明に戻す）
    /// </summary>
    public void HideDescription()
    {
        if (_isInUltimateSelection && _tempUltimateSkill != null)
        {
            ShowSkillDescription(_tempUltimateSkill);
        }
        else if (SelectedSkill != null)
        {
            ShowSkillDescription(SelectedSkill);
        }
        else if (descriptionPanel != null)
        {
            descriptionPanel.Hide();
        }
    }

    /// <summary>
    /// 説明パネルを強制的に非表示にする
    /// </summary>
    public void ForceHideDescription()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.Hide();
        }
    }
}
