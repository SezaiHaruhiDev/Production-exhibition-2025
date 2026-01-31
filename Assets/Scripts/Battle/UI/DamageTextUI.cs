using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// ダメージ数字のポップアップ演出を制御するクラス
/// </summary>
public class DamageTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float moveDistance = 1.5f;
    [SerializeField] private float duration = 0.8f;

    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 1000; // 最前面に持ってくる
        }
    }

    private void LateUpdate()
    {
        // 数字もカメラの方を向かせる
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void Setup(int amount, Color color)
    {
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        textMesh.text = amount.ToString();
        textMesh.color = color;

        // 演出開始
        Sequence sequence = DOTween.Sequence();
        
        // 1. 少し上に跳ね上がりながら上に移動
        transform.localPosition += Vector3.down * 0.5f; // 少し下から開始
        sequence.Join(transform.DOLocalMoveY(transform.localPosition.y + moveDistance, duration).SetEase(Ease.OutQuart));
        
        // 2. フェードアウト
        sequence.Join(textMesh.DOFade(0, duration).SetEase(Ease.InExpo));

        // 3. 終わったら破棄
        sequence.OnComplete(() => Destroy(gameObject));
    }
}
