using System.Collections.Generic;
using UnityEngine;

namespace Novel.Managers
{
    /// <summary>
    /// フラグ（通過したラベルや選択肢）を管理するクラス
    /// </summary>
    public class FlagManager : MonoBehaviour
    {
        private const string PrefKey = "ScenarioFlags";
        private HashSet<string> _flags = new HashSet<string>();

        private void Awake()
        {
            LoadFlags();
        }

        /// <summary>
        /// フラグを立てる
        /// </summary>
        public void SetFlag(string key)
        {
            if (!_flags.Contains(key))
            {
                _flags.Add(key);
                Debug.Log($"Flag Set: {key}");
                SaveFlags();
            }
        }

        /// <summary>
        /// フラグが立っているか確認
        /// </summary>
        public bool HasFlag(string key)
        {
            return _flags.Contains(key);
        }

        /// <summary>
        /// 指定された全てのフラグが立っているか確認
        /// </summary>
        public bool CheckAll(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (!HasFlag(key)) return false;
            }
            return true;
        }

        /// <summary>
        /// 全フラグをリセット
        /// </summary>
        public void ResetAll()
        {
            _flags.Clear();
            PlayerPrefs.DeleteKey(PrefKey);
            PlayerPrefs.Save();
        }

        private void SaveFlags()
        {
            string s = string.Join(",", _flags);
            PlayerPrefs.SetString(PrefKey, s);
            PlayerPrefs.Save();
        }

        private void LoadFlags()
        {
            _flags.Clear();
            if (PlayerPrefs.HasKey(PrefKey))
            {
                var s = PlayerPrefs.GetString(PrefKey);
                if (!string.IsNullOrEmpty(s))
                {
                    string[] parts = s.Split(',');
                    foreach (var p in parts) _flags.Add(p);
                }
            }
        }
    }
}
