using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// ボタンクリック時にSEを再生
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioClip _clickSE;
    [SerializeField] private string _resourcesSEName;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;

    private Button _button;
    private AudioClip _preloadedClip;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Start()
    {
        if (_clickSE != null)
        {
            _preloadedClip = _clickSE;
        }
        else if (!string.IsNullOrEmpty(_resourcesSEName))
        {
            _preloadedClip = Resources.Load<AudioClip>("se/" + _resourcesSEName);
        }
    }

    /// <summary>
    /// ボタン押下時（OnClickよりも反応を早くするため、PointerDownを使用）
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button.interactable)
        {
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (_preloadedClip != null)
        {
            SoundManager.Instance.PlaySE(_preloadedClip, _volume);
        }
    }
}
