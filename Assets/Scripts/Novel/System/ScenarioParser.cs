using System.Collections.Generic;
using Novel.Data;

namespace Novel.System
{
    /// <summary>
    /// シナリオテキストを解析してページとコマンドに変換
    /// </summary>
    public class ScenarioParser
    {
        private const char SeparatorPage = '&';
        private const char SeparatorCommand = '!';
        private const char SeparatorParam = '=';
        private const char SeparatorLabel = '#';
        private const char SeparatorDash = '-';

        /// <summary>
        /// テキストからラベル（#Label-Text）を抽出
        /// </summary>
        public Dictionary<string, string> ParseLabelDictionary(string text)
        {
            var labelDict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(text)) return labelDict;

            string[] chunks = text.Split(SeparatorLabel);
            foreach (var chunk in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunk)) continue;
                int dashIndex = chunk.IndexOf(SeparatorDash);
                if (dashIndex < 0) continue;

                string key = chunk.Substring(0, dashIndex).Trim();
                string value = chunk.Substring(dashIndex + 1).Trim();
                labelDict[key] = value;
            }
            return labelDict;
        }

        /// <summary>
        /// テキストをページ単位に分割し解析
        /// </summary>
        public Queue<Page> ParsePages(string text)
        {
            var queue = new Queue<Page>();
            if (string.IsNullOrEmpty(text)) return queue;

            string[] rawPages = text.Split(SeparatorPage);

            foreach (string raw in rawPages)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                if (raw.TrimStart()[0] == SeparatorCommand)
                {
                    var cmdPage = new Page { commands = ParseCommands(raw) };
                    queue.Enqueue(cmdPage);
                }
                else
                {
                    ParseTextPage(raw, queue);
                }
            }
            return queue;
        }

        private void ParseTextPage(string raw, Queue<Page> queue)
        {
            var textPage = new Page();
            string[] ts = raw.Split('「');
            if (ts.Length >= 2)
            {
                textPage.name = ts[0].Trim();
                string body = ts[1];
                int lastBracket = body.LastIndexOf('」');
                if (lastBracket >= 0) body = body.Remove(lastBracket);
                textPage.text = body;
                queue.Enqueue(textPage);
            }
            else
            {
                 // 名前なし（地の文）のケースも想定する場合ここを拡張
            }
        }

        private List<Command> ParseCommands(string cmdLine)
        {
            var list = new List<Command>();
            string line = cmdLine.Trim().TrimStart(SeparatorCommand);
            string[] cmds = line.Split(SeparatorCommand);

            foreach (string c in cmds)
            {
                if (string.IsNullOrWhiteSpace(c)) continue;

                string[] parts = c.Split(SeparatorParam);
                if (parts.Length < 2) continue;

                var cmd = new Command();
                cmd.type = parts[0].Trim();

                string paramString = parts[1].Trim().Trim('"');
                string[] kvPairs = paramString.Split(',');
                foreach (string kv in kvPairs)
                {
                    string[] kvSplit = kv.Split(':');
                    if (kvSplit.Length == 2)
                    {
                        cmd.parameters[kvSplit[0].Trim()] = kvSplit[1].Trim();
                    }
                    else
                    {
                        cmd.parameters[kvSplit[0].Trim()] = string.Empty;
                    }
                }
                list.Add(cmd);
            }
            return list;
        }
    }
}
