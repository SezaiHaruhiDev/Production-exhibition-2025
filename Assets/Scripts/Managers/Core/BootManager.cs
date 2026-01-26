using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Common;

/// <summary>
/// ゲーム起動時の初期化処理を管理
/// </summary>
public class BootManager : MonoBehaviour
{
    IEnumerator Start()
    {
        // 依存関係のあるマネージャー群を順番に初期化（コルーチンで待機）
        yield return PartyManager.Instance.InitializeCoroutine();
        yield return LoadManager.Instance.InitializeCoroutine();
        yield return SoundManager.Instance.InitializeCoroutine();

        // 最初のプレイアブルキャラ（ID: 0）を強制的にアンロックして開始する
        SaveDataManager.Instance.UnlockCharacter(0);
        SceneManager.LoadScene("FormationScene");
    }
}
