using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Novel.Data;
using Novel.Managers;
using Common;

namespace Novel.System
{
    /// <summary>
    /// クリックを受け取ってテキストを進めつつ、
    /// フェードアウトしてノベルシーンへ遷移する処理を管理するスクリプト
    /// </summary>
    public class NovelSceneLoader : MonoBehaviour
    {
        [Header("Transition Settings")]
        [Tooltip("次に読み込むシナリオファイル名（拡張子不要）")]
        [SerializeField] private string targetScenarioName = "scenario";
        
        [Tooltip("フェードアウトにかける秒数")]
        [SerializeField] private float fadeDuration = 1.0f;
        
        [Tooltip("画面全体を覆うCanvasGroup（黒画像などを想定）")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        private NovelEngine _engine;

        private void Start()
        {
            // シーン内のNovelEngineを探しておく
            _engine = Object.FindFirstObjectByType<NovelEngine>();
        }

        /// <summary>
        /// 全画面ボタンやStartボタンから呼び出すメソッド
        /// </summary>
        public void LoadWithFade()
        {
            // もしノベルエンジンがあれば、テキスト送り（OnClick）を1回呼び出す
            if (_engine != null)
            {
                _engine.OnClick();
            }

            // 即座に画面を暗くし、1秒後に遷移する
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.gameObject.SetActive(true);
                fadeCanvasGroup.alpha = 1f; // 即座に真っ暗に
                
                // 1秒待機してから遷移
                DOVirtual.DelayedCall(1f, () =>
                {
                    PerformSceneChange();
                });
            }
            else
            {
                PerformSceneChange();
            }
        }

        private void PerformSceneChange()
        {
            // 次のシナリオ名を予約してシーン遷移
            ScenarioDataHolder.NextScenarioName = targetScenarioName;
            SceneManager.LoadScene(GameConstants.Scenes.Novel);
        }
    }
}
