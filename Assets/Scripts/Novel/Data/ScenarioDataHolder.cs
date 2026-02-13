using UnityEngine;

namespace Novel.Data
{
    /// <summary>
    /// シーンをまたいでシナリオデータを保持するためのクラス
    /// </summary>
    public static class ScenarioDataHolder
    {
        private static string _nextScenarioName = "scenario";

        public static string NextScenarioName
        {
            get => _nextScenarioName;
            set => _nextScenarioName = value;
        }

        /// <summary>
        /// 保持しているシナリオ名を取得し、取得後はデフォルト値に戻す
        /// </summary>
        /// <returns></returns>
        public static string GetAndReset()
        {
            string name = _nextScenarioName;
            _nextScenarioName = "scenario";
            return name;
        }
    }
}
