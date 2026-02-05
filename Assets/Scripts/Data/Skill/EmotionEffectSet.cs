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
    
    [UnityEngine.Tooltip("この感情発動時の演出オーバーライド（任意）")]
    public SkillPerformanceSO performanceOverride;
}
