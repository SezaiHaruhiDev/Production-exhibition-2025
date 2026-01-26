using System;
using System.Collections.Generic;

namespace Novel.Data
{
    /// <summary>
    /// シナリオの1ページ（画面単位）を表すデータ。Parserによってテキストからこの形式に変換される。
    /// </summary>
    [global::System.Serializable]
    public class Page
    {
        public string name;
        public string text;
        public List<Command> commands = new List<Command>();
    }

    /// <summary>
    /// シナリオ内のコマンドを表すデータクラス
    /// </summary>
    [global::System.Serializable]
    public class Command
    {
        public string type;
        public Dictionary<string, string> parameters = new Dictionary<string, string>();
    }
}
