using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 戦闘UIの総括管理
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    [Header("Skill Panel")]
    [Header("Skill Panel")]
    [SerializeField] private List<BattleSkillButton> skillButtons;
    [SerializeField] private GameObject skillPanelRoot;

    [Header("Card Slot")]
    [SerializeField] private CardSlotUI cardSlot;

    [Header("Emotion Card UI")]
    [SerializeField] private GameObject emotionCardPrefab;
    [SerializeField] private Transform handArea;

    [Header("Shared Resources")]
    [SerializeField] private SharedMPBarUI mpBar;

    [Header("Data")]
    [SerializeField] private SkillDatabaseSO skillDatabase;

    public SkillData SelectedSkill { get; private set; }
    
    // 現在選択中の感情＝スロットに入っているカード
    public EmotionCardData SelectedEmotion => cardSlot != null ? cardSlot.CurrentCard : null;
    
    public bool IsSkillSelected => SelectedSkill != null;

    private void Awake()
    {
        foreach (var btn in skillButtons)
        {
            btn.Initialize(this);
            btn.Disable();
        }
        skillPanelRoot.SetActive(false);
        
        if (cardSlot == null) cardSlot = FindFirstObjectByType<CardSlotUI>();

        // TurnManagerのイベントを購読してMPゲージを更新する
        var turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null && mpBar != null)
        {
            turnManager.OnMPChanged += (current, max) => mpBar.UpdateView(current, max);
        }

        // EmotionDeckManagerのイベントを購読して手札UIを更新する
        var deckManager = FindFirstObjectByType<EmotionDeckManager>();
        if (deckManager != null)
        {
            deckManager.OnHandChanged += () => UpdateHandView(deckManager.Hand);
            
            // スロットからカードが外されたら手札に戻す
            if (cardSlot != null)
            {
                cardSlot.OnCardRemoved += (card) => deckManager.AddCard(card);
                // スロットに入ったら手札データから抜く（増殖防止）
                cardSlot.OnCardSet += (card) => deckManager.RemoveCardFromHand(card);
            }
        }
    }

    /// <summary>
    /// 指定ユニットのスキルパネルを表示する
    /// </summary>
    public void ShowSkillPanel(UnitCharacter unit)
    {
        SelectedSkill = null;
        if (cardSlot != null) cardSlot.Clear();
        skillPanelRoot.SetActive(true);

        int count = Mathf.Min(unit.skillIds.Count, skillButtons.Count);

        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (i < count)
            {
                string sId = unit.skillIds[i].ToString();
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
        if (cardSlot != null) cardSlot.Clear();
    }

    public void OnSkillSelected(SkillData skill, EmotionCardData emotion = null)
    {
        // ターゲット中にもう一度同じボタンを押したらキャンセル
        if (SelectedSkill == skill)
        {
            SelectedSkill = null;
            Debug.Log("Selection Cleared (Toggle)");
        }
        else
        {
            SelectedSkill = skill;
            Debug.Log($"Selected Skill: {skill.displayName}");
        }
    }

    /// <summary>
    /// 手札UIの更新（DeckManagerから呼ばれる、またはイベントで叩く）
    /// </summary>
    public void UpdateHandView(IReadOnlyList<EmotionCardData> handData)
    {
        foreach (Transform child in handArea)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in handData)
        {
            var obj = Instantiate(emotionCardPrefab, handArea);
            var card = obj.GetComponent<BattleEmotionCard>();
            if (card != null)
            {
                card.Setup(data);
            }
        }
    }
}
