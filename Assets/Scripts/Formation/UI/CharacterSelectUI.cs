using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Assertions;

/// <summary>
/// パーティ編成画面のキャラクター選択UIを管理
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    private const string NullButtonName = "NullButton";
    private const string FrameInSlot = "InSlotFrame";
    private const string FrameTempSelect = "TempSelectFrame";
    private const int NullCharacterId = -1;

    [SerializeField] private CharacterRegistrySO registry;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Image selectedCharacterImage;

    [FormerlySerializedAs("SelectPannel")]
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private Button[] partySlotButtons;
    [SerializeField] private Image[] partySlotImages;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite emptySlotBigSprite;

    public int CurrentSlotIndex { get; private set; } = -1;

    private int _tempSelectedCharacterId = -1;
    private Dictionary<int, GameObject> _inSlotHighlightDict = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> _tempSelectHighlightDict = new Dictionary<int, GameObject>();
    private Dictionary<int, Image> _buttonImageDict = new Dictionary<int, Image>();

    private void Awake()
    {
        Assert.IsNotNull(registry, "CharacterSelectUI: Registry is not assigned!");
        Assert.IsNotNull(buttonPrefab, "CharacterSelectUI: Button Prefab is not assigned!");
        Assert.IsNotNull(contentParent, "CharacterSelectUI: Content Parent is not assigned!");
        Assert.IsNotNull(selectPanel, "CharacterSelectUI: Select Panel is not assigned!");
    }

    private void Start()
    {
        for (int i = 0; i < partySlotButtons.Length; i++)
        {
            int index = i;
            partySlotButtons[i].onClick.AddListener(() => OnPartySlotClicked(index));
        }

        var ownedIds = registry.AllyCharacters
            .Where(c => SaveDataManager.Instance.GetCharacter(c.id).IsOwned)
            .Select(c => c.id)
            .ToList();

        CreateCharacterButton(NullCharacterId, emptySlotSprite);

        foreach (int charID in ownedIds)
        {
            var masterSo = registry.GetById(charID);
            if (masterSo != null)
            {
                CreateCharacterButton(charID, masterSo.characterMiniSprite);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
        CharacterImageRefresh();
    }

    private void CreateCharacterButton(int charID, Sprite sprite)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
        buttonObj.name = charID == NullCharacterId ? NullButtonName : $"Character ID {charID}";

        Image btnImg = buttonObj.GetComponent<Image>();
        if (btnImg != null)
        {
            btnImg.sprite = sprite;
            _buttonImageDict[charID] = btnImg;
        }

        Button btn = buttonObj.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(() => OnSelectCharacter(charID));

        Transform inSlot = buttonObj.transform.Find(FrameInSlot);
        if (inSlot != null) _inSlotHighlightDict[charID] = inSlot.gameObject;

        Transform tempSelect = buttonObj.transform.Find(FrameTempSelect);
        if (tempSelect != null) _tempSelectHighlightDict[charID] = tempSelect.gameObject;
    }

    /// <summary>
    /// キャラクター選択処理
    /// </summary>
    /// <param name="id">キャラクターID（-1で解除）</param>
    public void OnSelectCharacter(int id)
    {
        if (id == NullCharacterId)
        {
            if (selectedCharacterImage != null) selectedCharacterImage.sprite = emptySlotBigSprite;
            _tempSelectedCharacterId = NullCharacterId;
        }
        else
        {
            var masterSo = registry.GetById(id);
            if (masterSo != null)
            {
                if (selectedCharacterImage != null) selectedCharacterImage.sprite = masterSo.characterBigSprite;
                _tempSelectedCharacterId = id;
            }
        }

        RefreshListHighlights();
    }

    /// <summary>
    /// OKボタン押下時
    /// </summary>
    public void OnOkClick()
    {
        if (CurrentSlotIndex >= 0 && CurrentSlotIndex < PartyManager.Instance.MaxPartySize)
        {
            PartyManager.Instance.SetMember(CurrentSlotIndex, _tempSelectedCharacterId);
        }

        if (selectedCharacterImage != null) selectedCharacterImage.sprite = null;
        _tempSelectedCharacterId = NullCharacterId;

        if (selectPanel != null) selectPanel.SetActive(false);
        CharacterImageRefresh();
    }

    /// <summary>
    /// キャンセルボタン押下時
    /// </summary>
    public void CancelCkick()
    {
        _tempSelectedCharacterId = NullCharacterId;
        if (selectPanel != null) selectPanel.SetActive(false);
        CharacterImageRefresh();
        RefreshListHighlights();
    }

    /// <summary>
    /// パーティスロットクリック時
    /// </summary>
    /// <param name="index">スロット番号</param>
    public void OnPartySlotClicked(int index)
    {
        CurrentSlotIndex = index;
        int currentCharId = PartyManager.Instance.PartyMemberIds[index];

        var masterSo = registry.GetById(currentCharId);
        if (masterSo != null)
        {
            if (selectedCharacterImage != null) selectedCharacterImage.sprite = masterSo.characterBigSprite;
            _tempSelectedCharacterId = currentCharId;
        }
        else
        {
            if (selectedCharacterImage != null) selectedCharacterImage.sprite = emptySlotBigSprite;
            _tempSelectedCharacterId = NullCharacterId;
        }

        if (selectPanel != null) selectPanel.SetActive(true);
        RefreshListHighlights();
    }

    /// <summary>
    /// パーティスロット画像の更新
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
                bool isInParty = kvp.Key != NullCharacterId && partyMemberIds.Contains(kvp.Key);
                kvp.Value.SetActive(isInParty);
            }
        }
 
        foreach (var kvp in _tempSelectHighlightDict)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(kvp.Key == _tempSelectedCharacterId);
            }
        }
    }
}
