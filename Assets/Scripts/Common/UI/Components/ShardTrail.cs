using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

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

    private Vector2 _lastSpawnPos;
    private bool _isDragging = false;

    private const float LargeShardDuration = 0.45f;
    private const float LargeShardMinDist = 70f;
    private const float LargeShardMaxDist = 110f;

    private void Awake()
    {
        Assert.IsNotNull(shardPrefab, "ShardTrail: Shard Prefab is not assigned.");
        Assert.IsNotNull(shardLargePrefab, "ShardTrail: Shard Large Prefab is not assigned.");
        Assert.IsNotNull(canvas, "ShardTrail: Canvas is not assigned.");
        Assert.IsNotNull(circlePrefab, "ShardTrail: Circle Prefab is not assigned.");
    }

    private void Update()
    {
        bool isPointerDown = Input.GetMouseButtonDown(0);
        bool isPointerUp = Input.GetMouseButtonUp(0);
        bool isPointerHeld = Input.GetMouseButton(0);

        if (isPointerDown)
        {
            Vector2 pos = ScreenToCanvasPos(Input.mousePosition);
            SpawnShardLarge3(pos);
            SpawnCircle(pos);
            _lastSpawnPos = pos;
            _isDragging = true;
        }

        if (_isDragging && isPointerHeld)
        {
            Vector2 pos = ScreenToCanvasPos(Input.mousePosition);
            if (Vector2.Distance(pos, _lastSpawnPos) >= spawnDistance)
            {
                SpawnShard(pos);
                _lastSpawnPos = pos;
            }
        }

        if (isPointerUp) _isDragging = false;
    }

    private Vector2 ScreenToCanvasPos(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            null,
            out Vector2 result
        );
        return result;
    }

    private void SpawnShard(Vector2 pos)
    {
        RectTransform shard = Instantiate(shardPrefab, canvas.transform);
        shard.anchoredPosition = pos;
        StartCoroutine(AnimateShard(shard));
    }

    private void SpawnShardLarge3(Vector2 pos)
    {
        for (int i = 0; i < 3; i++)
        {
            RectTransform shard = Instantiate(shardLargePrefab, canvas.transform);
            shard.anchoredPosition = pos;

            float angle = Random.Range(0f, 360f) + (i * 40f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            StartCoroutine(AnimateShardDirectional(shard, dir, LargeShardDuration, LargeShardMinDist, LargeShardMaxDist));
        }
    }

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
            float n = t / duration; // Linear lerp for simplicity, can act as easing
            shard.anchoredPosition = startPos + dir * distance * n;
            shard.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(startRot, startRot + rot, n));
            cg.alpha = 1f - n;
            yield return null;
        }
        Destroy(shard.gameObject);
    }

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
        RectTransform circle = Instantiate(circlePrefab, canvas.transform);
        circle.anchoredPosition = pos;
        circle.localScale = Vector3.zero;

        CanvasGroup cg = circle.gameObject.AddComponent<CanvasGroup>();
        StartCoroutine(AnimateCircle(circle, cg));
    }

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
