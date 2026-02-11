using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// スキル選択ボタン（ドロップ受け入れ機能付き）
/// </summary>
public class BattleSkillButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image skillIconImage;

    private SkillData _currentSkill;
    private BattleUIManager _manager;
    private TurnManager _turnManager;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(BattleUIManager manager)
    {
        _manager = manager;
        _turnManager = Object.FindFirstObjectByType<TurnManager>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    /// <summary>
    /// ボタンの内容を設定して表示する
    /// </summary>
    public void Configure(SkillData skill)
    {
        _currentSkill = skill;
        
        // アイコンの設定
        if (skillIconImage != null)
        {
            if (skill.skillIcon != null)
            {
                skillIconImage.sprite = skill.skillIcon;
                skillIconImage.gameObject.SetActive(true);
            }
            else
            {
                // アイコンがない場合はボタン自体を非表示にする（テキスト廃止のため）
                skillIconImage.gameObject.SetActive(false);
                Debug.LogWarning($"Skill '{skill.displayName}' has no icon assigned!");
            }
        }

        gameObject.SetActive(true);
        
        int currentMP = _turnManager?.BattleCurrentMP ?? 0;
        UpdateUsability(currentMP);
    }

    /// <summary>
    /// ボタンを非表示にする
    /// </summary>
    public void Disable()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 現在のMPに応じてボタンの有効・無効を切り替える
    /// </summary>
    public void UpdateUsability(int currentMP)
    {
        if (_currentSkill == null || _manager == null) return;
        
        bool hasEnoughMP = currentMP >= _currentSkill.imaginationCost;
        bool isReviveEligible = true;

        // 蘇生スキルの場合、死亡者がいるかチェック
        if (_currentSkill.IsReviveSkill(_manager.SelectedEmotion))
        {
            isReviveEligible = _manager.HasDeadAllies();
        }

        bool canUse = hasEnoughMP && isReviveEligible;
        button.interactable = canUse;

        // 見た目のフィードバック
        Color feedbackColor = canUse ? Color.white : new Color(1, 1, 1, 0.5f);
        
        if (skillIconImage != null)
        {
            skillIconImage.color = feedbackColor;
        }
    }

    /// <summary>
    /// クリック時
    /// </summary>
    private void OnClicked()
    {
        if (_manager != null)
        {
            _manager.OnSkillSelected(_currentSkill);
        }
    }
}
