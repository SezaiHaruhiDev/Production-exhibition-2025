using UnityEngine;
/// <summary>
/// 戦闘画面上でキャラクターの見た目（2.5D）とデータを管理する実体クラス
/// </summary>
public class BattleUnit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public UnitCharacter Data { get; private set; }

    /// <summary>
    /// ユニットの初期設定を行い、指定された画像を表示する
    /// </summary>
    /// <param name="data">戦闘で使用するステータスデータ</param>
    /// <param name="sprite">表示するキャラクター画像</param>
    public void Setup(UnitCharacter data, Sprite sprite)
    {
        this.Data = data;
        if (_spriteRenderer != null && sprite != null)
        {
            _spriteRenderer.sprite = sprite;
        }
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
