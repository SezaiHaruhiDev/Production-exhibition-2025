using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 戦闘中の演出（カメラワーク、カットイン等）を管理するクラス
/// </summary>
public class BattlePresentationManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera _battleCamera;
    [SerializeField] private float _defaultFieldOfView = 60f;
    [SerializeField] private float _zoomInFieldOfView = 40f;
    [SerializeField] private float _zoomDistance = 4f;       // 手前への距離（小さいほど近い）
    [SerializeField] private float _zoomHeightOffset = -1.5f; // 高さのオフセット（小さいほど低い）
    [SerializeField] private float _zoomSideOffset = 2f;      // 左右へのオフセット（大きくするとより外側に振れる）
    
    [Header("Focus Points (Optional)")]
    [SerializeField] private Transform _enemyFocusPoint;
    [SerializeField] private Transform _allyFocusPoint;
    
    [Header("References")]
    [SerializeField] private BattleUIManager _uiManager;
    [SerializeField] private UnitManager _unitManager;

    private Vector3 _originalCameraPosition;
    private Quaternion _originalCameraRotation;

    private void Awake()
    {
        if (_battleCamera == null) _battleCamera = Camera.main;
        if (_uiManager == null) _uiManager = FindFirstObjectByType<BattleUIManager>();
        if (_unitManager == null) _unitManager = FindFirstObjectByType<UnitManager>();

        if (_battleCamera != null)
        {
            _originalCameraPosition = _battleCamera.transform.position;
            _originalCameraRotation = _battleCamera.transform.rotation;
        }
    }

    /// <summary>
    /// 戦闘開始時のカメラ演出を実行する
    /// </summary>
    public IEnumerator PlayBattleStartSequence()
    {
        if (_battleCamera == null) yield break;

        // UIを非表示にする
        _uiManager?.SetUIVisibility(false);

        // 1. 敵側にズーム
        var enemies = _unitManager.AllUnits.Where(u => !u.Data.isAlly).ToList();
        if (enemies.Count > 0 || _enemyFocusPoint != null)
        {
            Vector3 targetPos = GetTargetPosition(_enemyFocusPoint, enemies);
            yield return StartCoroutine(CameraFocusRoutine(targetPos, _zoomInFieldOfView, 1.5f));
            yield return new WaitForSeconds(0.5f);
        }

        // 2. 味方側にスライドしてズーム
        var allies = _unitManager.AllUnits.Where(u => u.Data.isAlly).ToList();
        if (allies.Count > 0 || _allyFocusPoint != null)
        {
            Vector3 targetPos = GetTargetPosition(_allyFocusPoint, allies);
            yield return StartCoroutine(CameraFocusRoutine(targetPos, _zoomInFieldOfView, 1.5f));
            yield return new WaitForSeconds(0.5f);
        }

        // 3. 元の位置に戻す
        yield return StartCoroutine(ResetCameraRoutine(1.0f));

        // UIを表示する
        _uiManager?.SetUIVisibility(true);
    }

    private Vector3 GetTargetPosition(Transform focusPoint, List<BattleUnit> units)
    {
        if (focusPoint != null) return focusPoint.position;
        return GetUnitsCenter(units);
    }

    private Vector3 GetUnitsCenter(List<BattleUnit> units)
    {
        if (units == null || units.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (var u in units) sum += u.transform.position;
        return sum / units.Count;
    }

    private IEnumerator CameraFocusRoutine(Vector3 targetWorldPos, float targetFOV, float duration)
    {
        float elapsed = 0;
        Vector3 startPos = _battleCamera.transform.position;
        float startFOV = _battleCamera.fieldOfView;
        
        // ターゲットの正面（少し手前）にカメラを移動させる
        Vector3 directionFromCenter = (_originalCameraPosition - GetUnitsCenter(_unitManager.AllUnits)).normalized;
        Vector3 cameraRight = Vector3.Cross(Vector3.up, -directionFromCenter).normalized;
        
        // ターゲットが中心に対して右側なら右へ、左側なら左へオフセットをかける
        float battleCenterX = GetUnitsCenter(_unitManager.AllUnits).x;
        float sideSign = targetWorldPos.x > battleCenterX ? 1f : -1f;

        Vector3 targetPos = targetWorldPos + directionFromCenter * _zoomDistance + cameraRight * _zoomSideOffset * sideSign;
        targetPos.y = _originalCameraPosition.y + _zoomHeightOffset; // 少し低くする

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            
            _battleCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            _battleCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            
            // ターゲットの方を向く（オプション）
            // _battleCamera.transform.LookAt(targetWorldPos);
            
            yield return null;
        }
    }

    private IEnumerator ResetCameraRoutine(float duration)
    {
        float elapsed = 0;
        Vector3 startPos = _battleCamera.transform.position;
        float startFOV = _battleCamera.fieldOfView;
        Quaternion startRot = _battleCamera.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            
            _battleCamera.transform.position = Vector3.Lerp(startPos, _originalCameraPosition, t);
            _battleCamera.transform.rotation = Quaternion.Lerp(startRot, _originalCameraRotation, t);
            _battleCamera.fieldOfView = Mathf.Lerp(startFOV, _defaultFieldOfView, t);
            
            yield return null;
        }
    }
}
