using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Novel.Data;

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
    private bool isChoiceActive = false;
    /// <summary>
    /// 選択肢ボタンを生成し、ユーザーの入力を待機するコルーチン
    /// </summary>
    public IEnumerator ChoiceButton(Command cmd, System.Action<string> onChoiceSelected)
    {
        DestroyButtons();

        if (overlay != null)
            overlay.SetActive(true);

        isChoiceActive = true;

        List<Button> buttons = new List<Button>();

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("ChoiceButtonPrefab is not assigned in Inspector!");
            yield break;
        }

        foreach (var kv in cmd.parameters)
        {
            string val = kv.Value;
            // "表示名^移動先ラベル" の形式（ノベルスクリプトの仕様）
            string[] parts = val.Split('^');
            if (parts.Length != 2) continue;

            string buttonText = parts[0];
            string labelName = parts[1];

            GameObject go = Instantiate(choiceButtonPrefab, parent);
            Button btn = go.GetComponent<Button>();
            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = buttonText;

            btn.interactable = false;
            buttons.Add(btn);

            btn.onClick.AddListener(() =>
            {
                if (overlay != null)
                    overlay.SetActive(false);

                string msg = $"あなたは「{buttonText}」を選んだ。";
                if (logManager != null)
                    logManager.AddLog("選択", msg);

                isChoiceActive = false;
                onChoiceSelected?.Invoke(labelName);
                DestroyButtons();
                novelEngine.OnClick();
            });
        }

        yield return new WaitForSeconds(0.5f);
        foreach (var btn in buttons)
            btn.interactable = true;

        while (isChoiceActive)
            yield return null;

        if (overlay != null)
            overlay.SetActive(false);
    }


    private void DestroyButtons()
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
}
