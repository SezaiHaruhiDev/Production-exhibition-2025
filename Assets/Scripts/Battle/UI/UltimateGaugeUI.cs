using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 共有アルティメットゲージの表示管理
/// </summary>
public class UltimateGaugeUI : MonoBehaviour
{
    [SerializeField] private Image fillImage; 
    [SerializeField] private TextMeshProUGUI gaugeText;
    [SerializeField] private float smoothSpeed = 5f; // 滑らかさの速度

    private float _targetRatio = 0f;
    private float _currentDisplayRatio = 0f;

    /// <summary>
    /// 表示を更新する（外部からはこれを呼ぶだけ）
    /// </summary>
    public void UpdateView(float current, float max)
    {
        _targetRatio = Mathf.Clamp01(current / max);
    }

    private void Update()
    {
        // 現在の表示用数値をターゲットに近づける（線形補間）
        if (Mathf.Abs(_currentDisplayRatio - _targetRatio) > 0.001f)
        {
            _currentDisplayRatio = Mathf.Lerp(_currentDisplayRatio, _targetRatio, Time.unscaledDeltaTime * smoothSpeed);
            ApplyView(_currentDisplayRatio);
        }
        else
        {
            _currentDisplayRatio = _targetRatio;
            ApplyView(_currentDisplayRatio);
        }
    }

    private void ApplyView(float ratio)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = ratio;
        }

        if (gaugeText != null)
        {
            float percentage = ratio * 100f;
            gaugeText.text = $"{Mathf.FloorToInt(percentage)}%";
        }
    }
}
