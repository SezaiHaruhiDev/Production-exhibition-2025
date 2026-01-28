using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Common;

/// <summary>
/// パーティ編成を管理（PlayerPrefsで永続化）
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] private int maxPartySize = 4;
    private int[] _partyMemberIds;

    public int MaxPartySize => maxPartySize;
    public IReadOnlyList<int> PartyMemberIds => _partyMemberIds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _partyMemberIds = new int[maxPartySize];
        for (int i = 0; i < maxPartySize; i++) _partyMemberIds[i] = -1;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
        Assert.IsTrue(maxPartySize > 0, "PartyManager: Max Party Size must be greater than 0");
    }

    /// <summary>
    /// 初期化コルーチン
    /// </summary>
    public IEnumerator InitializeCoroutine()
    {
        LoadParty();
        yield return null;
    }

    /// <summary>
    /// 指定スロットにキャラクターをセットし、保存する
    /// </summary>
    /// <param name="slotIndex">スロット番号</param>
    /// <param name="characterId">キャラクターID</param>
    public void SetMember(int slotIndex, int characterId)
    {
        if (slotIndex < 0 || slotIndex >= _partyMemberIds.Length) return;

        for (int i = 0; i < _partyMemberIds.Length; i++)
        {
            if (_partyMemberIds[i] == characterId) _partyMemberIds[i] = -1;
        }

        _partyMemberIds[slotIndex] = characterId;
        SaveParty();
    }

    /// <summary>
    /// 現在のパーティ構成を保存する
    /// </summary>
    public void SaveParty()
    {
        string saveString = string.Join(",", _partyMemberIds);
        PlayerPrefs.SetString(GameConstants.Prefs.PlayerParty, saveString);
        PlayerPrefs.Save();
    }

    private void LoadParty()
    {
        if (_partyMemberIds == null || _partyMemberIds.Length != maxPartySize)
        {
            _partyMemberIds = new int[maxPartySize];
            for (int i = 0; i < maxPartySize; i++) _partyMemberIds[i] = -1;
        }

        string saved = PlayerPrefs.GetString(GameConstants.Prefs.PlayerParty, string.Empty);
        if (!string.IsNullOrEmpty(saved))
        {
            string[] split = saved.Split(',');
            for (int i = 0; i < split.Length && i < maxPartySize; i++)
            {
                if (int.TryParse(split[i], out int id))
                {
                    _partyMemberIds[i] = id;
                }
            }
        }
    }
}