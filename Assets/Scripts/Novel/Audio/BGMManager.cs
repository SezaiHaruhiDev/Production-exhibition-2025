using UnityEngine;
using Novel.Data;

/// <summary>
/// ノベルシーンのBGM再生を管理
/// </summary>
public class BGMManager : MonoBehaviour
{
    private const string KeyNew = "new";
    private const string KeyPlayState = "p";
    private const string KeyChange = "chan";
    private const string KeyVolume = "vol";

    private const int StateStop = 0;
    private const int StateResume = 1;
    private const int StatePause = 2;
    private const int StateUnpause = 3;

    /// <summary>
    /// コマンドパラメータを解析してBGM操作を実行
    /// </summary>
    public void ProcessCommand(Command cmd)
    {
        if (cmd.parameters.TryGetValue(KeyNew, out string bgmName))
        {
            SoundManager.Instance.PlayBGM(bgmName);
        }

        if (cmd.parameters.TryGetValue(KeyPlayState, out string stateStr) && int.TryParse(stateStr, out int state))
        {
            switch (state)
            {
                case StateStop:
                    SoundManager.Instance.StopBGM();
                    break;
                case StateResume:
                    SoundManager.Instance.ResumeBGM();
                    break;
                case StatePause:
                    SoundManager.Instance.PauseBGM();
                    break;
                case StateUnpause:
                    SoundManager.Instance.UnpauseBGM();
                    break;
            }
        }

        if (cmd.parameters.TryGetValue(KeyChange, out string changeName))
        {
            SoundManager.Instance.PlayBGM(changeName);
        }

        if (cmd.parameters.TryGetValue(KeyVolume, out string volStr) && int.TryParse(volStr, out int volume))
        {
            float volumeNormalized = Mathf.Clamp01(volume / 100f);
            SoundManager.Instance.SetBGMVolume(volumeNormalized);
        }
    }
}
