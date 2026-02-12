using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

/// <summary>
/// 戦闘中の演出を管理するクラス（カメラ系を更地に戻した状態）
/// </summary>
public class BattlePresentationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleUIManager _uiManager;
    [SerializeField] private UnitManager _unitManager;

    // --- 透明度管理の状態保持 ---
    private BattleUnit _ambientFocusActor; // 現在のターン担当者（常に強調されるべきベース）
    private BattleUnit _currentFocusActor;
    private List<BattleUnit> _currentFocusGroup;
    private float _currentDimAlpha = 1.0f;

    [Header("End Sequence Settings")]
    [SerializeField] private Vector3 allyVictoryCameraOffset;
    [SerializeField] private Vector3 enemyVictoryCameraOffset;
    [SerializeField] private float cameraZoomDuration = 1.5f;

    private void Reset()
    {
        //初期値の設定
        allyVictoryCameraOffset = new Vector3(-4f, 2f, -8f);
        enemyVictoryCameraOffset = new Vector3(4f, 2f, -8f);
        cameraZoomDuration = 1.5f;
    }

    private void Awake()
    {
        if (_uiManager == null) _uiManager = FindObjectOfType<BattleUIManager>();
        if (_unitManager == null) _unitManager = FindObjectOfType<UnitManager>();
    }


    public IEnumerator PlayBattleEndSequence(bool isVictory)
    {
        if (Camera.main == null) yield break;

        if (_uiManager != null)
        {
            _uiManager.SetUIVisibility(false); // UIを隠す
        }

        // カメラズーム（勝った陣営の方へ）
        Vector3 targetPos = isVictory ? allyVictoryCameraOffset : enemyVictoryCameraOffset;
        Camera.main.transform.DOMove(targetPos, cameraZoomDuration).SetEase(Ease.OutCubic);
        // ついでにFOVも少し絞る（より印象的に）
        Camera.main.DOFieldOfView(40f, cameraZoomDuration).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.5f);

        // 文字スプラッシュ
        string splashText = isVictory ? "VICTORY" : "DEFEAT";
        if (_uiManager != null)
        {
            yield return StartCoroutine(_uiManager.PlayBattleEndSplash(splashText));
        }

        // ここでリザルト画面への遷移などのコールバックを待つ形にするのが一般的
    }

    /// <summary>
    /// 戦闘開始時のカメラ演出（無効化）
    /// </summary>
    public IEnumerator PlayBattleStartSequence()
    {
        if (_uiManager != null)
        {
            _uiManager.SetUIVisibility(false); // UIを一旦隠す
            yield return StartCoroutine(_uiManager.PlayBattleStartSplash("BATTLE START"));
            _uiManager.SetUIVisibility(true);  // UIを出す
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 攻撃演出（連番エフェクト、ノックバック、SE、タイミング調整込み）
    /// </summary>
    public IEnumerator PlayAttackSequence(BattleUnit attacker, List<BattleUnit> targets, GameObject effectPrefab, float hitDelay, float yOffset, bool playKnockback, AudioClip hitSE, System.Action onImpact)
    {
        float antDuration = 0.2f;
        float recoveryDelay = 0.6f;

        // --- フォーカス開始 ---
        SetOtherUnitsTransparency(attacker, targets, 0.3f);
        
        if (playKnockback) attacker.PlayStepAction();
        else attacker.PlayJumpAction();

        yield return new WaitForSeconds(antDuration);

        if (effectPrefab != null)
        {
            foreach (var t in targets)
            {
                if (t != null)
                {
                    Vector3 spawnPos = t.transform.position + new Vector3(0, yOffset, -0.1f);
                    SpawnEffect(effectPrefab, spawnPos, 1.0f);
                }
            }
        }

        yield return new WaitForSeconds(hitDelay);

        if (hitSE != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySE(hitSE);
            else AudioSource.PlayClipAtPoint(hitSE, Camera.main.transform.position);
        }

        onImpact?.Invoke();

        if (playKnockback)
        {
            foreach (var t in targets)
            {
                if (t != null) t.PlayKnockback();
            }
            yield return StartCoroutine(HitStopRoutine(0.1f));
        }
        
        yield return new WaitForSeconds(recoveryDelay);

        // --- 演出終了：リセット ---
        ResetAllTransparency();
    }

    private void SpawnEffect(GameObject prefab, Vector3 position, float scale)
    {
        if (prefab == null) return;
        var effect = Instantiate(prefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;

        // レイヤーを最前面の "Effect" に設定
        var sr = effect.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Effect";
            sr.sortingOrder = 100; //念のためオーダーも高くする
        }

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

    /// <summary>
    /// 現在のターン担当者を設定する（演出リセット後の戻り先になる）
    /// </summary>
    public void SetAmbientFocus(BattleUnit unit)
    {
        _ambientFocusActor = unit;
        RefreshTransparency();
    }

    /// <summary>
    /// 指定したユニット（行動者やターゲット）以外を半透明にする
    /// </summary>
    public void SetOtherUnitsTransparency(BattleUnit attacker, List<BattleUnit> targets, float alpha)
    {
        _currentFocusActor = attacker;
        _currentFocusGroup = targets;
        _currentDimAlpha = alpha;
        
        RefreshTransparency();
    }

    /// <summary>
    /// ターゲット選択時に、行動者と「選択候補」以外を半透明にする
    /// </summary>
    public void SetTargetCandidatesTransparency(BattleUnit attacker, List<BattleUnit> candidates, float unselectedAlpha)
    {
        SetOtherUnitsTransparency(attacker, candidates, unselectedAlpha);
    }

    /// <summary>
    /// 現在の戦況（フォーカス状態や生存状態）から全ユニットの透明度を一括更新する
    /// </summary>
    public void RefreshTransparency()
    {
        if (_unitManager == null || _unitManager.AllUnits == null) return;

        foreach (var unit in _unitManager.AllUnits)
        {
            if (unit == null || unit.IsFadingOut || unit.Data == null) continue;

            float targetAlpha = 1.0f;
            
            // 決定優先順位：
            // 1. 現在の演出フォーカス（_currentFocusActor/Group）
            // 2. なければターン担当（_ambientFocusActor）
            
            bool isCore = false;
            float dim = 1.0f;

            if (_currentFocusActor != null || (_currentFocusGroup != null && _currentFocusGroup.Count > 0))
            {
                // 演出中
                isCore = (unit == _currentFocusActor) || (_currentFocusGroup != null && _currentFocusGroup.Contains(unit));
                dim = _currentDimAlpha;
            }
            else if (_ambientFocusActor != null)
            {
                // 通常時（ターン担当強調）
                isCore = (unit == _ambientFocusActor);
                dim = 0.3f; // ターン中デフォルトの暗さ
            }

            if (!isCore)
            {
                targetAlpha = dim;
                // 死亡ユニットはさらに薄く
                if (unit.Data.currentHp <= 0) targetAlpha *= 0.5f;
            }

            unit.SetAlpha(targetAlpha);
        }
    }

    /// <summary>
    /// 演出後に透明度を完全にリセットする
    /// </summary>
    public void ResetAllTransparency()
    {
        _currentFocusActor = null;
        _currentFocusGroup = null;
        _currentDimAlpha = 1.0f;
        RefreshTransparency();
    }
}
