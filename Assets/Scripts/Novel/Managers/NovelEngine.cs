using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Novel.System;
using Novel.Data;
using Common;

/// <summary>
/// ノベルゲームのメイン管理クラス（シナリオ解析、テキスト表示、コマンド実行）
/// </summary>
public class NovelEngine : MonoBehaviour
{
    #region Serialized Fields
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI subText;
    [FormerlySerializedAs("AfiText")]
    [SerializeField] private TextMeshProUGUI affiliationText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject nextIcon;

    [Header("Image Switch Settings")]
    [SerializeField] private Image switchImage;
    [SerializeField] private Sprite imageA;
    [SerializeField] private Sprite imageB;

    [Header("Settings")]
    [SerializeField] private float captionSpeed = 0.05f;
    [SerializeField] private string textFile = GameConstants.Resources.ScenarioDir;

    [Header("Managers")]
    [FormerlySerializedAs("charactermanager")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ChoicesManager choicesManager;
    [SerializeField] private BGMManager bgmManager;
    [SerializeField] private SEManager seManager;
    [SerializeField] private LogManager logManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraFocusManager cameraFocusManager;
    #endregion

    #region Properties for CommandExecutor
    public CharacterManager CharacterManager => characterManager;
    public ChoicesManager ChoicesManager => choicesManager;
    public BGMManager BgmManager => bgmManager;
    public SEManager SeManager => seManager;
    public LogManager LogManager => logManager;
    public UIManager UiManager => uiManager;
    public CameraFocusManager CameraFocusManager => cameraFocusManager;
    public Image BackgroundImage => backgroundImage;
    public TextMeshProUGUI AffiliationText => affiliationText;
    public Image SwitchImage => switchImage;
    public Sprite ImageA => imageA;
    public Sprite ImageB => imageB;

    public bool SubFlag { get; set; } = false;
    public float CaptionSpeed { get => captionSpeed; set => captionSpeed = value; }
    #endregion

    #region Internal State
    private Queue<Page> _pageQueue;
    private Dictionary<string, string> labelDict = new Dictionary<string, string>();
    private bool isDelaying = false;
    private Coroutine _showCharsCoroutine;
    private TextMeshProUGUI currentOutputText => SubFlag ? subText : mainText;
    private ScenarioParser _parser;
    private CommandExecutor _executor;
    #endregion

    void Start()
    {
        _parser = new ScenarioParser();
        _executor = new CommandExecutor(this);
        Init();
    }

    private void Init()
    {
        string text = LoadTextFile(textFile);

        mainText.text = "";
        subText.text = "";
        nameText.text = "";
        affiliationText.text = "";
        titleText.text = "";

        labelDict = _parser.ParseLabelDictionary(text);

        if (labelDict.TryGetValue("start", out string firstText))
        {
            _pageQueue = _parser.ParsePages(firstText);
        }
        else
        {
             // "start" ラベルがない場合のフェイルセーフ（必要なら最初の行から開始するロジックをここに書く）
             // 現状は何もせず待機
        }

        StartCoroutine(RunScenarioLoop());
    }

    private string LoadTextFile(string fname)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fname);
        if (textAsset == null)
        {
            Debug.LogError($"Scenario file '{fname}' not found in Resources.");
            return "";
        }
        return textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private IEnumerator RunScenarioLoop()
    {
        while (true)
        {
            while (isDelaying) yield return null;

            if (_pageQueue == null || _pageQueue.Count <= 0)
            {
                yield return null;
                continue;
            }

            if (nextIcon != null) nextIcon.SetActive(false);

            Page page = _pageQueue.Dequeue();
            if (page.commands.Count > 0)
            {
                _executor.Execute(page.commands);
                continue;
            }

            // メインテキストとサブテキスト（アウトライン）の表示切り替え
            nameText.text = page.name;
            mainText.text = "";
            subText.text = "";

            currentOutputText.text = page.text;
            currentOutputText.maxVisibleCharacters = 0;

            _showCharsCoroutine = StartCoroutine(ShowChars(captionSpeed));

            if (logManager != null)
                logManager.AddLog(page.name, page.text);

            // クリック待ち（_readyForNextPageがtrueになるまで待機）
            yield return new WaitUntil(() => _readyForNextPage);
            _readyForNextPage = false;
        }
    }

