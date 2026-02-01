using System.Collections;
using UnityEngine;

/// <summary>
/// 敵の行動ロジックを定義する基底クラス (Strategy Pattern)
/// </summary>
public abstract class EnemyLogicSO : ScriptableObject
{
    /// <summary>
    /// 敵のターンを実行する
    /// </summary>
    /// <param name="actor">行動する敵ユニット</param>
    /// <param name="turnManager">ターン管理クラス（盤面情報へのアクセス用）</param>
    public abstract IEnumerator ExecuteTurn(BattleUnit actor, TurnManager turnManager);
}
