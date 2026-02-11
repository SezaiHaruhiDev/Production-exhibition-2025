using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 将来の行動順を計算するマネージャ
/// </summary>
public class TurnOrderManager : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private UnitManager unitManager;

    private void Awake()
    {
        if (turnManager == null) turnManager = GetComponent<TurnManager>() ?? FindFirstObjectByType<TurnManager>();
        if (unitManager == null) unitManager = GetComponent<UnitManager>() ?? FindFirstObjectByType<UnitManager>();
    }

    /// <summary>
    /// 指定されたターン数分の行動順を予測して返す
    /// </summary>
    public List<BattleUnit> CalculateTurnOrder(int turnsToPredict)
    {
        if (turnManager == null || unitManager == null) return new List<BattleUnit>();

        var activeUnits = unitManager.ActiveUnits;
        if (activeUnits.Count == 0) return new List<BattleUnit>();

        // シミュレーション用の軽量データを作成
        // 参照元のBattleUnitと、シミュレーション中で変化するActionGaugeを持つ
        var simStates = activeUnits.Select(u => new SimulationUnit
        {
            SourceUnit = u,
            CurrentGauge = u.Data.currentActionGauge,
            Speed = u.Data.speed
        }).ToList();

        List<BattleUnit> result = new List<BattleUnit>();
        int safetyCount = 0;

        while (result.Count < turnsToPredict && safetyCount < turnsToPredict * 10) // 無限ループ防止
        {
            safetyCount++;

            // 1. ゴール到達までの最短時間を計算
            // time = (Goal - Gauge) / Speed
            // Speedが0以下の場合は除外（動けない）
            var validUnits = simStates.Where(s => s.Speed > 0).ToList();
            if (validUnits.Count == 0) break;

            float minTime = validUnits.Min(s => (TurnManager.ACTION_GAUGE_GOAL - s.CurrentGauge) / s.Speed);

            // 2. 全員のゲージを進める
            foreach (var sim in simStates)
            {
                sim.CurrentGauge += sim.Speed * minTime;
            }

            // 3. ゴールに到達したユニットを抽出
            // 実際には浮動小数点の誤差があるため、Goal - epsilon で判定
            var reachedUnits = simStates
                .Where(s => s.CurrentGauge >= TurnManager.ACTION_GAUGE_GOAL - 0.01f)
                .OrderByDescending(s => s.CurrentGauge) // ゲージが多い順（オーバーフロー分）に行動
                .ToList();

            foreach (var reached in reachedUnits)
            {
                result.Add(reached.SourceUnit);
                if (result.Count >= turnsToPredict) break;

                // 行動後はゲージを消費
                reached.CurrentGauge -= TurnManager.ACTION_GAUGE_GOAL;
            }
        }

        return result;
    }

    private class SimulationUnit
    {
        public BattleUnit SourceUnit;
        public float CurrentGauge;
        public int Speed;
    }
}
