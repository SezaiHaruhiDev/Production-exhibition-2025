using UnityEngine;
using Novel.Data;
using UnityEngine.Serialization;

/// <summary>
/// ノベルシーンのSE再生を管理
/// </summary>
public class SEManager : MonoBehaviour
{
    /// <summary>
    /// ノベルスクリプトからのSE再生コマンドを処理する
    /// </summary>
    public void ProcessCommand(Command cmd)
    {
        // "sep" (SE Play) コマンドを処理
        if (cmd.parameters.TryGetValue("sep", out string sestr))
        {
            SoundManager.Instance.PlaySE(sestr);
        }
    }
}
