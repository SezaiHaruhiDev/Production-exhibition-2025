using UnityEngine;

/// <summary>
/// RectTransformの高さを別のRectTransformに同期
/// </summary>
public class HeightSync : MonoBehaviour
{
    [SerializeField] private RectTransform source;
    [SerializeField] private RectTransform target;

    void Update()
    {
        if (source == null || target == null) return;

        float sourceHeight = source.sizeDelta.y;

        Vector2 size = target.sizeDelta;
        // 無限ループや微細な振動（ジッター）を防ぐため、変化量が閾値を超えた場合のみ更新
        if (Mathf.Abs(size.y - sourceHeight) > 0.01f)
        {
            size.y = sourceHeight;
            target.sizeDelta = size;
        }
    }
}
