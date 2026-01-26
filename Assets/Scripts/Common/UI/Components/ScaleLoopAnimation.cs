using UnityEngine;

namespace Common.UI
{
    /// <summary>
    /// UIオブジェクトを拡大縮小でループアニメーション
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScaleLoopAnimation : MonoBehaviour
    {
        [Header("Scale Settings")]
        [SerializeField] private Vector3 minScale = new Vector3(0.95f, 0.95f, 1f);
        [SerializeField] private Vector3 maxScale = new Vector3(1.05f, 1.05f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float speed = 3.0f;
        [SerializeField] private bool useUnscaledTime = false;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            float time = useUnscaledTime ? Time.unscaledTime : Time.time;

            // Sine波(-1~1)を0~1の範囲に正規化してLerpに渡す
            float t = (Mathf.Sin(time * speed) + 1.0f) / 2.0f;

            _rectTransform.localScale = Vector3.Lerp(minScale, maxScale, t);
        }
    }
}
