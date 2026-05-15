using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 画面端を暗くするビネットフィルターの制御クラス
/// </summary>
[RequireComponent(typeof(Image))]
public class VignetteController : MonoBehaviour
{
    private Image _image;
    private Material _material;

    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int PowerID = Shader.PropertyToID("_Power");

    [Header("Default Settings")]
    [SerializeField] private float defaultIntensity = 15f;
    [SerializeField] private float defaultPower = 0.5f;

    private void Awake()
    {
        _image = GetComponent<Image>();
        // マテリアルのインスタンスを作成して個別に制御できるようにする
        if (_image.material != null)
        {
            _image.material = new Material(_image.material);
            _material = _image.material;
        }

        // 初期状態を設定
        SetVignette(defaultIntensity, defaultPower);
    }

    public void SetVignette(float intensity, float power)
    {
        if (_material == null) return;
        _material.SetFloat(IntensityID, intensity);
        _material.SetFloat(PowerID, power);
    }

    /// <summary>
    /// アルティメット発動時などの強調演出
    /// </summary>
    public void AnimateVignette(float targetIntensity, float targetPower, float duration)
    {
        if (_material == null) return;

        DOTween.To(() => _material.GetFloat(IntensityID), x => _material.SetFloat(IntensityID, x), targetIntensity, duration);
        DOTween.To(() => _material.GetFloat(PowerID), x => _material.SetFloat(PowerID, x), targetPower, duration);
    }

    /// <summary>
    /// デフォルトの状態に戻す
    /// </summary>
    public void ResetVignette(float duration)
    {
        AnimateVignette(defaultIntensity, defaultPower, duration);
    }
}
