using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Common.UI
{
    /// <summary>
    /// ボタン押下時に縮小アニメーションを適用
    /// ボタンの当たり判定を画像と統一
    /// </summary>
    public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float speed = 10f;

        private Vector3 normalScale;
        private bool isPressed = false;

        private void Awake()
        {
            if (target == null) target = GetComponent<RectTransform>();

            normalScale = target.localScale;
        }

        private void Start()
        {
            Image myImage = GetComponent<Image>();
            if (myImage != null)
            {
                try
                {
                    myImage.alphaHitTestMinimumThreshold = 0.1f;
                }
                catch (System.InvalidOperationException){}
            }
        }

        private void Update()
        {
            Vector3 targetScale = isPressed ? normalScale * pressedScale : normalScale;
            // Lerpを用いて滑らかにサイズを変更する（Smoothing）
            target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.deltaTime * speed);
        }

        /// <summary>
        /// ボタン押下時処理（縮小開始）
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
        }

        /// <summary>
        /// ボタン解放時処理（元のサイズに戻す）
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
        }
    }
}
