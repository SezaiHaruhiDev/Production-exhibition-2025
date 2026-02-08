using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 戦闘中の演出を管理するクラス（カメラ系を更地に戻した状態）
/// </summary>
public class BattlePresentationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleUIManager _uiManager;
    [SerializeField] private UnitManager _unitManager;

    private void Awake()
    {
        if (_uiManager == null) _uiManager = FindFirstObjectByType<BattleUIManager>();
        if (_unitManager == null) _unitManager = FindFirstObjectByType<UnitManager>();
    }

    private void Start()
    {
        // カメラセットアップ処理を削除
    }

    /// <summary>
    /// 戦闘開始時のカメラ演出（無効化）
    /// </summary>
    public IEnumerator PlayBattleStartSequence()
    {
        // UIを表示するだけに留めるなど、必要最低限の処理があれば記述
        // 基本は何もしない
        yield break;
    }

    /// <summary>
    /// 攻撃演出（カメラワークなし・タイミングとエフェクトのみ）
    /// </summary>
    public IEnumerator PlayAttackSequence(BattleUnit attacker, List<BattleUnit> targets, SkillPerformanceSO performanceData, System.Action onImpact)
    {
        // パラメータ取得
        float antDuration = performanceData != null ? performanceData.anticipationDuration : 0.4f;
        float actionMoveDuration = performanceData != null ? performanceData.actionMovementDuration : 0.15f;
        float hitStopDur = performanceData != null ? performanceData.hitStopDuration : 0.15f;
        float recoveryDelay = performanceData != null ? performanceData.recoveryDelay : 0.5f;

        Vector3 targetCenter = GetUnitsCenter(targets);

        // --- 0. Start Effect ---
        if (performanceData != null && performanceData.startEffectPrefab != null)
        {
            SpawnEffect(performanceData.startEffectPrefab, attacker.transform.position, performanceData.effectScale);
        }

        // --- 透明化処理（行動者とターゲット以外を半透明に） ---
        SetOtherUnitsTransparency(attacker, targets, 0.3f);

        // --- 1. Anticipation (Wait) ---
        yield return new WaitForSeconds(antDuration);

        // --- 2. Action (Wait) ---
        // カメラ移動がないので、単に待つ
        yield return new WaitForSeconds(actionMoveDuration);

        // --- 3. Impact ---
        onImpact?.Invoke();

        // Hit Effect
        if (performanceData != null && performanceData.hitEffectPrefab != null)
        {
            SpawnEffect(performanceData.hitEffectPrefab, targetCenter, performanceData.effectScale);
        }

        // Hit Stop (Time Scale操作) は演出として残す（不要なら削除可）
        yield return StartCoroutine(HitStopRoutine(hitStopDur));
        
        // Recovery Wait
        yield return new WaitForSeconds(recoveryDelay);

        // --- 透明化解除 ---
        SetOtherUnitsTransparency(attacker, targets, 1.0f);
    }

    private void SpawnEffect(GameObject prefab, Vector3 position, float scale)
    {
        if (prefab == null) return;
        var effect = Instantiate(prefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
        Destroy(effect, 3.0f);
    }

    private Vector3 GetUnitsCenter(List<BattleUnit> units)
    {
        if (units == null || units.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (var u in units) sum += u.transform.position;
        return sum / units.Count;
    }

    /// <summary>
    /// ヒットストップ
    /// </summary>
    public IEnumerator HitStopRoutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.05f; 
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
    }

    private void SetOtherUnitsTransparency(BattleUnit attacker, List<BattleUnit> targets, float alpha)
    {
        if (_unitManager == null || _unitManager.AllUnits == null) return;

        foreach (var unit in _unitManager.AllUnits)
        {
            // 攻撃者でもターゲットでもないユニットを対象にする
            if (unit != attacker && (targets == null || !targets.Contains(unit)))
            {
                unit.SetAlpha(alpha);
            }
            else
            {
                // 対象者は不透明に戻す（演出終了時用、または念のため）
                unit.SetAlpha(1.0f);
            }
        }
    }
}
