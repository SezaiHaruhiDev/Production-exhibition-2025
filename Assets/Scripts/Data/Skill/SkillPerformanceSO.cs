using UnityEngine;

/// <summary>
/// スキルごとの演出パラメータ（カメラ、振動、エフェクト等）を定義するデータ
/// Limbus Company風の演出を微調整するために使用
/// </summary>
[CreateAssetMenu(menuName = "Game/Skill Performance Data")]
public class SkillPerformanceSO : ScriptableObject
{
    [Header("Camera Focus Settings")]
    [Tooltip("攻撃者のフォーカス位置（足元からの高さ）")]
    public float attackerFocusHeight = 1.7f;
    [Tooltip("ターゲットのフォーカス位置（足元からの高さ）")]
    public float targetFocusHeight = 1.5f;
    
    [Header("Zoom Settings")]
    [Tooltip("攻撃前のクローズアップ時の距離")]
    public float anticipationZoomDistance = 2.5f;
    [Tooltip("攻撃前クローズアップの高さオフセット")]
    public float anticipationHeightOffset = 0.2f;
    
    [Header("Timing")]
    [Tooltip("攻撃前のタメ（ズームイン）にかかる時間")]
    public float anticipationDuration = 0.4f;
    [Tooltip("ターゲットへ移動する時間")]
    public float actionMovementDuration = 0.15f;
    [Tooltip("攻撃ヒット後の静止時間")]
    public float hitStopDuration = 0.15f;
    [Tooltip("攻撃後の余韻（カメラが戻るまでの待機時間）")]
    public float recoveryDelay = 0.5f;

    [Header("Screen Shake")]
    [Tooltip("ヒット時の画面振動ベクトル（Cinemachine Impulse）")]
    public Vector3 shakeImpulse = new Vector3(1.0f, 1.0f, 0f);
    
    [Header("VFX")]
    [Tooltip("攻撃ヒット時にターゲット位置で再生するエフェクト")]
    public GameObject hitEffectPrefab;
    [Tooltip("攻撃開始時に攻撃者位置で再生するエフェクト")]
    public GameObject startEffectPrefab;
    [Tooltip("エフェクトのサイズ倍率")]
    public float effectScale = 1.0f;
}
