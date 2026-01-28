using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

/// <summary>
/// ScrollViewを最下部までスクロールする補助機能
/// </summary>
public class ScrollToBottomHelper : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        Assert.IsNotNull(scrollRect, "ScrollToBottomHelper: ScrollRect is not assigned.");
        Assert.IsNotNull(panel, "ScrollToBottomHelper: Panel is not assigned.");
    }

    /// <summary>
    /// ログボタンクリック時。パネルを表示し、最下部へスクロールする。
    /// </summary>
    public void LogOnClick()
    {
        panel.SetActive(true);
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// 閉じるボタンクリック時。パネルを非表示にする。
    /// </summary>
    public void CloseLogPanel()
    {
        panel.SetActive(false);
    }

    /// <summary>
    /// 次のフレームでScrollRectを最下部に移動させるコルーチン
    /// </summary>
    public IEnumerator ScrollToBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
