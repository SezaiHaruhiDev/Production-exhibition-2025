using UnityEngine;
using UnityEngine.Serialization;
using Novel.Data;

/// <summary>
/// ノベルシーンのUI表示/非表示を管理
/// </summary>
public class UIManager : MonoBehaviour
{

    [FormerlySerializedAs("TextPannel")]
    [SerializeField] GameObject TextPanel;

    [SerializeField] GameObject LogButton;
    [SerializeField] GameObject SkipButton;
    [SerializeField] GameObject MenuButton;

    [SerializeField] GameObject FilmNoiseObj;

    /// <summary>
    /// UI（テキストパネル、ログボタン等）の表示・非表示を切り替える
    /// </summary>
    public void UIChange(Command cmd)
    {
        if (cmd.parameters.TryGetValue("textpanel", out string tpstring) && int.TryParse(tpstring, out int value))
        {
            // コマンド引数(string)をintとして解釈し、1なら表示(true)、それ以外なら非表示(false)とする
            if (TextPanel != null) TextPanel.SetActive(value == 1);
        }

        if (cmd.parameters.TryGetValue("logbutton", out string logbstring) && int.TryParse(logbstring, out int button))
        {
            if (LogButton != null) LogButton.SetActive(button == 1);
            if (SkipButton != null) SkipButton.SetActive(button == 1);
            if (MenuButton != null) MenuButton.SetActive(button == 1);
        }

        if (cmd.parameters.TryGetValue("menubutton", out string menubstring) && int.TryParse(menubstring, out int menuValue))
        {
            if (MenuButton != null) MenuButton.SetActive(menuValue == 1);
        }
        
        // レトロノイズの表示切り替え
        if (cmd.parameters.TryGetValue("filmnoise", out string filmstring) && int.TryParse(filmstring, out int filmValue))
        {
            if (FilmNoiseObj != null) FilmNoiseObj.SetActive(filmValue == 1);
        }
    }
}
