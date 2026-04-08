using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 戦闘画面上でキャラクターの見た目（2.5D）とデータを管理する実体クラス
/// </summary>
public class BattleUnit : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _shadowCaster;
    [SerializeField] private UnitHPBarUI _hpBar;
    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private Color _damageColor = Color.red;
    [SerializeField] private Color _healColor = Color.green;
    [SerializeField] private Vector3 _damageTextOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private GameObject _targetMark;
    [SerializeField] private Vector3 _targetMarkOffset = new Vector3(0, 1f, -2.0f);
    [SerializeField] private GameObject _turnIndicator;
    [SerializeField] private Vector3 _turnIndicatorOffset = new Vector3(0, 2f, -1.0f);
    
    [Header("Shadow Settings")]
    [SerializeField] private SpriteRenderer _blobShadow;
    [SerializeField] private float _shadowScale = 1.0f;
    [SerializeField] private float _shadowYOffset = 0.02f;
    [SerializeField] private Color _shadowColor = new Color(0, 0, 0, 0.5f);
    
    private Sprite _originalSprite;

    public UnitCharacter Data { get; private set; }
    public bool IsFadingOut { get; set; } = false;

    /// <summary>
    /// ユニットの初期設定を行い、指定された画像を表示する
    /// </summary>
    public void Setup(UnitCharacter data, Sprite sprite)
    {
        this.Data = data;
        
        if (_spriteRenderer == null) _spriteRenderer = transform.Find("Visual")?.GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (_shadowCaster == null) _shadowCaster = transform.Find("ShadowCaster")?.GetComponent<SpriteRenderer>();
        if (_blobShadow == null) _blobShadow = transform.Find("BlobShadow")?.GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = sprite;
            _originalSprite = sprite;
        }
        else
        {
            Debug.LogError($"[BattleUnit] Setup {data.name}: No SpriteRenderer found!");
        }

        if (_shadowCaster != null)
        {
            _shadowCaster.sprite = sprite;
            _shadowCaster.transform.localPosition = _spriteRenderer.transform.localPosition;
            _shadowCaster.transform.localScale = _spriteRenderer.transform.localScale;
        }

        if (_blobShadow != null)
        {
            _blobShadow.color = _shadowColor;
            _blobShadow.transform.localPosition = new Vector3(0, _shadowYOffset, 0); // 地面から少し浮かせる
            // スプライトの幅に合わせて影の大きさを調整（デフォルトは _shadowScale）
            float baseScale = (sprite != null) ? (sprite.bounds.size.x * 0.5f) : 1.0f;
            _blobShadow.transform.localScale = new Vector3(baseScale * _shadowScale, baseScale * _shadowScale * 0.4f, 1.0f);
        }

        if (_hpBar != null)
        {
            _hpBar.SetSide(data.isAlly);
        }

        RefreshHPBar();
        AdjustToGround(_spriteRenderer);
    }

    /// <summary>
    /// HPバーの表示を最新の状態に更新する
    /// </summary>
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
        
        float currentBottomY = sr.bounds.min.y;
        float baseOriginY = transform.position.y;
        
        // 差分だけ持ち上げる（または下げる）
        float offsetY = baseOriginY - currentBottomY;
        sr.transform.localPosition = new Vector3(0, offsetY, 0);

        // スプライトが動いた分、他の要素の同期を行う
        Physics2D.SyncTransforms();

        // コライダーのサイズと位置を「画像本体のローカル空間」で合わせる
        // これにより、親の回転や自身のスケールに正確に追従するようになります。
        if (_collider is BoxCollider box && sr.sprite != null)
        {
            // sr.sprite.bounds はスプライト自体のローカルサイズ（PPU考慮済み）を返します
            box.size = new Vector3(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y, 0.5f);
            box.center = new Vector3(sr.sprite.bounds.center.x, sr.sprite.bounds.center.y, 0);
        }
    }

   
    /// <summary>
    /// ダメージを表示し、死亡判定を行う
    /// </summary>
    public void ShowDamage(int amount)
    {
        ShowFloatingText(amount, _damageColor);

        if (Data.currentHp <= 0)
        {
            var manager = GetComponentInParent<UnitManager>();
            if (manager == null) manager = FindAnyObjectByType<UnitManager>();
            
            manager?.OnUnitDead(this);
        }
    }

    /// <summary>
    /// 回復量を表示する
    /// </summary>
    public void ShowHeal(int amount)
    {
        ShowFloatingText(amount, _healColor);
    }

    private void ShowFloatingText(int amount, Color color)
    {
        if (_damageTextPrefab == null) return;

        // 指定されたオフセット位置に出現させる
        Vector3 spawnPos = transform.position + _damageTextOffset;
        GameObject go = Instantiate(_damageTextPrefab, spawnPos, Quaternion.identity);
        
        var damageUI = go.GetComponent<DamageTextUI>();
        if (damageUI != null)
        {
            damageUI.Setup(amount, color); 
        }
    }

    /// <summary>
    /// ユニットの戦闘不能（ダウン）状態を切り替える
    /// </summary>
    public void SetDown(bool isDown, Sprite downSprite = null)
    {
        if (_spriteRenderer == null) return;

        if (isDown)
        {
            if (downSprite != null)
            {
                _spriteRenderer.sprite = downSprite;
            }
            else
            {
                _spriteRenderer.color = Color.gray;
            }
        }
        else
        {
            if (_originalSprite != null)
            {
                _spriteRenderer.sprite = _originalSprite;
            }
            _spriteRenderer.color = Color.white;
        }
    }


    public event System.Action<BattleUnit> OnSelected;
    private Collider _collider;
    private bool _isSelectable;

    private void Awake()
    {
        // 先にSpriteRendererを確定させる（これがないと判定を付けられない）
        if (_spriteRenderer == null)
            _spriteRenderer = transform.Find("Visual")?.GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            // 画像と同じオブジェクトにコライダーを持たせることで、
            // ビルボード（回転）やスケール変更に判定が正確に追従するようになります。
            _collider = _spriteRenderer.GetComponent<Collider>();
            if (_collider == null)
            {
                var box = _spriteRenderer.gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(1, 2, 0.5f);
                box.center = new Vector3(0, 1, 0);
                _collider = box;
            }

            // 画像オブジェクトへのクリックをこのBattleUnitに伝えるためのプロキシを追加
            var proxy = _spriteRenderer.gameObject.GetComponent<BattleUnitClickProxy>();
            if (proxy == null) proxy = _spriteRenderer.gameObject.AddComponent<BattleUnitClickProxy>();
            proxy.owner = this;
        }
        else
        {
            // 予備：ルートオブジェクトで判定
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(1, 2, 0.5f);
                box.center = new Vector3(0, 1, 0);
                _collider = box;
            }
        }

        if (_targetMark != null)
        {
            _targetMark.SetActive(false);
        }

        if (_turnIndicator != null)
        {
            _turnIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// ターゲットとして選択可能かどうかを設定する
    /// </summary>
    public void SetSelectable(bool selectable)
    {
        _isSelectable = selectable;
        
        if (_targetMark != null)
        {
            if (selectable)
            {
                _targetMark.transform.localPosition = _targetMarkOffset;
            }
            _targetMark.SetActive(selectable);
        }
    }

    /// <summary>
    /// このユニットのターンであるかどうかを設定する
    /// </summary>
    public void SetTurnActive(bool active)
    {
        if (_turnIndicator != null)
        {
            if (active)
            {
                _turnIndicator.transform.localPosition = _turnIndicatorOffset;
            }
            _turnIndicator.SetActive(active);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isSelectable)
        {
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

            // ターゲットマークもカメラの方を向かせる（アニメーションの回転を活かす）
            if (_targetMark != null && _targetMark.activeSelf)
            {
                // カメラを向く回転に対して、自身のローカルZ回転（アニメーション分）を掛け合わせる
                _targetMark.transform.rotation = rot * Quaternion.Euler(0, 0, _targetMark.transform.localEulerAngles.z);
            }

            // ターンインジケーターもカメラの方を向かせる
            if (_turnIndicator != null && _turnIndicator.activeSelf)
            {
                _turnIndicator.transform.rotation = rot;

                // ふわふわさせるアニメーション (元のオフセット位置 + サイン波)
                float hover = Mathf.Sin(Time.time * 5.0f) * 0.15f;
                _turnIndicator.transform.localPosition = _turnIndicatorOffset + new Vector3(0, hover, 0);
            }
        }
    }

    /// <summary>
    /// スプライトの透明度を設定する（攻撃演出時のフォーカス用）
    /// </summary>
    public void SetAlpha(float alpha)
    {
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = alpha;
            _spriteRenderer.color = c;
        }

        if (_shadowCaster != null)
        {
            Color sc = _shadowCaster.color;
            sc.a = alpha;
            _shadowCaster.color = sc;
        }

        if (_blobShadow != null)
        {
            Color bc = _blobShadow.color;
            bc.a = alpha * 0.5f; // もともとの影の濃さを考慮
            _blobShadow.color = bc;
        }
    }

    /// <summary>
    /// 被弾時のノックバック演出（後ろに下がる・傾く・戻る）
    /// </summary>
    public void PlayKnockback(float intensity = 1.0f)
    {
        // ビルボード処理と競合しないよう、ローカル座標をベースに演出する
        float direction = Data.isAlly ? -1f : 1f; // 味方なら左、敵なら右へ
        float moveDist = 0.3f * intensity * direction; // directionの向きを考慮
        float rotAngle = 10f * intensity * direction;
        float duration = 0.1f;

        Sequence seq = DOTween.Sequence();
        
        // 1. 下がる & 傾く
        seq.Append(_spriteRenderer.transform.DOLocalMoveX(moveDist, duration).SetRelative());
        seq.Join(_spriteRenderer.transform.DOLocalRotate(new Vector3(0, 0, rotAngle), duration).SetRelative());
        
        // 2. 素早く戻る
        seq.Append(_spriteRenderer.transform.DOLocalMoveX(0, duration * 2f));
        seq.Join(_spriteRenderer.transform.DOLocalRotate(Vector3.zero, duration * 2f));
    }

    /// <summary>
    /// 行動開始時の「踏み込み」演出
    /// </summary>
    public void PlayStepAction(float duration = 0.15f)
    {
        float direction = Data.isAlly ? 1f : -1f; 
        float moveDist = 0.8f * direction; // より深く踏み込む

        Sequence seq = DOTween.Sequence();
        // 踏み込む
        seq.Append(_spriteRenderer.transform.DOLocalMoveX(moveDist, duration).SetEase(Ease.OutCubic));
        // 維持時間を短縮し、素早く戻る
        seq.AppendInterval(0.1f);
        seq.Append(_spriteRenderer.transform.DOLocalMoveX(0, duration).SetEase(Ease.InExpo)); // 戻りはさらに鋭く
    }

    /// <summary>
    /// 回復・補助時の「ジャンプ」演出
    /// </summary>
    public void PlayJumpAction(float duration = 0.2f)
    {
        float jumpHeight = 0.5f;
        Sequence seq = DOTween.Sequence();
        // 上に跳ねる
        seq.Append(_spriteRenderer.transform.DOLocalMoveY(jumpHeight, duration).SetEase(Ease.OutCubic).SetRelative());
        // 戻る
        seq.Append(_spriteRenderer.transform.DOLocalMoveY(-jumpHeight, duration).SetEase(Ease.InCubic).SetRelative());
        // 着地で少し潰れる（任意だが一旦シンプルに）
    }
}

/// <summary>
/// 子オブジェクト（画像）へのクリックを親のBattleUnitに伝えるためのプロキシ
/// </summary>
public class BattleUnitClickProxy : MonoBehaviour, IPointerDownHandler
{
    public BattleUnit owner;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (owner != null) owner.OnPointerDown(eventData);
    }
}
