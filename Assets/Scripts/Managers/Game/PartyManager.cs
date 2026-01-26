using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using Common;

/// <summary>
/// パーティ編成を管理（PlayerPrefsで永続化）
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] public int maxPartySize = 4;
    private int[] _partyMemberIds;

    public IReadOnlyList<int> PartyMemberIds => _partyMemberIds;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ロード完了前のアクセスエラーを防ぐために初期化
        _partyMemberIds = new int[maxPartySize];
        for (int i = 0; i < maxPartySize; i++) _partyMemberIds[i] = -1;

        // DontDestroyOnLoadはルートオブジェクトにしか効かないため、親から分離する
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 初期化コルーチン。データのロードを行う。
    /// </summary>
    public IEnumerator InitializeCoroutine()
    {
        LoadParty();
        yield return null;
    }

    /// <summary>
    /// 指定スロットにキャラクターをセットし、保存する。
    /// </summary>
    /// <param name="slotIndex">スロット番号</param>
    /// <param name="characterId">キャラクターID</param>
    public void SetMember(int slotIndex, int characterId)
    {
        if (slotIndex < 0 || slotIndex >= _partyMemberIds.Length) return;

        // 重複防止：他のスロットに同じキャラがいたらクリア
        for (int i = 0; i < _partyMemberIds.Length; i++)
        {
            if (_partyMemberIds[i] == characterId) _partyMemberIds[i] = -1;
        }

        _partyMemberIds[slotIndex] = characterId;
        SaveParty();
    }

    /// <summary>
    /// 現在のパーティ構成をPlayerPrefsに保存する。
    /// </summary>
    public void SaveParty()
    {
        string saveString = string.Join(",", _partyMemberIds);
        PlayerPrefs.SetString(GameConstants.Prefs.PlayerParty, saveString);
    }

    private void LoadParty()
    {
        if (_partyMemberIds == null || _partyMemberIds.Length != maxPartySize)
        {
            _partyMemberIds = new int[maxPartySize];
            for (int i = 0; i < maxPartySize; i++) _partyMemberIds[i] = -1;
        }

        string saved = PlayerPrefs.GetString(GameConstants.Prefs.PlayerParty, "");
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