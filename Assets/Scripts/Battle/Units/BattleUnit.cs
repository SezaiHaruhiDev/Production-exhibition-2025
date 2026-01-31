using UnityEngine;

/// <summary>
/// 戦闘画面上でキャラクターの見た目（2.5D）とデータを管理する実体クラス
/// </summary>
public class BattleUnit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _shadowCaster; // 影を落とす専用のスプライト
    [SerializeField] private UnitHPBarUI _hpBar;
    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private Color _damageColor = Color.red;
    [SerializeField] private Vector3 _damageTextOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private GameObject _targetMark;
    [SerializeField] private Vector3 _targetMarkOffset = new Vector3(0, 1f, -2.0f);

    public UnitCharacter Data { get; private set; }

    /// <summary>
    /// ユニットの初期設定を行い、指定された画像を表示する
    /// </summary>
    public void Setup(UnitCharacter data, Sprite sprite)
    {
        this.Data = data;
        

        Debug.Log($"[BattleUnit] Setup: {data.name} (Sprite: {(sprite != null ? sprite.name : "NULL")})", gameObject);

        // 各SpriteRendererの自動取得とセット
        if (_spriteRenderer == null) _spriteRenderer = transform.Find("Visual")?.GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (_shadowCaster == null) _shadowCaster = transform.Find("ShadowCaster")?.GetComponent<SpriteRenderer>();

        // 本体のセットアップ
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = sprite;
            AdjustToGround(_spriteRenderer);
        }

        // 影落とし用のセットアップ
        if (_shadowCaster != null)
        {
            _shadowCaster.sprite = sprite;
            // 本体と同じ位置に合わせる
            _shadowCaster.transform.localPosition = _spriteRenderer.transform.localPosition;
            _shadowCaster.transform.localScale = _spriteRenderer.transform.localScale;
        }

        RefreshHPBar();
    }

    public void RefreshHPBar()
    {
        if (_hpBar != null && Data != null)
        {
            _hpBar.UpdateHP(Data.currentHp, Data.maxHp);
        }
    }

    /// <summary>
    /// 画像の底辺を基準位置に合わせる（体が半分の画像などを足元に接地させる）
    /// </summary>
    private void AdjustToGround(SpriteRenderer sr)
    {
        sr.transform.localPosition = Vector3.zero;
        Physics2D.SyncTransforms(); 
        
        float currentBottomY = sr.bounds.min.y;
        float baseOriginY = transform.position.y;
        
        // 差分だけ持ち上げる（または下げる）
        float offsetY = baseOriginY - currentBottomY;
        sr.transform.localPosition = new Vector3(0, offsetY, 0);

        // スプライトが動いた分、ターゲットマークとダメージ表示位置も補正する
        // Boundsも移動しているので、最新のBounds中心を使うのが確実
        Physics2D.SyncTransforms();
        Bounds b = sr.bounds;

        // ターゲットマーク：画像の中心
        // ローカル座標系でのオフセットを計算（ワールド座標 - Unitの原点）
        float centerX = b.center.x - transform.position.x;
        float centerY = b.center.y - transform.position.y;
        
        // Zは手前にガッツリ出す（-2.0f）
        _targetMarkOffset = new Vector3(centerX, centerY, -2.0f);

        // ダメージテキスト：画像の頭上
        float topY = b.max.y - transform.position.y;
        _damageTextOffset = new Vector3(centerX, topY + 0.5f, -1.0f);

        // コライダーのサイズと位置も画像に合わせる（クリック判定用）
        if (_collider is BoxCollider box)
        {
            // サイズは見た目通り（Zはクリックしやすいように少し厚みを持たせる）
            box.size = new Vector3(b.size.x, b.size.y, 0.5f);
            // 中心位置（ローカル座標）
            box.center = new Vector3(centerX, centerY, 0);
        }
    }

    public void ShowDamage(int amount)
    {
        if (_damageTextPrefab == null) return;

        // 指定されたオフセット位置に出現させる
        Vector3 spawnPos = transform.position + _damageTextOffset;
        GameObject go = Instantiate(_damageTextPrefab, spawnPos, Quaternion.identity);
        
        var damageUI = go.GetComponent<DamageTextUI>();
        if (damageUI != null)
        {
            damageUI.Setup(amount, _damageColor); 
        }
    }


    public event System.Action<BattleUnit> OnSelected;
    private Collider _collider;
    private bool _isSelectable;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            // 簡易的にBoxColliderを付与（本来はプレハブで設定するのが望ましい）
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1, 2, 0.1f);
            box.center = new Vector3(0, 1, 0);
            _collider = box;
        }

        if (_targetMark != null)
        {
            _targetMark.SetActive(false);
        }
    }

    public void SetSelectable(bool selectable)
    {
        _isSelectable = selectable;
        
        // 色を変えるのではなく、ターゲットマークの表示/非表示を切り替える
        if (_targetMark != null)
        {
            if (selectable)
            {
                _targetMark.transform.localPosition = _targetMarkOffset;
            }
            _targetMark.SetActive(selectable);
        }
    }

    private void OnMouseDown()
    {
        if (_isSelectable)
        {
            Debug.Log($"[BattleUnit] Clicked: {Data.name}");
            OnSelected?.Invoke(this);
        }
    }

    private void LateUpdate()
    {
        if (_spriteRenderer != null && Camera.main != null)
        {
            Vector3 targetRotation = Camera.main.transform.eulerAngles;
            Quaternion rot = Quaternion.Euler(0, targetRotation.y, 0);
            
            _spriteRenderer.transform.rotation = rot;
            
            // 影落とし用も同じ方向を向かせる（影の形を合わせるため）
            if (_shadowCaster != null)
            {
                _shadowCaster.transform.rotation = rot;
            }

            // ターゲットマークもカメラの方を向かせる
            if (_targetMark != null && _targetMark.activeSelf)
            {
                _targetMark.transform.rotation = rot;
            }
        }
    }
}
