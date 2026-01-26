using UnityEngine;

/// <summary>
/// 「次へ」アイコンを上下にふわふわ動かすアニメーション
/// </summary>
public class NextIcon : MonoBehaviour
{
    [SerializeField] private float moveDistance = 10f;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Sine波を使って上下に浮遊させる（-moveDistance ~ +moveDistance の範囲）
        float newY = startPos.y + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}
