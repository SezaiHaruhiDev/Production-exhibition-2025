using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 感情カードデータのデータベース
/// </summary>
[CreateAssetMenu(menuName = "Emotion/EmotionRegistry")]
public class EmotionRegistrySO : ScriptableObject
{
    [SerializeField] private List<EmotionCardData> emotionList;

    /// <summary>
    /// 名前から感情データを検索する
    /// </summary>
    public EmotionCardData GetByName(string name)
    {
        return emotionList.FirstOrDefault(e => e.emotionName == name);
    }

    /// <summary>
    /// タイプとレベルから検索する
    /// </summary>
    public EmotionCardData GetByTypeAndLevel(Emotion type, int level)
    {
        return emotionList.FirstOrDefault(e => e.emotion == type && e.level == level);
    }
}
