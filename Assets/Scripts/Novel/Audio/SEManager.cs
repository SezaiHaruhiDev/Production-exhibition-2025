using UnityEngine;
using Novel.Data;

/// <summary>
/// ノベルシーンのSE再生を管理
/// </summary>
public class SEManager : MonoBehaviour
{
    private const string KeyPlaySE = "sep";

    /// <summary>
    /// ノベルスクリプトからのSE再生コマンドを処理する
    /// </summary>
    public void ProcessCommand(Command cmd)
    {
        if (cmd.parameters.TryGetValue(KeyPlaySE, out string seName))
        {
            SoundManager.Instance.PlaySE(seName);
        }
    }
}
