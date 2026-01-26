using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

/// <summary>
/// ScrollViewを最下部までスクロールする補助機能
/// </summary>
public class ScrollToBottomHelper : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    [FormerlySerializedAs("pannel")]
    [SerializeField] private GameObject panel;

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
    public void LogOnClick2()
    {
        panel.SetActive(false);
    }
    /// <summary>
    /// 次のフレームでScrollRectを最下部に移動させるコルーチン
    /// </summary>
    public IEnumerator ScrollToBottom()
    {
        yield return null;
        // レイアウト計算を強制実行し、正しい高さが確定してからスクロール位置をセットする
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
