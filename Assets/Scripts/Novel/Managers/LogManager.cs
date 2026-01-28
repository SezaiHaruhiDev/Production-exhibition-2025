using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Common;
using UnityEngine.Assertions;

/// <summary>
/// ノベルシーンの会話ログを管理
/// </summary>
public class LogManager : MonoBehaviour
{
    [SerializeField] private GameObject logPanel;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField, Min(1)] private int maxLogs = 100;

    private Queue<string> _logQueue = new Queue<string>();

    private void Awake()
    {
        Assert.IsNotNull(logPanel, "LogManager: Log Panel is not assigned.");
        Assert.IsNotNull(logText, "LogManager: Log Text is not assigned.");
    }

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
            // プレイヤー名（あなた）かそれ以外かで色を変える
            string colorHex = name == "あなた" ? GameConstants.Colors.ChatBlue : GameConstants.Colors.ChatRed;
            line = $"<color={colorHex}>{name}</color>: {text}";
        }

        if (_logQueue.Count > 0) _logQueue.Enqueue("\n"); // 改行コードを積む代わりに空文字ではなく改行で区切る形式に変更も可だが、既存ロジック尊重

        _logQueue.Enqueue(line);

        while (_logQueue.Count > maxLogs)
        {
            _logQueue.Dequeue();
        }

        UpdateLogText();
    }

    private void UpdateLogText()
    {
        logText.text = string.Join("\n", _logQueue);
        // Canvas更新は重いため、必要なタイミングでのみ呼び出す（ここでは常時）
        Canvas.ForceUpdateCanvases();
    }
}
