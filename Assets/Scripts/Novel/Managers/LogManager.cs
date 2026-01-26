using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Common;
using System.Runtime.CompilerServices;

/// <summary>
/// ノベルシーンの会話ログを管理
/// </summary>
public class LogManager : MonoBehaviour
{
    [SerializeField] private GameObject logPanel;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private int maxLogs = 100;

    private Queue<string> logQueue = new Queue<string>();

    /// <summary>
    /// 会話ログに新しいエントリを追加する
    /// </summary>
    public void AddLog(string name, string text)
    {
        if (!gameObject.activeInHierarchy)
        {
            logPanel.SetActive(true);
        }

        string line;

        if (string.IsNullOrEmpty(name))
        {
            line = text;
        }
        else
        {
            // プレイヤー名（あなた）かそれ以外かで色を変える（リッチテキストを使用）
            string colorHex = name == "あなた" ? GameConstants.Colors.ChatBlue : GameConstants.Colors.ChatRed;
            line = $"<color={colorHex}>{name}</color>: {text}";
        }

        if (logQueue.Count > 0)
            logQueue.Enqueue("");

        logQueue.Enqueue(line);

        while (logQueue.Count > maxLogs)
            logQueue.Dequeue();

        UpdateLogText();
    }





    private void UpdateLogText()
    {
        logText.text = string.Join("\n", logQueue);
        Canvas.ForceUpdateCanvases();
    }
}
