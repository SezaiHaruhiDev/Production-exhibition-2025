using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 行動順リストの個々のアイコン表示
/// </summary>
public class TurnOrderUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Color allyFrameColor = Color.blue;
    [SerializeField] private Color enemyFrameColor = Color.red;

    // オプション: AV値を表示するテキストなど
    // [SerializeField] private TMPro.TextMeshProUGUI avText; 

    public void Setup(BattleUnit unit, Sprite sprite)
    {
        if (unit == null || unit.Data == null) return;
        
        if (sprite != null)
        {
            iconImage.sprite = sprite;
        }

        if (frameImage != null)
        {
            frameImage.color = unit.Data.isAlly ? allyFrameColor : enemyFrameColor;
        }
    }
}
