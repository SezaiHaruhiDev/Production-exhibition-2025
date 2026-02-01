using UnityEngine;

/// <summary>
/// キャラクターの成長曲線を定義するScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "Data/GrowthTable")]
public class GrowthTableSO : ScriptableObject
{
    [Header("Level Settings")]
    public int maxLevel = 99;
    public int expForMaxLevel = 1000000;
    
    /// <summary>
    /// レベル進捗(0~1)に対する必要経験値のカーブ
    /// 時間軸(0=Lv1, 1=MaxLv) -> 値(0~1: 累積Exp割合)
    /// </summary>
    public AnimationCurve expCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Stat Growth Limits (Gain from Lv1 to Max)")]
    public int maxHpGain = 1000;
    public int maxMpGain = 100;
    public int maxAtkGain = 200;
    public int maxDefGain = 200;
    public int maxSpeedGain = 50;

    /// <summary>
    /// ステータス上昇カーブ
    /// 時間軸(0=Lv1, 1=MaxLv) -> 値(0~1: 成長率)
    /// </summary>
    public AnimationCurve statCurve = AnimationCurve.Linear(0, 0, 1, 1);

    /// <summary>
    /// 指定レベルに必要な累積経験値を取得する
    /// </summary>
    public int GetRequiredExpForLevel(int level)
    {
        if (level <= 1) return 0;
        if (level >= maxLevel) return expForMaxLevel;

        float t = (float)(level - 1) / (maxLevel - 1);
        float curveValue = expCurve.Evaluate(t);
        return Mathf.RoundToInt(expForMaxLevel * curveValue);
    }

    /// <summary>
    /// 指定された累積経験値から、到達しているレベルを計算する
    /// </summary>
    public int GetLevelFromExp(int currentExp)
    {
        if (currentExp <= 0) return 1;
        if (currentExp >= expForMaxLevel) return maxLevel;

        // 簡易的な逆引き：本来はカーブの逆関数が必要だが、
        // 単調増加を前提として 1..MaxLevel でループチェックするか二分探索を行う
        
        // 二分探索で探す
        int low = 1;
        int high = maxLevel;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            int required = GetRequiredExpForLevel(mid);

            if (required == currentExp) return mid;
            
            if (required < currentExp)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // lowが「次に到達すべきレベル」になるため、現在は low-1
        return Mathf.Max(1, low - 1);
    }

    /// <summary>
    /// 指定レベル時点でのステータス加算値（Lv1からの増加分）を取得する
    /// </summary>
    public (int hp, int mp, int atk, int def, int spd) GetStatGain(int level)
    {
        if (level <= 1) return (0, 0, 0, 0, 0);

        float t = (float)(level - 1) / ( maxLevel - 1);
        // レベル上限を超えている場合は1.0（カンスト）として扱う
        if (level > maxLevel) t = 1.0f;

        float rate = statCurve.Evaluate(t);

        int hp = Mathf.RoundToInt(maxHpGain * rate);
        int mp = Mathf.RoundToInt(maxMpGain * rate);
        int atk = Mathf.RoundToInt(maxAtkGain * rate);
        int def = Mathf.RoundToInt(maxDefGain * rate);
        int spd = Mathf.RoundToInt(maxSpeedGain * rate);

        return (hp, mp, atk, def, spd);
    }
}
