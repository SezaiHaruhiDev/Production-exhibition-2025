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
    
    private List<TurnOrderUIItem> _items = new List<TurnOrderUIItem>();

    private void Awake()
    {
        if (turnOrderManager == null) turnOrderManager = FindFirstObjectByType<TurnOrderManager>();
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();
        
        // Registry reference should be assigned in Inspector, but could try to find it
        // Usually safer to rely on inspector assignment or a singleton manager if available
    }

    private void Start()
    {
        // 初期化時に一度更新
        UpdateTurnOrder();
    }

    private void Update()
    {
        // 毎フレーム更新するか、イベントベースにするか
        // TurnManagerのゲージは毎フレーム動くので、本来は毎フレーム更新が正しいが、
        // 負荷軽減のため、一定間隔またはTurnManagerの状態変化を監視するのが良い。
        // ここでは簡易的に、常に更新をかける（シミュレーションコストが低いと仮定）
        // もし重ければ、TurnCountが変化したときや、アクション終了時のみにする。
        
        // とりあえず今回は「常に最新の状況を反映」させるため、Updateで呼ぶ。
        UpdateTurnOrder();
    }

    public void UpdateTurnOrder()
    {
        if (turnOrderManager == null || container == null || turnOrderUIItemPrefab == null) return;

        List<BattleUnit> order = turnOrderManager.CalculateTurnOrder(predictionCount);
        
        // UIアイテムの数を調整（プーリング的な再利用）
        while (_items.Count < order.Count)
        {
            var go = Instantiate(turnOrderUIItemPrefab, container);
            var item = go.GetComponent<TurnOrderUIItem>();
            if (item != null) _items.Add(item);
        }
        
        // 余分なアイテムは非表示あるいは削除
        for (int i = 0; i < _items.Count; i++)
        {
            if (i < order.Count)
            {
                _items[i].gameObject.SetActive(true);
                
                BattleUnit unit = order[i];
                Sprite icon = null;
                
                // マスターデータからアイコンを取得
                if (characterRegistry != null && unit.Data != null)
                {
                    var master = characterRegistry.GetById(unit.Data.characterId);
                    if (master != null)
                    {
                        // characterMiniSprite優先、なければBattleSprite、それもなければUnitSprite（現在表示中のもの）
                        icon = master.characterMiniSprite;
                        if (icon == null) icon = master.characterBattleSprite;
                    }
                }
                
                // フォールバック: 現在のユニットの表示画像
                if (icon == null)
                {
                    // BattleUnitにUnitSpriteプロパティがあると仮定（先ほど追加したもの）
                    // もしない場合はGetComponent<SpriteRenderer>().spriteなど
                    var sr = unit.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) icon = sr.sprite;
                }

                _items[i].Setup(unit, icon);
            }
            else
            {
                _items[i].gameObject.SetActive(false);
            }
        }
    }
}
