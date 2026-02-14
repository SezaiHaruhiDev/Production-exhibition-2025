using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Common;
using Novel.Data;
using UnityEngine.Assertions;

/// <summary>
/// ノベルシーンのキャラクター画像を管理（生成、表示切替、サイズ変更）
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject characterImagePrefab;
    [SerializeField] private string spritesDirectory = GameConstants.Resources.SpritesDir;

    private Dictionary<string, Image> _characterCache = new Dictionary<string, Image>();

    private void Awake()
    {
        Assert.IsNotNull(parent, "CharacterManager: Parent transform is not assigned!");
        Assert.IsNotNull(characterImagePrefab, "CharacterManager: Character Image Prefab is not assigned!");
    }

    /// <summary>
    /// コマンドに基づいてキャラクター画像を生成する
    /// </summary>
    public void CreateCharacter(Command cmd)
    {
        if (characterImagePrefab == null) return;

        GameObject go = Instantiate(characterImagePrefab, parent);
        Image newImage = go.GetComponent<Image>();

        // 初期状態で非表示にする（あとでパラメータに基づいて制御）
        newImage.enabled = false;

        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string str))
        {
            newImage.name = str;
            _characterCache[str] = newImage;
        }

        // 全パラメータを一括適用
        ApplyCommandParameters(cmd, newImage);
    }

    /// <summary>
    /// キャラクターのサイズや位置、色を変更する
    /// </summary>
    public void ChangeCharacterSize(Command cmd)
    {
        if (!cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string name)) return;
        if (!_characterCache.TryGetValue(name, out Image img))
        {
            Debug.LogWarning($"CharacterManager: Character '{name}' not found in cache.");
            return;
        }

        ApplyCommandParameters(cmd, img);
    }

    /// <summary>
    /// キャラクターの画像差し替えや表示/非表示を切り替える
    /// </summary>
    public void ChangeCharacter(Command cmd)
    {
        if (!cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string name)) return;
        if (!_characterCache.TryGetValue(name, out Image img))
        {
            Debug.LogWarning($"CharacterManager: Character '{name}' not found in cache.");
            return;
        }

        ApplyCommandParameters(cmd, img);
    }

    /// <summary>
    /// コマンドのパラメータをImage/RectTransformに適用する
    /// </summary>
    private void ApplyCommandParameters(Command cmd, Image img)
    {
        if (img == null) return;

        RectTransform rt = img.rectTransform;

        // スプライト
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Sprite, out string spstr))
        {
            Sprite sp = Resources.Load<Sprite>(spritesDirectory + spstr);
            if (sp != null)
            {
                img.sprite = sp;
            }
            else
            {
                Debug.LogError($"CharacterManager: Sprite not found at {spritesDirectory}{spstr}");
            }
        }

        // 表示・非表示
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Show, out string showStr) &&
            int.TryParse(showStr, out int showState))
        {
            img.enabled = (showState == 1);
        }

        // サイズ (Format: "width^height")
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Size, out string sizeStr))
        {
            string[] parts = sizeStr.Split('^');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], out float width) &&
                float.TryParse(parts[1], out float height))
            {
                rt.sizeDelta = new Vector2(width, height);
            }
        }

        // 位置 (Format: "x^y")
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Pos, out string posStr))
        {
            string[] parts = posStr.Split('^');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y))
            {
                rt.anchoredPosition = new Vector2(x, y);
            }
        }

        // 色 (Format: "r^g^b^a")
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Col, out string colStr))
        {
            string[] parts = colStr.Split('^');
            if (parts.Length >= 4 &&
                float.TryParse(parts[0], out float r) &&
                float.TryParse(parts[1], out float g) &&
                float.TryParse(parts[2], out float b) &&
                float.TryParse(parts[3], out float a))
            {
                img.color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }
    }

    /// <summary>
    /// 指定名のキャラクター画像のRectTransformを取得する
    /// </summary>
    public RectTransform GetCharacterRect(string name)
    {
        return _characterCache.TryGetValue(name, out Image img) ? img.rectTransform : null;
    }
}
