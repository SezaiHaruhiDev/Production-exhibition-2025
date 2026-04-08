using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 行動順UIの管理
/// </summary>
public class TurnOrderUI : MonoBehaviour
{
    [SerializeField] private TurnOrderManager turnOrderManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CharacterRegistrySO characterRegistry;
    [SerializeField] private GameObject turnOrderUIItemPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private int predictionCount = 10;
    [SerializeField] private float updateInterval = 0.1f; // 更新間隔（秒）
    
    [SerializeField] private float itemSpacing = 120f;
    [SerializeField] private float startOffsetX = 50f; // 左端からのオフセット
    [SerializeField] private float animationDuration = 0.4f;
    
    private List<TurnOrderUIItem> _items = new List<TurnOrderUIItem>();
    private List<BattleUnit> _lastOrder = new List<BattleUnit>();
    private float _lastUpdateTime;

    private void Awake()
    {
        if (turnOrderManager == null) turnOrderManager = FindAnyObjectByType<TurnOrderManager>();
        if (turnManager == null) turnManager = FindAnyObjectByType<TurnManager>();
        
        // レイアウトを強制的に左合わせにする設定
        RectTransform rt = container as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        var layout = container.GetComponent<UnityEngine.UI.LayoutGroup>();
        if (layout != null) layout.enabled = false;
    }

    private void Start()
    {
        UpdateTurnOrder(true); // 初回は即時配置
        _lastUpdateTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _lastUpdateTime >= updateInterval)
        {
            UpdateTurnOrder(false);
            _lastUpdateTime = Time.time;
        }
    }

    public void UpdateTurnOrder(bool immediate)
    {
        if (turnOrderManager == null || container == null || turnOrderUIItemPrefab == null) return;

        List<BattleUnit> order = turnOrderManager.CalculateTurnOrder(predictionCount);
        
        // 1. 行動者が変わった（先頭が入れ替わった）かチェック
        if (_lastOrder.Count > 0 && order.Count > 0 && _lastOrder[0] != order[0])
        {
            // 旧先頭を退場させるアニメーション
            if (_items.Count > 0)
            {
                var oldTop = _items[0];
                _items.RemoveAt(0);
                oldTop.PlayExitAnimation(animationDuration);
            }
        }

        // 2. アイテムの数を調整
        while (_items.Count < order.Count)
        {
            var go = Instantiate(turnOrderUIItemPrefab, container);
            var item = go.GetComponent<TurnOrderUIItem>();
            if (item != null)
            {
                // 新規追加分は右端の見えない位置から出すなど
                item.transform.localPosition = new Vector3(order.Count * itemSpacing, 0, 0);
                _items.Add(item);
            }
        }

        // 3. 各アイテムを目標の位置へ動かす
        for (int i = 0; i < _items.Count; i++)
        {
            if (i < order.Count)
            {
                _items[i].gameObject.SetActive(true);
                
                BattleUnit unit = order[i];
                Sprite icon = GetIconForUnit(unit);

                // データ更新
                _items[i].Setup(unit, icon, i == 0);

                // 位置計算 (左端から並べる)
                Vector3 targetPos = new Vector3(startOffsetX + i * itemSpacing, 0, 0);

                if (immediate)
                {
                    _items[i].transform.localPosition = targetPos;
                }
                else
                {
                    _items[i].AnimateTo(targetPos, i == 0, animationDuration);
                }
            }
            else
            {
                // 予測数より多い余剰分
                _items[i].gameObject.SetActive(false);
            }
        }

        _lastOrder = new List<BattleUnit>(order);
    }

    private Sprite GetIconForUnit(BattleUnit unit)
    {
        Sprite icon = null;
        if (characterRegistry != null && unit.Data != null)
        {
            var master = characterRegistry.GetById(unit.Data.characterId);
            if (master != null)
            {
                icon = master.characterMiniSprite ?? master.characterBattleSprite;
            }
        }
        
        if (icon == null)
        {
            var sr = unit.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) icon = sr.sprite;
        }
        return icon;
    }
}
