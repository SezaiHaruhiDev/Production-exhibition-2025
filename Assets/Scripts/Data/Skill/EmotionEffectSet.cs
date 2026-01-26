using System.Collections.Generic;

/// <summary>
/// 感情レベルに応じたスキル効果セット
/// </summary>
[System.Serializable]
public class EmotionEffectSet
{
    public Emotion emotion;
    public int level;
    public List<EffectData> effectDataList;
}
