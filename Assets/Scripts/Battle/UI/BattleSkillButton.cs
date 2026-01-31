using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// スキル選択ボタン（ドロップ受け入れ機能付き）
/// </summary>
/// <summary>
/// スキル選択ボタン
/// </summary>
public class BattleSkillButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI skillNameText;

    private SkillData _currentSkill;
    private BattleUIManager _manager;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(BattleUIManager manager)
    {
        _manager = manager;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    /// <summary>
    /// ボタンの内容を設定して表示する
    /// </summary>
    public void Configure(SkillData skill)
    {
        _currentSkill = skill;
        if (skillNameText != null)
        {
            skillNameText.text = skill.displayName;
        }
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ボタンを非表示にする
    /// </summary>
    public void Disable()
    {
        gameObject.SetActive(false);
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
