using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Common;
using UnityEngine.Assertions;

/// <summary>
/// ゲーム起動時の初期化処理を管理
/// </summary>
public class BootManager : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return PartyManager.Instance.InitializeCoroutine();
        yield return LoadManager.Instance.InitializeCoroutine();
        yield return SoundManager.Instance.InitializeCoroutine();

        Assert.IsTrue(PartyManager.Instance.PartyMemberIds.Count == PartyManager.Instance.MaxPartySize, 
            "BootManager: Party size mismatch on boot.");
        SaveDataManager.Instance.UnlockCharacter(0);

        SceneManager.LoadScene(GameConstants.Scenes.Formation);
    }
}
