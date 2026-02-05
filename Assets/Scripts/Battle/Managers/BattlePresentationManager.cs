using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Cinemachine;

/// <summary>
/// 戦闘中の演出（カメラワーク、カットイン等）を管理するクラス
/// Cinemachine移行版
/// </summary>
public class BattlePresentationManager : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineCamera _virtualCamera;
    [SerializeField] private CinemachineImpulseSource _impulseSource;

    [Header("Limbus Effect Settings")]
    [SerializeField] private float _attackerFocusHeight = 1.7f;
    [SerializeField] private float _targetFocusHeight = 1.5f;
    [SerializeField] private float _anticipationZoomDistance = 2.5f;
    [SerializeField] private float _anticipationHeightOffset = 0.2f;
    // [SerializeField] private float _impactZoomDistance = 3.5f; // Removed unused field
    [SerializeField] private float _defaultFOV = 60f;
    [SerializeField] private float _zoomFOV = 35f;

    // 仮想ターゲット（カメラの注視点として動かす用）
    private GameObject _cameraTargetObj;

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
        SetupCinemachine();
    }

    private void SetupCinemachine()
    {
        var mainCam = Camera.main;
        if (mainCam == null) return;

        // 1. Main CameraにBrainがなければ追加
        var brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
            brain.ShowDebugText = false;
            brain.DefaultBlend.Time = 0.5f; 
        }

        // 2. Virtual Cameraがなければ作成
        if (_virtualCamera == null)
        {
            var vcamObj = new GameObject("BattleVirtualCamera");
            // 初期位置をZ-10にしておく（2D/2.5Dの標準的な引き）
            vcamObj.transform.position = new Vector3(0, 0, -10f);
            
            _virtualCamera = vcamObj.AddComponent<CinemachineCamera>();
            _virtualCamera.Priority = 10;
        }

        // 3. カメラのターゲット用オブジェクトを作成
        if (_cameraTargetObj == null)
        {
            _cameraTargetObj = new GameObject("BattleCameraTarget");
        }

        // 初期設定:
        // プログラムで制御するため、Follow/LookAtは一旦nullにしておき、
        // 演出時に必要に応じてセットするか、あるいはスクリプトでtransformを動かす。
        // Cinemachineの基本機能 (Lens, Shake) を使いつつ、位置はスクリプトでMoveToするアプローチをとる。
        _virtualCamera.Follow = null; 
        
        // レンズ設定を初期化
        _virtualCamera.Lens.FieldOfView = _defaultFOV;

        // 4. Impulse Source (Shake用) がなければ追加
        if (_impulseSource == null)
        {
            _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }
        
        // 初期位置へ移動
        ResetCameraPosition();
    }

    private void ResetCameraPosition()
    {
        // 敵味方の中間地点などをデフォルトとする
        Vector3 center = Vector3.zero;
        if (_unitManager != null && _unitManager.AllUnits.Count > 0)
        {
            center = GetUnitsCenter(_unitManager.AllUnits);
        }
        
        // ターゲットオブジェクトを配置
        if (_cameraTargetObj != null)
        {
            _cameraTargetObj.transform.position = center;
        }

        // カメラをターゲットの「手前」に配置
        if (_virtualCamera != null)
        {
            // Zオフセット -10f を維持しつつ、XYはセンターに合わせる
            // 高さ（Y）は少し見下ろす感じにしたいなら調整
            Vector3 camPos = center;
            camPos.z = -10f; 
            camPos.y += 1.0f; // 少し上から

            _virtualCamera.transform.position = camPos;
            _virtualCamera.transform.rotation = Quaternion.identity;
            _virtualCamera.Lens.FieldOfView = _defaultFOV;
        }
    }

    /// <summary>
    /// 戦闘開始時のカメラ演出を実行する
    /// </summary>
    public IEnumerator PlayBattleStartSequence()
    {
        // UIを非表示にする
        _uiManager?.SetUIVisibility(false);

        // とりあえず初期位置へ
        ResetCameraPosition();

        // 1. 敵側にフォーカス
        var enemies = _unitManager.AllUnits.Where(u => !u.Data.isAlly).ToList();
        if (enemies.Count > 0)
        {
            Vector3 enemyCenter = GetUnitsCenter(enemies);
            MoveCameraTarget(enemyCenter, 0.5f);
            yield return new WaitForSeconds(1.0f);
        }

        // 2. 味方側にフォーカス
        var allies = _unitManager.AllUnits.Where(u => u.Data.isAlly).ToList();
        if (allies.Count > 0)
        {
            Vector3 allyCenter = GetUnitsCenter(allies);
            MoveCameraTarget(allyCenter, 0.5f);
            yield return new WaitForSeconds(1.0f);
        }

        // 3. 全体に戻す
        ResetCameraPosition();
        yield return new WaitForSeconds(1.0f);

        // UIを表示する
        _uiManager?.SetUIVisibility(true);
    }
    
    private void MoveCameraTarget(Vector3 position, float duration)
    {
        // CinemachineのDampingが効くので、Transformを瞬時に動かしてもカメラは追従してくる。
        if (_cameraTargetObj != null)
        {
            _cameraTargetObj.transform.position = position;
        }
    }

    /// <summary>
    /// 攻撃アニメーション演出（Limbus Company風 - Cinemachine版）
    /// </summary>
    /// <summary>
    /// 攻撃アニメーション演出（Limbus Company風 - Cinemachine版）
    /// </summary>
    public IEnumerator PlayAttackSequence(BattleUnit attacker, List<BattleUnit> targets, SkillPerformanceSO performanceData, System.Action onImpact)
    {
        Vector3 targetCenter = GetUnitsCenter(targets);
        
        // パラメータの取得（SOがあればそちらを優先、なければデフォルト）
        float attackerHeight = performanceData != null ? performanceData.attackerFocusHeight : _attackerFocusHeight;
        float targetHeight = performanceData != null ? performanceData.targetFocusHeight : _targetFocusHeight;
        float antZoomDist = performanceData != null ? performanceData.anticipationZoomDistance : _anticipationZoomDistance;
        float antHeightOffset = performanceData != null ? performanceData.anticipationHeightOffset : _anticipationHeightOffset;
        float antDuration = performanceData != null ? performanceData.anticipationDuration : 0.4f;
        
        float actionMoveDuration = performanceData != null ? performanceData.actionMovementDuration : 0.15f;
        // float shakeDuration = 0.5f; // Removed unused variable
        Vector3 shakeImpulse = performanceData != null ? performanceData.shakeImpulse : new Vector3(0.5f, 0.5f, 0) * 1.5f;
        
        float hitStopDur = performanceData != null ? performanceData.hitStopDuration : 0.15f;
        float recoveryDelay = performanceData != null ? performanceData.recoveryDelay : 0.5f;
        
        // --- 0. Start Effect ---
        if (performanceData != null && performanceData.startEffectPrefab != null)
        {
            SpawnEffect(performanceData.startEffectPrefab, attacker.transform.position, performanceData.effectScale);
        }

        // --- 1. Anticipation: 攻撃者へズーム ---
        // 攻撃者の顔付近へ。カメラ位置はZ-10を維持
        Vector3 attackerFacePos = attacker.transform.position + new Vector3(0, attackerHeight, 0);
        
        // カメラの目標位置（手前）
        Vector3 cameraPosAnticipation = attackerFacePos + (attackerFacePos - _virtualCamera.transform.position).normalized * 0.1f; // 向き計算用
        cameraPosAnticipation = attackerFacePos; 
        cameraPosAnticipation.y += antHeightOffset; // 少し上げるなど
        cameraPosAnticipation.z = -10f; // 手前へ固定

        // ズームイン & 移動
        float startFOV = _virtualCamera.Lens.FieldOfView;
        Vector3 startPos = _virtualCamera.transform.position;
        
        float elapsed = 0;
        
        while (elapsed < antDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / antDuration;
            
            // 位置移動（滑らかに）
            _virtualCamera.transform.position = Vector3.Lerp(startPos, cameraPosAnticipation, t);
            
            // FOVを絞る（ズーム）
            // 距離ではなくFOVで寄る表現
            _virtualCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, _zoomFOV, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);


        // --- 2. Action: ターゲットへ移動 ---
        Vector3 targetFacePos = targetCenter + new Vector3(0, targetHeight, -10f); // Zはずっと-10
        
        // ターゲットへ瞬間移動
        if (_virtualCamera != null)
        {
            _virtualCamera.transform.position = targetFacePos;
        }

        // FOVは維持
        yield return new WaitForSeconds(actionMoveDuration);


        // --- 3. Impact ---
        onImpact?.Invoke();

        // Hit Effect
        if (performanceData != null && performanceData.hitEffectPrefab != null)
        {
            SpawnEffect(performanceData.hitEffectPrefab, targetCenter, performanceData.effectScale);
        }

        // Screen Shake (Impulse)
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(shakeImpulse);
        }

        yield return StartCoroutine(HitStopRoutine(hitStopDur));
        yield return new WaitForSeconds(recoveryDelay);


        // --- 4. Recovery ---
        // 元のFOVに戻しつつ、全体が見える位置へ
        
        // リセット位置計算
        Vector3 center = GetUnitsCenter(_unitManager.AllUnits);
        Vector3 resetPos = center;
        resetPos.z = -10f;
        resetPos.y += 1.0f;

        elapsed = 0;
        float recoverTime = 0.8f;
        Vector3 recoverStartPos = _virtualCamera.transform.position;
        
        while (elapsed < recoverTime)
        {
            elapsed += Time.deltaTime; // ここはUnscaledではない
            float t = elapsed / recoverTime;
            
            _virtualCamera.transform.position = Vector3.Lerp(recoverStartPos, resetPos, t);
            _virtualCamera.Lens.FieldOfView = Mathf.Lerp(_zoomFOV, _defaultFOV, t);
             yield return null;
        }
    }

    private void SpawnEffect(GameObject prefab, Vector3 position, float scale)
    {
        if (prefab == null) return;
        var effect = Instantiate(prefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
        // ParticleSystemがあれば再生終わったら消すなどの処理が必要だが、
        // DestroyOnLoad なりの自動削除スクリプトがついている前提、あるいは数秒で消す
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

    // 互換性のための残骸メソッド（必要なら空で残すか、削除）
    public void ShakeCamera(float intensity, float duration) {}
}
