using UnityEngine;
using TMPro;

/// <summary>
/// スキルの説明文を表示するシンプルなパネル
/// </summary>
public class SkillDescriptionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    /// <summary>
    /// スキルの説明を表示
    /// </summary>
    public void ShowSkill(SkillData skill, EmotionCardData card)
    {
        if (skill == null || descriptionText == null) return;

        // スキル名とコスト（あれば）を太字などで際立たせる
        string costText = skill.imaginationCost > 0 ? $" (MP:{skill.imaginationCost})" : "";
        string header = $"<b>【{skill.displayName}{costText}】</b>\n";
        
        string displayBody = skill.description;

        // カードがある場合、特殊な説明文に差し替える
        if (card != null)
        {
            var effects = skill.GetEffectiveEffects(card);
            if (effects != null && effects.Count > 0)
            {
                string specializedDesc = "";
                foreach (var effect in effects)
                {
                    if (!string.IsNullOrEmpty(effect.effectexplanation))
                    {
                        specializedDesc += effect.effectexplanation + "\n";
                    }
                }

                if (!string.IsNullOrEmpty(specializedDesc))
                {
                    displayBody = specializedDesc.TrimEnd();
                }
            }
        }

        descriptionText.text = header + displayBody;

        gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// カード単体の説明を表示
    /// </summary>
    public void ShowCard(EmotionCardData card)
    {
        if (card == null || descriptionText == null) return;

        // カード単体の説明
        descriptionText.text = $"<b>【{card.emotionName}】</b>\nレベル {card.level} の感情カード。";
        
        gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}
