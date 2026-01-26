using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Serialization;

/// <summary>
/// パーティ編成画面のキャラクター選択UIを管理
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private CharacterRegistrySO registry;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Image SelectedCharacterImage;

    [FormerlySerializedAs("SelectPannel")]
    [SerializeField] private GameObject SelectPanel;
    [SerializeField] private Button[] partySlotButtons;
    [SerializeField] private Image[] partySlotImages;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite emptySlotBigSprite;

    public int IsSelected = -1;
    private int tempSelectedCharacterId = -1;
    public List<int> IsOwnedCharacterID = new List<int>();
    private Dictionary<int, GameObject> _inSlotHighlightDict = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> _tempSelectHighlightDict = new Dictionary<int, GameObject>();
    private Dictionary<int, Image> _buttonImageDict = new Dictionary<int, Image>();

    private void Start()
    {
        for (int i = 0; i < partySlotButtons.Length; i++)
        {
            int index = i;
            partySlotButtons[i].onClick.AddListener(() => OnPartySlotClicked(index));
        }

        if (registry == null)
        {
            Debug.LogError("CharacterSelectUI: Registry is not assigned!");
            return;
        }

        // 味方のうち、所持しているキャラのIDリストを作成
        var ownedIds = registry.AllyCharacters
            .Where(c => SaveDataManager.Instance.GetCharacter(c.id).IsOwned)
            .Select(c => c.id)
            .ToList();

        // Null解除ボタン（パーティから外す用）
        GameObject nullbutton = Instantiate(buttonPrefab, contentParent);
        nullbutton.name = "NullButton";
        Button nbtn = nullbutton.GetComponent<Button>();
        Image nbtnimg = nullbutton.GetComponent<Image>();
        if (nbtnimg != null)
        {
            nbtnimg.sprite = emptySlotSprite;
            _buttonImageDict[-1] = nbtnimg;
        }
        nbtn.onClick.AddListener(() => OnSelectCharacter(-1));

        // Nullボタンのハイライト枠を登録（現在スロットが空であることを示すハイライトなど）
        Transform nInSlot = nullbutton.transform.Find("InSlotFrame");
        if (nInSlot != null) _inSlotHighlightDict[-1] = nInSlot.gameObject;

        Transform nTempSelect = nullbutton.transform.Find("TempSelectFrame");
        if (nTempSelect != null) _tempSelectHighlightDict[-1] = nTempSelect.gameObject;

        foreach (int charID in ownedIds)
        {
            int cid = charID;
            GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
            buttonObj.name = "Character ID " + cid;

            var masterSo = registry.GetById(cid);
            if (masterSo != null)
            {
                Image btnimg = buttonObj.GetComponent<Image>();
                if (btnimg != null)
                {
                    btnimg.sprite = masterSo.characterMiniSprite;
                    _buttonImageDict[cid] = btnimg;
                }

                Button btn = buttonObj.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => OnSelectCharacter(cid));

                Transform inSlot = buttonObj.transform.Find("InSlotFrame");
                if (inSlot != null) _inSlotHighlightDict[cid] = inSlot.gameObject;

                Transform tempSelect = buttonObj.transform.Find("TempSelectFrame");
                if (tempSelect != null) _tempSelectHighlightDict[cid] = tempSelect.gameObject;
            }
        }

        // 動的なボタン追加後にGrid Layout Groupなどの計算を即座に確定させる
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
        CharacterImageRefresh();
    }

    /// <summary>
    /// キャラクター選択処理。リストからキャラを選ぶ、または選択解除する。
    /// </summary>
    /// <param name="id">キャラクターID（-1で解除）</param>
    public void OnSelectCharacter(int id)
    {
        if (id == -1)
        {
            if (SelectedCharacterImage != null) SelectedCharacterImage.sprite = emptySlotBigSprite;
            tempSelectedCharacterId = -1;
        }
        else
        {
            var masterSo = registry.GetById(id);
            if (masterSo != null)
            {
                if (SelectedCharacterImage != null) SelectedCharacterImage.sprite = masterSo.characterBigSprite;
                tempSelectedCharacterId = id;
            }
        }

        RefreshListHighlights();
    }

    /// <summary>
    /// OKボタン押下時。選択中のキャラを現在の編成スロットに確定する。
    /// </summary>
    public void OnOkClick()
    {
        if (IsSelected >= 0 && IsSelected < PartyManager.Instance.maxPartySize)
        {
            PartyManager.Instance.SetMember(IsSelected, tempSelectedCharacterId);
        }
        else
        {
            Debug.LogWarning("選択が無効です");
        }

        if (SelectedCharacterImage != null) SelectedCharacterImage.sprite = null;
        tempSelectedCharacterId = -1;

        if (SelectPanel != null) SelectPanel.SetActive(false);
        CharacterImageRefresh();
    }

    /// <summary>
    /// キャンセルボタン押下時。仮選択状態を解除する。
    /// </summary>
    public void CancelCkick()
    {
        tempSelectedCharacterId = -1;
        if (SelectPanel != null) SelectPanel.SetActive(false);
        CharacterImageRefresh();
        RefreshListHighlights();
    }

    /// <summary>
    /// パーティスロット（編成枠）クリック時。枠を選択状態にし、現在のキャラを表示する。
    /// </summary>
    /// <param name="index">スロット番号</param>
    public void OnPartySlotClicked(int index)
    {
        IsSelected = index;
        int currentCharId = PartyManager.Instance.PartyMemberIds[index];

        var masterSo = registry.GetById(currentCharId);
        if (masterSo != null)
        {
            if (SelectedCharacterImage != null) SelectedCharacterImage.sprite = masterSo.characterBigSprite;
            tempSelectedCharacterId = currentCharId;
        }
        else
        {
            if (SelectedCharacterImage != null) SelectedCharacterImage.sprite = emptySlotBigSprite;
            tempSelectedCharacterId = -1;
        }

        if (SelectPanel != null) SelectPanel.SetActive(true);
        RefreshListHighlights();
    }

    /// <summary>
    /// パーティスロットの画像を最新の状態に更新する。
    /// </summary>
    public void CharacterImageRefresh()
    {
        for (int i = 0; i < partySlotImages.Length; i++)
        {
            int characterId = PartyManager.Instance.PartyMemberIds[i];
            var masterSo = registry.GetById(characterId);

            if (partySlotImages[i] != null)
            {
                partySlotImages[i].sprite = (masterSo != null) ? masterSo.characterBigSprite : emptySlotBigSprite;
            }
        }
    }

    private void RefreshListHighlights()
    {
        var partyMemberIds = PartyManager.Instance.PartyMemberIds;

        foreach (var kvp in _inSlotHighlightDict)
        {
            if (kvp.Value != null)
            {
                // すでにパーティ（いずれかのスロット）に入っているキャラを暗くするなどの強調表示
                bool isInParty = kvp.Key != -1 && partyMemberIds.Contains(kvp.Key);
                kvp.Value.SetActive(isInParty);
            }
        }
 
        foreach (var kvp in _tempSelectHighlightDict)
        {
            if (kvp.Value != null)
            {
                // 現在「クリックして詳細表示中」のキャラを枠で囲むなどの強調表示
                kvp.Value.SetActive(kvp.Key == tempSelectedCharacterId);
            }
        }
    }
}
