using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Novel.Data;
using Novel.Managers;
using UnityEngine.Assertions;

/// <summary>
/// ノベルシーンの選択肢ボタンを管理
/// </summary>
public class ChoicesManager : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private GameObject overlay;
    [SerializeField] private LogManager logManager;
    [SerializeField] private NovelEngine novelEngine;

    private bool _isChoiceActive = false;

    private void Awake()
    {
        Assert.IsNotNull(parent, "ChoicesManager: Parent transform is missing.");
        Assert.IsNotNull(choiceButtonPrefab, "ChoicesManager: Button Prefab is missing.");
        Assert.IsNotNull(novelEngine, "ChoicesManager: NovelEngine reference is missing.");
    }

    /// <summary>
    /// 選択肢ボタンを生成し、ユーザーの入力を待機するコルーチン
    /// </summary>
    public IEnumerator ChoiceButton(Command cmd, System.Action<string> onChoiceSelected)
    {
        DestroyButtons();

        if (overlay != null) overlay.SetActive(true);
        _isChoiceActive = true;

        List<Button> buttons = new List<Button>();

        foreach (var kv in cmd.parameters)
        {
            // "表示名^移動先ラベル" の形式
            string val = kv.Value;
            string[] parts = val.Split('^');
            if (parts.Length != 2) continue;

            string buttonText = parts[0];
            string labelName = parts[1];

            // 既に選択済みの場合は表示しない
            if (novelEngine.FlagManager != null && novelEngine.FlagManager.HasFlag(labelName))
            {
                continue;
            }

            GameObject go = Instantiate(choiceButtonPrefab, parent);
            Button btn = go.GetComponent<Button>();
            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();

            if (tmp != null) tmp.text = buttonText;

            btn.interactable = false;
            buttons.Add(btn);

            btn.onClick.AddListener(() =>
            {
                if (overlay != null) overlay.SetActive(false);

                // 選択ログ
                if (logManager != null)
                {
                    logManager.AddLog("選択", $"あなたは「{buttonText}」を選んだ。");
                }

                // フラグを立てる
                if (novelEngine.FlagManager != null)
                {
                    novelEngine.FlagManager.SetFlag(labelName);
                }

                _isChoiceActive = false;
                onChoiceSelected?.Invoke(labelName);
                DestroyButtons();
                novelEngine.OnClick();
            });
        }

        // 即座に押せないように少し待つ
        yield return new WaitForSeconds(0.5f);
        foreach (var btn in buttons)
        {
            if (btn != null) btn.interactable = true;
        }

        while (_isChoiceActive)
        {
            yield return null;
        }

        if (overlay != null) overlay.SetActive(false);
    }

    private void DestroyButtons()
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}
