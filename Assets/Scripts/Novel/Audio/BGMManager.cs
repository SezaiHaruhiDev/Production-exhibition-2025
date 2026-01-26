using UnityEngine;
using Novel.Data;
using UnityEngine.Serialization;
using System.Collections;

/// <summary>
/// ノベルシーンのBGM再生を管理（フェードイン/アウト対応）
/// </summary>
public class BGMManager : MonoBehaviour
{
    void Start()
    {
    }

    /// <summary>
    /// ノベルスクリプトからのコマンド（bgm, seなど）を処理し、SoundManagerに命令を送る
    /// </summary>
    public void ProcessCommand(Command cmd)
    {
        if (cmd.parameters.TryGetValue("new", out string bgmstr))
        {
            SoundManager.Instance.PlayBGM(bgmstr);
        }

        // "p": Play/Pause State (0: Stop, 1: Resume)
        if (cmd.parameters.TryGetValue("p", out string pstr))
        {
            if (int.TryParse(pstr, out int pint))
            {
                if (pint == 0)
                    SoundManager.Instance.StopBGM();
                if (pint == 1)
                    SoundManager.Instance.ResumeBGM();
                if (pint == 2)
                    SoundManager.Instance.PauseBGM();
                if (pint == 3)
                    SoundManager.Instance.UnpauseBGM();
            }
        }

        // "chan": Change BGM Track
        if (cmd.parameters.TryGetValue("chan", out string cbgmstr))
        {
            SoundManager.Instance.PlayBGM(cbgmstr);
        }

        if (cmd.parameters.TryGetValue("vol", out string volstr))
        {
            if (int.TryParse(volstr, out int volint))
            {
                float volflo = Mathf.Clamp01(volint / 100f);
                SoundManager.Instance.SetBGMVolume(volflo);
            }
        }
    }
}
