using UnityEngine;
using UnityEngine.Serialization;
using Novel.Data;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Common;

/// <summary>
/// ノベルシーンのUI表示/非表示、およびフェード演出を管理する
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Main Panels")]
    [FormerlySerializedAs("TextPannel")]
    [SerializeField] private GameObject textPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject logButton;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject menuButton;

    [Header("Special Effects")]
    [SerializeField] private GameObject filmNoiseObj;

    [Header("Title UI Animation")]
    [SerializeField] private List<GameObject> titleUIItems;
    [SerializeField] private float fadeDuration = GameConstants.UI.DefaultFadeDuration;
    #endregion

    private void Awake()
    {
        Assert.IsNotNull(textPanel, "UIManager: TextPanel is not assigned!");
        Assert.IsNotNull(logButton, "UIManager: LogButton is not assigned!");
    }

    /// <summary>
    /// 各種UI要素の表示・非表示をコマンドによって切り替える
    /// </summary>
    /// <param name="cmd">実行するコマンドデータ</param>
    public void UIChange(Command cmd)
    {
        // テキストパネルの表示切り替え
        if (cmd.parameters.TryGetValue("textpanel", out string tpstring) && int.TryParse(tpstring, out int value))
        {
            if (textPanel != null) textPanel.SetActive(value == 1);
        }

        // 基本操作ボタン（ログ、スキップ、メニュー）の表示切り替え
        if (cmd.parameters.TryGetValue("logbutton", out string logbstring) && int.TryParse(logbstring, out int button))
        {
            bool show = button == 1;
            if (logButton != null) logButton.SetActive(show);
            if (skipButton != null) skipButton.SetActive(show);
            if (menuButton != null) menuButton.SetActive(show);
        }

        // メニューボタン単体の切り替え
        if (cmd.parameters.TryGetValue("menubutton", out string menubstring) && int.TryParse(menubstring, out int menuValue))
        {
            if (menuButton != null) menuButton.SetActive(menuValue == 1);
        }
        
        // レトロノイズ（フィルムノイズ）の表示切り替え
        if (cmd.parameters.TryGetValue("filmnoise", out string filmstring) && int.TryParse(filmstring, out int filmValue))
        {
            if (filmNoiseObj != null) filmNoiseObj.SetActive(filmValue == 1);
        }
    }

    /// <summary>
    /// 指定されたUIリストをフェードアウトし、完了後に非表示にする
    /// </summary>
    public void FadeOutTitleUI()
    {
        foreach (var item in titleUIItems)
        {
            if (item == null) continue;
            
            CanvasGroup cg = GetOrAddCanvasGroup(item);
            cg.DOFade(0f, fadeDuration).OnComplete(() => item.SetActive(false));
        }
    }

    /// <summary>
    /// 指定されたUIリストを表示し、フェードインさせる
    /// </summary>
    public void FadeInTitleUI()
    {
        foreach (var item in titleUIItems)
        {
            if (item == null) continue;
            
            item.SetActive(true);
            CanvasGroup cg = GetOrAddCanvasGroup(item);
            cg.alpha = 0f;
            cg.DOFade(1f, fadeDuration);
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }
}
