using UnityEngine;

/// <summary>
/// 感情カードのマスターデータ
/// </summary>
[CreateAssetMenu(menuName = "Emotion/EmotionCard")]
public class EmotionCardData : ScriptableObject
{
    public string emotionName;
    public Emotion emotion;
    [Range(1, 3)]
    public int level = 1;
}
