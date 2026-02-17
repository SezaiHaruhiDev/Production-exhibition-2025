using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ドラッグ可能な感情カードUI（DCG風のアニメーション対応）
/// </summary>
public class BattleEmotionCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI emotionNameText;
    [SerializeField] private Image cardIcon;
    [SerializeField] private Image frameImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float smoothSpeed = 12f;
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float hoverYOffset = 100f;
    [SerializeField] private int hoverSortOrderOffset = 100;
    [SerializeField] private AudioClip cardDrawSE;

    public EmotionCardData Data { get; private set; }
    public bool IsConsumed => _isConsumed;
    
    private Canvas _canvas;
    private GraphicRaycaster _raycaster;
    private Vector3 _layoutPosition;
    private Quaternion _layoutRotation;
    private int _baseSortOrder = 0;
    private bool _isHovered = false;
    private bool _isDragging = false;
    private bool _isConsumed = false;
    private bool _isAnimating = false;

    private Vector3 _startDragPosition;

    public void MarkConsumed() => _isConsumed = true;
    public void UnmarkConsumed() => _isConsumed = false;

    private void Awake()
    {
        // Canvasコンポーネントの準備（なければ追加）
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            _canvas = gameObject.AddComponent<Canvas>();
        }
        _canvas.overrideSorting = true;

        _raycaster = GetComponent<GraphicRaycaster>();
        if (_raycaster == null)
        {
            _raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    public void Setup(EmotionCardData data)
    {
        Data = data;
        if (emotionNameText != null)
        {
            emotionNameText.text = ""; // 名前を表示しないようにクリア
        }

        if (cardIcon != null && data.cardSprite != null)
        {
            cardIcon.sprite = data.cardSprite;
        }


        UpdateVisualState();
    }

    /// <summary>
    /// ドローアニメーション（山札→中央で見せつけ→手札）の開始
    /// </summary>
    public void PlayDrawAnimation(Vector3 startWorldPos, Vector3 centerWorldPos)
    {
        StartCoroutine(DrawAnimationCoroutine(startWorldPos, centerWorldPos));
    }

    private System.Collections.IEnumerator DrawAnimationCoroutine(Vector3 startWorldPos, Vector3 centerWorldPos)
    {
        _isAnimating = true;
        
        // 開始位置（山札）
        transform.position = startWorldPos;
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 1. 画面中央へ移動しつつ拡大
        float duration = 0.5f;
        float elapsed = 0f;
        
        if (_canvas != null) _canvas.sortingOrder = 999; // 最前面

        // ドロー音再生
        if (cardDrawSE != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(cardDrawSE);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // イージング（少し勢いよく出る）
            float curve = 1f - Mathf.Pow(1f - t, 3); 
            
            transform.position = Vector3.Lerp(transform.position, centerWorldPos, curve);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 2.5f, curve);
            yield return null;
        }

        // 2. 見せつけ休止
        yield return new WaitForSeconds(0.8f);

        // 3. アニメーション終了（あとはUpdateでの補間に任せる）
        _isAnimating = false;
    }

    /// <summary>
    /// LayoutGroupから基本位置とソート順を設定される
    /// </summary>
    public void SetLayoutPosition(Vector3 pos, Quaternion rot, int baseOrder)
    {
        _layoutPosition = pos;
        _layoutRotation = rot;
        _baseSortOrder = baseOrder;
    }

    private void Update()
    {
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (_isDragging || _isConsumed || _isAnimating) return;

        // 目標の状態を計算
        Vector3 targetPos = _layoutPosition;
        Quaternion targetRot = _layoutRotation;
        Vector3 targetScale = Vector3.one;
        int targetSortOrder = _baseSortOrder;

        if (_isHovered)
        {
            targetPos += Vector3.up * hoverYOffset;
            targetRot = Quaternion.identity; // 正面を向く
            targetScale = Vector3.one * hoverScale;
            targetSortOrder += hoverSortOrderOffset;
        }

        // スムーズに補間
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * smoothSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smoothSpeed);
        
        if (_canvas != null)
        {
            _canvas.sortingOrder = targetSortOrder;
        }
    }

    private BattleUIManager _uiManager;

    private void Start()
    {
        _uiManager = Object.FindFirstObjectByType<BattleUIManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (_uiManager != null && Data != null)
        {
            _uiManager.ShowCardDescription(Data);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        if (_uiManager != null)
        {
            _uiManager.HideDescription();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _startDragPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
        if (_uiManager != null) _uiManager.HideDescription();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        canvasGroup.blocksRaycasts = true;

        if (!_isConsumed)
        {
            // ドラッグ終了時はLerpで戻るようにするため、localPositionは弄らない（Updateに任せる）
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedCardUI = eventData.pointerDrag?.GetComponent<BattleEmotionCard>();

        if (droppedCardUI != null && droppedCardUI != this)
        {
            // すでに消費されているカード同士での合成を防ぐ
            if (droppedCardUI.IsConsumed || this.IsConsumed) return;

            var deckManager = Object.FindFirstObjectByType<EmotionDeckManager>();
            if (deckManager != null)
            {
                // 楽観的ロック：合成処理中にUI更新が走った際、これらが「使用済み」とみなされるように先にフラグを立てる
                droppedCardUI.MarkConsumed();
                this.MarkConsumed();

                bool success = deckManager.Synthesize(this.Data, droppedCardUI.Data);
                if (success)
                {
                    // 成功したら物理的に削除
                    droppedCardUI.OnConsumedBySlot(); 
                    this.OnConsumedBySlot();
                }
                else
                {
                    // 失敗したらロック解除
                    droppedCardUI.UnmarkConsumed();
                    this.UnmarkConsumed();
                }
            }
        }
    }

    public void OnConsumedBySlot()
    {
        _isConsumed = true;
        gameObject.SetActive(false); // 即座に非表示にして重なりを防ぐ
        Destroy(gameObject);
    }
}
