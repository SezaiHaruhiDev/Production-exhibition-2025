using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 手札をDCG（デジタルカードゲーム）風に扇状に並べるカスタムレイアウト
/// </summary>
[ExecuteInEditMode]
public class HandLayoutGroup : MonoBehaviour
{
    [SerializeField] private float maxWidth = 800f;
    [SerializeField] private float cardWidth = 140f;
    [SerializeField] private float arcAmount = 20f;
    [SerializeField] private float fanningAmount = 10f;

    private List<RectTransform> _children = new List<RectTransform>();

    private void OnEnable()
    {
        UpdateLayout();
    }

    private void OnTransformChildrenChanged()
    {
        UpdateLayout();
    }

    /// <summary>
    /// レイアウトの更新
    /// </summary>
    public void UpdateLayout()
    {
        _children.Clear();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                var rt = child as RectTransform;
                if (rt != null) _children.Add(rt);
            }
        }

        int count = _children.Count;
        if (count == 0) return;

        // カード間の間隔計算
        // 常に少し重なるように（cardWidth * 0.8f など）基本の間隔を設定
        float baseSpacing = cardWidth * 0.7f;
        float totalNeededWidth = (count - 1) * baseSpacing;
        float actualSpacing = baseSpacing;

        // maxWidthを超える場合のみさらに詰める
        if (totalNeededWidth > maxWidth && count > 1)
        {
            actualSpacing = maxWidth / (count - 1);
        }

        float totalWidth = actualSpacing * (count - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            // t: 0.0 (左端) ～ 1.0 (右端)
            float t = (count > 1) ? (float)i / (count - 1) : 0.5f;

            // X位置
            float x = startX + i * actualSpacing;

            // Y位置 (放物線を描く)
            float yOffset = -4f * Mathf.Pow(t - 0.5f, 2) + 1f;
            float y = yOffset * arcAmount;

            // 回転 (扇状に広げる)
            float rotZ = Mathf.Lerp(fanningAmount, -fanningAmount, t);

            var child = _children[i];
            var card = child.GetComponent<BattleEmotionCard>();

            if (card != null && Application.isPlaying)
            {
                // 再生中はカード側の補間アニメーションに任せる
                // 重なり順（基本は左から順）を考慮した基本のソートオーダーを10から開始するように底上げ
                card.SetLayoutPosition(new Vector3(x, y, 0), Quaternion.Euler(0, 0, rotZ), 10 + i);
            }
            else
            {
                // エディタ上などは直接
                child.localPosition = new Vector3(x, y, 0);
                child.localRotation = Quaternion.Euler(0, 0, rotZ);
            }
        }
    }

    private void Update()
    {
        // エディタ上でのリアルタイム反映用
        if (!Application.isPlaying)
        {
            UpdateLayout();
        }
    }
}
