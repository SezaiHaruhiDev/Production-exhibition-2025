using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Data;
using Common;
using Novel.Data;

/// <summary>
/// ノベルシーンのキャラクター画像を管理（生成、表示切替、サイズ変更）
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject characterImagePrefab;
    [SerializeField] private string spritesDirectory = GameConstants.Resources.SpritesDir;

    private Dictionary<string, Image> _characterCache = new Dictionary<string, Image>();

    /// <summary>
    /// コマンドに基づいてキャラクター画像を生成する
    /// </summary>
    public void CreateCharacter(Command cmd)
    {
        if (characterImagePrefab == null)
        {
            return;
        }
        GameObject go = Instantiate(characterImagePrefab, parent);
        Image newImage = go.GetComponent<Image>();
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string str))
        {
            newImage.name = str;
            _characterCache[str] = newImage;
        }
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Sprite, out string spstr))
        {
            Sprite sp = Resources.Load<Sprite>(spritesDirectory + spstr);
            if (sp != null)
            {
                newImage.sprite = sp;
            }
            else
            {
                Debug.LogError("画像が見つかりません: " + spritesDirectory + spstr);
            }
        }
    }

    /// <summary>
    /// キャラクターのサイズや位置、色を変更する
    /// </summary>
    public void ChangeCharacterSize(Command cmd)
    {
        if (!cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string name))
            return;
        if (!_characterCache.TryGetValue(name, out Image img))
        {
            Debug.LogWarning($"キャラキャッシュに '{name}' が見つかりませんでした。");
            return;
        }
        RectTransform rt = img.GetComponent<RectTransform>();
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Size, out string sint))
        {
            // Format: "width^height" (例: 500^800)
            string[] parts = sint.Split('^');
            if (parts.Length == 2 &&
            float.TryParse(parts[0], out float width) &&
            float.TryParse(parts[1], out float height))
            {
                img.rectTransform.sizeDelta = new Vector2(width, height);
            }
            else
            {
                Debug.LogWarning($"size の形式が不正です: {sint}");
            }
        }
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Pos, out string pint))
        {
            // Format: "x^y" (例: 100^-50)
            string[] posparts = pint.Split('^');
            if (posparts.Length == 2 &&
            int.TryParse(posparts[0], out int posx) &&
            int.TryParse(posparts[1], out int posy))
            {
                rt.anchoredPosition = new Vector2(posx, posy);
            }
        }
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Col, out string colint))
        {
            string[] colparts = colint.Split('^');
            if (colparts.Length >= 4 && int.TryParse(colparts[0], out int r) && int.TryParse(colparts[1], out int g) && int.TryParse(colparts[2], out int b) && int.TryParse(colparts[3], out int a))
            {
                img.color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }
    }

    /// <summary>
    /// キャラクターの画像差し替えや表示/非表示を切り替える
    /// </summary>
    public void ChangeCharacter(Command cmd)
    {
        if (!cmd.parameters.TryGetValue(GameConstants.NovelCommands.Name, out string name))
            return;

        if (!_characterCache.TryGetValue(name, out Image img))
        {
            Debug.LogWarning($"キャラキャッシュに '{name}' が見つかりませんでした。");
            return;
        }
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Sprite, out string spstr))
        {
            Sprite sp = Resources.Load<Sprite>(spritesDirectory + spstr);
            if (sp != null)
            {
                img.sprite = sp;
            }
            else
            {
                Debug.LogError("画像が見つかりません: " + spritesDirectory + spstr);
            }
        }
        // Show/Hide (0: Hide, 1: Show)
        if (cmd.parameters.TryGetValue(GameConstants.NovelCommands.Show, out string show))
        {
            if (int.TryParse(show, out int result))
            {
                if (result == 0)
                {
                    if (img != null)
                    {
                        img.enabled = false;
                    }
                }
                else if (result == 1)
                {
                    if (img != null)
                    {
                        img.enabled = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 指定名のキャラクター画像のRectTransformを取得する
    /// </summary>
    public RectTransform GetCharacterRect(string name)
    {
        if (_characterCache.TryGetValue(name, out Image img))
        {
            return img.rectTransform;
        }
        return null;
    }
}
