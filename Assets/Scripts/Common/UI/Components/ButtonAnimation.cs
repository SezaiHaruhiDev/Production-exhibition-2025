using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

namespace Common.UI
{
    /// <summary>
    /// ボタン押下時に縮小アニメーションを適用
    /// </summary>
    public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float speed = 10f;

        private Vector3 _normalScale;
        private bool _isPressed = false;

        private void Awake()
        {
            if (target == null) target = GetComponent<RectTransform>();
            Assert.IsNotNull(target, "ButtonAnimation: Target RectTransform is not assigned and could not be found.");
            _normalScale = target.localScale;
        }

        private void Start()
        {
            var myImage = GetComponent<Image>();
            if (myImage != null)
            {
                try
                {
                    myImage.alphaHitTestMinimumThreshold = 0.1f;
                }
                catch (System.InvalidOperationException)
                {
                    // 無視: 設定できない画像の場合は透明度判定を行わない
                }
            }
        }

        private void Update()
        {
            Vector3 targetScale = _isPressed ? _normalScale * pressedScale : _normalScale;
            target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.deltaTime * speed);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }
    }
}
