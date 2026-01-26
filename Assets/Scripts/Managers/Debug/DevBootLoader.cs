using System.Collections;
using UnityEngine;

/// <summary>
/// 開発時、BootSceneを経由せずにシーンを再生した場合の初期化を代行するローダー。
/// 本番ビルドでは含まれないか、無効化することを想定。
/// </summary>
public class DevBootLoader : MonoBehaviour
{
    [SerializeField] private GameObject managersPrefab; // PartyManagerなどがついているプレハブ

    private IEnumerator Start()
    {
        // 既にマネージャーが存在する（BootScene経由）場合は何もしない
        if (PartyManager.Instance != null && LoadManager.Instance != null)
        {
            // 二重存在の可能性があるため、このローダー自体を破棄してもよい
            // Destroy(gameObject);
            yield break;
        }

        Debug.LogWarning("DevBootLoader: BootSceneを経由していないため、マネージャーを生成・初期化します。");

        if (managersPrefab != null)
        {
            Instantiate(managersPrefab);
        }
        else
        {
            Debug.LogError("DevBootLoader: ManagersPrefabが設定されていません！Inspectorで割り当ててください。");
            yield break;
        }

        // Awake処理待ち（1フレーム待機）
        yield return null;

        // BootManagerと同様の順序で初期化を実行

        // 1. PartyManager
        if (PartyManager.Instance != null)
            yield return PartyManager.Instance.InitializeCoroutine();

        // 2. LoadManager
        if (LoadManager.Instance != null)
            yield return LoadManager.Instance.InitializeCoroutine();

        // 3. SoundManager
        if (SoundManager.Instance != null)
            yield return SoundManager.Instance.InitializeCoroutine();

        // 4. 初期データ調整（UnlockCharacterなど）
        if (SaveDataManager.Instance != null)
        {
            SaveDataManager.Instance.UnlockCharacter(0);
        }

        Debug.Log("DevBootLoader: 初期化完了");
    }
}
