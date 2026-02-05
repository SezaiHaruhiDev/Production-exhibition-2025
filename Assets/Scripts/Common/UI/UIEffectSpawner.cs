using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI上でスプライトを破片のように散らすエフェクト
/// テンプレートオブジェクト不要版
/// </summary>
public class UIEffectSpawner : MonoBehaviour
{
    public static UIEffectSpawner Instance { get; private set; }

    [SerializeField] private Image _debrisTemplate; // オプション：あればこれを使う、なければ生成する
    [SerializeField] private Sprite _defaultDebrisSprite; // デフォルトで使用するスプライト

    [Header("Settings")]
    [SerializeField] private int spawnCount = 10; // 生成数
    [SerializeField] private float moveDistance = 200f; // 飛んでいく距離
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.0f;

    private void Awake()
    {
        Instance = this;
        if (_debrisTemplate != null)
        {
            _debrisTemplate.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 設定されたデフォルトのスプライトを使って散らす（インスペクターで設定用）
    /// </summary>
    public void Play()
    {
        if (_defaultDebrisSprite == null)
        {
            Debug.LogWarning("UIEffectSpawner: Default Debris Sprite is not assigned.");
            return;
        }
        Scatter(_defaultDebrisSprite, Vector2.zero, spawnCount, null, moveDistance);
    }

    /// <summary>
    /// 指定したスプライトを四方八方に散らす
    /// </summary>
    /// <param name="sprite">表示する画像</param>
    /// <param name="position">発生位置（parent内のローカル座標）</param>
    /// <param name="count">個数</param>
    /// <param name="parent">親にするRectTransform（nullならこのスクリプトのある階層）</param>
    /// <param name="power">飛び散る強さ（距離）</param>
    public void Scatter(Sprite sprite, Vector2 position, int count = 10, RectTransform parent = null, float power = 200f)
    {
        Transform targetParent = parent != null ? parent : transform;

        for (int i = 0; i < count; i++)
        {
            Image debris;

            // テンプレートがあれば複製、なければ新規作成
            if (_debrisTemplate != null)
            {
                debris = Instantiate(_debrisTemplate, targetParent);
            }
            else
            {
                // 新規GameObject作成
                var go = new GameObject($"Debris_{i}");
                go.transform.SetParent(targetParent, false);
                debris = go.AddComponent<Image>();
                debris.raycastTarget = false; // クリック判定を無効化
            }

            debris.gameObject.SetActive(true);
            debris.sprite = sprite;
            debris.rectTransform.anchoredPosition = position;
            
            // サイズを少しランダムに（元の画像サイズに依存するので調整）
            debris.SetNativeSize(); 

            // ランダム要素
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float distance = Random.Range(power * 0.5f, power * 1.5f);
            float duration = Random.Range(0.5f, 1.0f);
            float scale = Random.Range(minScale, maxScale); // 設定されたスケール範囲を使用

            debris.transform.localScale = Vector3.one * scale;

            // アニメーション (DOTween)
            // 1. 移動
            debris.rectTransform.DOAnchorPos(dir * distance, duration)
                .SetRelative(true)
                .SetEase(Ease.OutExpo);

            // 2. 回転
            debris.rectTransform.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), duration, RotateMode.FastBeyond360);

            // 3. 縮小 & 削除
            debris.rectTransform.DOScale(0f, duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(debris.gameObject));
        }
    }
}
