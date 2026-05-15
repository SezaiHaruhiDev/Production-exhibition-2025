using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// アルティメットゲージの演出・表示を管理するクラス
/// </summary>
public class UltimateGaugeController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image gaugeFillImage;
    [SerializeField] private Image gaugeGlowImage;
    [SerializeField] private CanvasGroup gaugeGroup;
    [SerializeField] private GameObject readyIndicator; // アルティメット準備完了時に表示する画像オブジェクト

    [Header("Settings")]
    [SerializeField] private float fillDuration = 0.4f;
    [SerializeField] private Ease fillEase = Ease.OutCubic;

    [Header("Wave Visuals")]
    [SerializeField] private float normalWaveAmp = 0.02f;
    [SerializeField] private float fullWaveAmp = 0.04f;
    [SerializeField] private float normalGlow = 1.0f;
    [SerializeField] private float fullGlow = 2.0f;
    [SerializeField] private float waveFrequency = 3.0f;

    [Header("Debug")]
    [SerializeField, Range(0, 100)] private int debugValue;

    private Material _gaugeMaterial;
    private int _currentValue = -1;
    private int _maxValue = 100;
    private Tweener _fillTweener;
    private Sequence _pulseSequence;

    private static readonly int PropFillAmount = Shader.PropertyToID("_FillAmount");
    private static readonly int PropWaveAmp = Shader.PropertyToID("_WaveAmp");
    private static readonly int PropWaveFreq = Shader.PropertyToID("_WaveFreq");
    private static readonly int PropGlowPower = Shader.PropertyToID("_GlowPower");

    private void Awake()
    {
        if (gaugeFillImage != null)
        {
            _gaugeMaterial = Instantiate(gaugeFillImage.material);
            gaugeFillImage.material = _gaugeMaterial;
            gaugeFillImage.fillAmount = 1f;
        }
    }

    private void OnDestroy()
    {
        if (_gaugeMaterial != null) Destroy(_gaugeMaterial);
        _fillTweener?.Kill();
        _pulseSequence?.Kill();
    }

    /// <summary>
    /// アルティメット値を設定（初期化・リセット用）
    /// </summary>
    public void SetUltimate(int value, int max = 100)
    {
        // 変化量や通知なしで即時セット
        _currentValue = Mathf.Clamp(value, 0, max);
        _maxValue = max;

        float ratio = (float)_currentValue / _maxValue;
        UpdateVisuals(ratio);
    }

    /// <summary>
    /// アルティメット値を更新（演出あり）
    /// </summary>
    public void UpdateView(float current, float max)
    {
        // 変更がある場合のみTween
        int nextValue = Mathf.FloorToInt(current);
        int maxVal = Mathf.FloorToInt(max);

        if (_currentValue == nextValue && _maxValue == maxVal) return;

        bool wasFull = IsFull();
        _currentValue = nextValue;
        _maxValue = maxVal;
        bool isFull = IsFull();

        float targetRatio = (float)_currentValue / _maxValue;

        // Tween Fill
        _fillTweener?.Kill();

        // Custom Tween for Shader Property
        _fillTweener = DOVirtual.Float(
            GetShaderFillAmount(),
            targetRatio,
            fillDuration,
            (val) => SetShaderFillAmount(val)
        ).SetEase(fillEase);

        // State Check
        if (isFull && !wasFull)
        {
            PlayFullEffect();
        }
        else if (!isFull && wasFull)
        {
            StopFullEffect();
        }
    }

    /// <summary>
    /// アルティメット発動時の演出
    /// </summary>
    public void PlayUseAnimation()
    {
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);
        UpdateView(0, _maxValue);
    }

    private bool IsFull() => _currentValue >= _maxValue;

    private void UpdateVisuals(float ratio)
    {
        SetShaderFillAmount(ratio);
        SetShaderWaveFreq(waveFrequency);

        if (IsFull()) PlayFullEffect();
        else StopFullEffect();
    }

    private void SetShaderFillAmount(float val)
    {
        if (_gaugeMaterial != null)
        {
            _gaugeMaterial.SetFloat(PropFillAmount, val);
        }
    }

    private void SetShaderWaveFreq(float val)
    {
        if (_gaugeMaterial != null && _gaugeMaterial.HasProperty(PropWaveFreq))
        {
            _gaugeMaterial.SetFloat(PropWaveFreq, val);
        }
    }

    private float GetShaderFillAmount()
    {
        if (_gaugeMaterial != null) return _gaugeMaterial.GetFloat(PropFillAmount);
        return 0f;
    }

    private void PlayFullEffect()
    {
        if (_pulseSequence != null && _pulseSequence.IsActive()) return;
        if (_gaugeMaterial != null)
        {
            _gaugeMaterial.DOFloat(fullWaveAmp, PropWaveAmp, 0.5f);

            // Loop Glow Pulse
            _pulseSequence = DOTween.Sequence();
            _pulseSequence.Append(_gaugeMaterial.DOFloat(fullGlow * 1.5f, PropGlowPower, 0.8f).SetEase(Ease.InOutSine));
            _pulseSequence.Append(_gaugeMaterial.DOFloat(fullGlow, PropGlowPower, 0.8f).SetEase(Ease.InOutSine));
            _pulseSequence.SetLoops(-1);
        }

        // ゲージ最大時のパーティクル散布（UIEffectSpawner）
        UIEffectSpawner.Instance?.Play();

        // 準備完了インジケーターON
        if (readyIndicator != null) readyIndicator.SetActive(true);
    }

    private void StopFullEffect()
    {
        _pulseSequence?.Kill();
        _pulseSequence = null;

        transform.DOKill();
        transform.localScale = Vector3.one;

        if (_gaugeMaterial != null)
        {
            _gaugeMaterial.DOFloat(normalWaveAmp, PropWaveAmp, 0.5f);
            _gaugeMaterial.DOFloat(normalGlow, PropGlowPower, 0.5f);
        }

        // 準備完了インジケーターOFF
        if (readyIndicator != null) readyIndicator.SetActive(false);
    }
    private void Update()
    {
        // 実行開始直後の debugValue (0) による上書きを防ぎ、
        // インスペクターで値が動かされたときだけ反応させる
        if (Application.isPlaying && debugValue != _lastDebugValue)
        {
            _lastDebugValue = debugValue;
            UpdateView(debugValue, 100);
        }
    }
    private int _lastDebugValue = -1;
    private void OnValidate()
    {
        // ここでは何もしない、または DOTween を含まない即時更新のみを行う
    }
}
