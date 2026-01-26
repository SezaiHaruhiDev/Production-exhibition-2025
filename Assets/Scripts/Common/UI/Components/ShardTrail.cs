using UnityEngine;
using System.Collections;

/// <summary>
/// タッチ/クリック時にパーティクル風のエフェクトを生成
/// </summary>
public class ShardTrail : MonoBehaviour
{
    [SerializeField] private RectTransform shardPrefab;
    [SerializeField] private RectTransform shardLargePrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float spawnDistance = 20f;
    [SerializeField] private RectTransform circlePrefab;
    [SerializeField] private float circleExpandDuration = 0.4f;
    [SerializeField] private float circleMaxScale = 2f;

    private Vector2 lastSpawnPos;
    private bool isDragging = false;

    private const float LargeShardDuration = 0.45f;
    private const float LargeShardMinDist = 70f;
    private const float LargeShardMaxDist = 110f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = ScreenToCanvasPos(Input.mousePosition);
            SpawnShardLarge3(pos);
            SpawnCircle(pos);
            lastSpawnPos = pos;
            isDragging = true;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 pos = ScreenToCanvasPos(Input.mousePosition);
            if (Vector2.Distance(pos, lastSpawnPos) >= spawnDistance)
            {
                SpawnShard(pos);
                lastSpawnPos = pos;
            }
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;
    }

    private Vector2 ScreenToCanvasPos(Vector2 screenPos)
    {
        Vector2 result;
        // スクリーン座標（マウス位置）をCanvas内のローカル座標に変換する
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            null,
            out result
        );
        return result;
    }

    private void SpawnShard(Vector2 pos)
    {
        if (shardPrefab == null) return;
        RectTransform shard = Instantiate(shardPrefab, canvas.transform);
        shard.anchoredPosition = pos;
        StartCoroutine(AnimateShard(shard));
    }

    private void SpawnShardLarge3(Vector2 pos)
    {
        if (shardLargePrefab == null) return;
        for (int i = 0; i < 3; i++)
        {
            RectTransform shard = Instantiate(shardLargePrefab, canvas.transform);
            shard.anchoredPosition = pos;

            // 放射状に広がるようにランダムな角度を計算（3つの破片が重なりすぎないようにオフセットを追加）
            float angle = Random.Range(0f, 360f) + (i * 40f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            StartCoroutine(AnimateShardDirectional(shard, dir, LargeShardDuration, LargeShardMinDist, LargeShardMaxDist));
        }
    }

    /// <summary>
    /// 破片が指定方向に飛び散るアニメーション
    /// </summary>
    private IEnumerator AnimateShardDirectional(RectTransform shard, Vector2 dir, float duration, float distanceMin, float distanceMax)
    {
        float t = 0f;
        float distance = Random.Range(distanceMin, distanceMax);
        float rot = Random.Range(-180f, 180f);

        CanvasGroup cg = shard.gameObject.AddComponent<CanvasGroup>();
        Vector2 startPos = shard.anchoredPosition;
        float startRot = shard.localEulerAngles.z;

        while (t < duration)
        {
            t += Time.deltaTime;
            float n = t / duration;
            shard.anchoredPosition = startPos + dir * distance * n;
            shard.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(startRot, startRot + rot, n));
            cg.alpha = 1f - n;
            yield return null;
        }
        Destroy(shard.gameObject);
    }

    /// <summary>
    /// 小さい破片のアニメーション（ランダム方向）
    /// </summary>
    private IEnumerator AnimateShard(RectTransform shard, float duration = 0.3f, float distanceMin = 20f, float distanceMax = 50f)
    {
        float t = 0f;
        Vector2 dir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(distanceMin, distanceMax);
        float rot = Random.Range(-180f, 180f);

        CanvasGroup cg = shard.gameObject.AddComponent<CanvasGroup>();
        Vector2 startPos = shard.anchoredPosition;
        float startRot = shard.localEulerAngles.z;

        while (t < duration)
        {
            t += Time.deltaTime;
            float n = t / duration;
            shard.anchoredPosition = startPos + dir * distance * n;
            shard.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(startRot, startRot + rot, n));
            cg.alpha = 1f - n;
            yield return null;
        }
        Destroy(shard.gameObject);
    }

    private void SpawnCircle(Vector2 pos)
    {
        if (circlePrefab == null) return;
        RectTransform circle = Instantiate(circlePrefab, canvas.transform);
        circle.anchoredPosition = pos;
        circle.localScale = Vector3.zero;

        CanvasGroup cg = circle.gameObject.AddComponent<CanvasGroup>();
        StartCoroutine(AnimateCircle(circle, cg));
    }

    /// <summary>
    /// 円形波紋のエフェクトアニメーション
    /// </summary>
    private IEnumerator AnimateCircle(RectTransform circle, CanvasGroup cg)
    {
        float t = 0f;
        float duration = circleExpandDuration;

        while (t < duration)
        {
            t += Time.deltaTime;
            float n = t / duration;
            circle.localScale = Vector3.one * Mathf.Lerp(0f, circleMaxScale, n);
            cg.alpha = 1f - n;
            yield return null;
        }
        Destroy(circle.gameObject);
    }
}