    private bool _readyForNextPage = false;
    /// <summary>
    /// 指定時間待機するコルーチン（isDelayingフラグを操作）
    /// </summary>
    public IEnumerator DelayRoutine(float t)
    {
        isDelaying = true;
        yield return new WaitForSeconds(t);
        isDelaying = false;
    }

    /// <summary>
    /// 画面クリック時の処理（テキスト送り、スキップなど）
    /// </summary>
    public void OnClick()
    {
        if (isDelaying) return;

        if (nextIcon != null) nextIcon.SetActive(false);
        if (currentOutputText.maxVisibleCharacters < currentOutputText.text.Length)
        {
            if (_showCharsCoroutine != null)
            {
                StopCoroutine(_showCharsCoroutine);
                _showCharsCoroutine = null;
            }
            currentOutputText.maxVisibleCharacters = currentOutputText.text.Length;
            if (nextIcon != null) nextIcon.SetActive(true);
        }
        else
        {
            _readyForNextPage = true;
        }
    }

    private IEnumerator ShowChars(float wait)
    {
        while (currentOutputText.maxVisibleCharacters < currentOutputText.text.Length)
        {
            currentOutputText.maxVisibleCharacters++;
            yield return new WaitForSeconds(wait);
        }
        if (nextIcon != null) nextIcon.SetActive(true);
        _showCharsCoroutine = null;
    }

    /// <summary>
    /// 指定したラベルのページへジャンプする
    /// </summary>
    public void GoToLabel(string labelName)
    {
        if (labelDict.TryGetValue(labelName, out string text))
        {
            _pageQueue = _parser.ParsePages(text);
            _readyForNextPage = true;
        }
        else
        {
            Debug.LogError($"Label '{labelName}' not found.");
        }
    }

    /// <summary>
    /// スキップボタン押下時の処理（endラベルへ飛ぶ）
    /// </summary>
    public void SkipButton()
    {
        GoToLabel("end");
        _readyForNextPage = true;
    }

    /// <summary>
    /// メイン・サブテキストのフォントサイズを変更
    /// </summary>
    public void SetFontSize(float size)
    {
        mainText.fontSize = size;
        subText.fontSize = size;
    }

    /// <summary>
    /// メイン・サブテキストのフォント色を変更
    /// </summary>
    public void SetFontColor(Color col)
    {
        mainText.color = col;
        subText.color = col;
    }

    /// <summary>
    /// タイトル演出を表示する
    /// </summary>
    public void ShowTitleSequence(string sceneName)
    {
        titleText.text = sceneName;
        FadeInAll();
        StartCoroutine(DelayRoutine(3));
        StartCoroutine(TitleDelay());
    }

    private IEnumerator TitleDelay()
    {
        yield return new WaitForSeconds(2f);
        FadeOutAll();
    }

    [Header("Title UI")]
    public List<GameObject> uiItems;
    public float fadeDuration = 0.5f;

    /// <summary>
    /// タイトルUIをフェードアウトして非表示にする
    /// </summary>
    public void FadeOutAll()
    {
        foreach (var item in uiItems)
        {
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg == null) cg = item.AddComponent<CanvasGroup>();
            cg.DOFade(0f, fadeDuration).OnComplete(() => item.SetActive(false));
        }
    }

    /// <summary>
    /// タイトルUIを表示し、フェードインさせる
    /// </summary>
    public void FadeInAll()
    {
        foreach (var item in uiItems)
        {
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg == null) cg = item.AddComponent<CanvasGroup>();
            item.SetActive(true);
            cg.alpha = 0f;
            cg.DOFade(1f, fadeDuration);
        }
    }
}
