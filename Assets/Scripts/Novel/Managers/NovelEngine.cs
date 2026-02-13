using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Novel.System;
using Novel.Data;
using Novel.Managers;
using UnityEngine.Assertions;
using Common;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    [SerializeField] private string textFile = "texts/scenario";

    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoOutput;

    [Header("Managers")]
    [FormerlySerializedAs("charactermanager")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ChoicesManager choicesManager;
    [SerializeField] private BGMManager bgmManager;
    [SerializeField] private SEManager seManager;
    [SerializeField] private LogManager logManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraFocusManager cameraFocusManager;
    [SerializeField] private FlagManager flagManager;
    #endregion

    #region Properties for CommandExecutor
    public CharacterManager CharacterManager => characterManager;
    public ChoicesManager ChoicesManager => choicesManager;
    public BGMManager BgmManager => bgmManager;
    public SEManager SeManager => seManager;
    public LogManager LogManager => logManager;
    public UIManager UiManager => uiManager;
    public CameraFocusManager CameraFocusManager => cameraFocusManager;
    public FlagManager FlagManager => flagManager;
    public Image BackgroundImage => backgroundImage;
    public TextMeshProUGUI AffiliationText => affiliationText;
    public Image SwitchImage => switchImage;
    public Sprite ImageA => imageA;
    public Sprite ImageB => imageB;

    public Image BmImage => backgroundImage;
    public VideoPlayer VideoPlayer => videoPlayer;
    public RawImage VideoOutput => videoOutput;
    public bool IsDelaying => isDelaying;
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

    private void Awake()
    {
        Assert.IsNotNull(mainText, "NovelEngine: mainText is not assigned!");
        Assert.IsNotNull(subText, "NovelEngine: subText is not assigned!");
        Assert.IsNotNull(uiManager, "NovelEngine: uiManager is not assigned!");
        Assert.IsNotNull(characterManager, "NovelEngine: characterManager is not assigned!");

        if (flagManager == null)
        {
            flagManager = GetComponentInChildren<FlagManager>();
            if (flagManager == null) 
            {
                flagManager = gameObject.AddComponent<FlagManager>();
            }
        }
    }

    void Start()
    {
        _parser = new ScenarioParser();
        _executor = new CommandExecutor(this);
        
        // 外部から指定されたシナリオ名があればそれを使う
        textFile = "texts/" + ScenarioDataHolder.GetAndReset();
        
        Init();
    }

    /// <summary>
    /// シナリオの初期化と再生開始を行う
    /// </summary>
    private void Init()
    {
        string text = LoadTextFile(textFile);

        // 基本UIの初期化
        mainText.text = string.Empty;
        subText.text = string.Empty;
        nameText.text = string.Empty;
        affiliationText.text = string.Empty;
        titleText.text = string.Empty;

        // ラベル辞書の解析
        labelDict = _parser.ParseLabelDictionary(text);

        // 開始位置の決定 (定数を使用)
        if (labelDict.TryGetValue(GameConstants.ScenarioLabels.Start, out string firstText))
        {
            _pageQueue = _parser.ParsePages(firstText);
        }
        
        StartCoroutine(RunScenarioLoop());
    }

    private string LoadTextFile(string fname)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fname);
        if (textAsset == null)
        {
            Debug.LogError($"Scenario file '{fname}' not found in Resources folder.");
            return string.Empty;
        }
        return textAsset.text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// 新しいシナリオファイルを読み込み、再生を開始する
    /// </summary>
    public void LoadNewScenario(string fileName)
    {
        StopAllCoroutines();
        textFile = "texts/" + fileName;
        Init();
    }

    /// <summary>
    /// メインシナリオループ：ページを一つずつ取り出して実行・表示する
    /// </summary>
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
                yield return StartCoroutine(_executor.ExecuteCoroutine(page.commands));
                continue;
            }

            nameText.text = page.name;
            mainText.text = string.Empty;
            subText.text = string.Empty;

            currentOutputText.text = page.text ?? string.Empty;
            currentOutputText.maxVisibleCharacters = 0;

            _showCharsCoroutine = StartCoroutine(ShowChars(captionSpeed));

            if (logManager != null)
                logManager.AddLog(page.name, page.text);

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
    /// 画面クリック時の処理（テキスト送り、ページ送り、スキップ）
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
    /// <param name="labelName">ジャンプ先ラベル名</param>
    public void GoToLabel(string labelName)
    {
        if (labelDict.TryGetValue(labelName, out string text))
        {
            _pageQueue = _parser.ParsePages(text);
            _readyForNextPage = true;
        }
        else
        {
            Debug.LogError($"Label '{labelName}' not found in scenario.");
        }
    }

    /// <summary>
    /// スキップボタン押下時の処理（endラベルへ強制遷移）
    /// </summary>
    public void SkipButton()
    {
        GoToLabel(GameConstants.ScenarioLabels.End);
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
    /// タイトル演出（タイトルロゴ、フェード等）を表示する
    /// </summary>
    public void ShowTitleSequence(string sceneName)
    {
        titleText.text = sceneName;
        if (uiManager != null)
        {
            uiManager.FadeInTitleUI();
            StartCoroutine(DelayRoutine(GameConstants.UI.TitleStayDuration));
            StartCoroutine(TitleDelay());
        }
    }

    private IEnumerator TitleDelay()
    {
        yield return new WaitForSeconds(2f); 
        if (uiManager != null)
        {
            uiManager.FadeOutTitleUI();
        }
    }

    /// <summary>
    /// 動画を再生する
    /// </summary>
    public IEnumerator PlayVideoRoutine(string videoName)
    {
        if (videoPlayer == null || videoOutput == null)
        {
            Debug.LogError("VideoPlayer or videoOutput is not assigned to NovelEngine.");
            yield break;
        }

        // 操作とシナリオ進行をロック
        isDelaying = true;
        if (nextIcon != null) nextIcon.SetActive(false);
        videoOutput.gameObject.SetActive(true);

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGLの場合はURL指定。スラッシュを明示的に使用
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.streamingAssetsPath + "/videos/" + videoName + ".mp4";
#else
        // 通常はResourcesからロードを試みる
        Debug.Log($"Attempting to load video from Resources: videos/{videoName}");
        VideoClip clip = Resources.Load<VideoClip>("videos/" + videoName);
        if (clip != null)
        {
            Debug.Log("VideoClip loaded successfully from Resources.");
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = clip;
        }
        else
        {
            Debug.LogWarning("VideoClip not found in Resources. Falling back to StreamingAssets URL.");
            // Resourcesにない場合はStreamingAssetsからのURL再生を試みる
            videoPlayer.source = VideoSource.Url;
            string path = Application.streamingAssetsPath + "/videos/" + videoName;
            
            // エディタ（Mac）なら .mov も試せるようにする
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                videoPlayer.url = path + ".mov";
            }
            else
            {
                videoPlayer.url = path + ".mp4";
            }
        }
#endif

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // 縦横比の自動調整
        if (videoOutput.TryGetComponent<UnityEngine.UI.AspectRatioFitter>(out var fitter))
        {
            fitter.aspectRatio = (float)videoPlayer.width / (float)videoPlayer.height;
        }

        videoPlayer.Play();
        
        // 再生終了まで待機
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        videoOutput.gameObject.SetActive(false);
        isDelaying = false;
    }
}
