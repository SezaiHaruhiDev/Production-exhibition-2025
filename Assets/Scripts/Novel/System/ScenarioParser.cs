using System.Collections.Generic;
using UnityEngine;
using Novel.Data;

namespace Novel.System
{
    /// <summary>
    /// シナリオテキストを解析してページとコマンドに変換
    /// </summary>
    public class ScenarioParser
    {
        private const char SEPARATE_PAGE = '&';
        private const char SEPARATE_COMMAND = '!';
        private const char COMMAND_SEPARATE_PARAM = '=';

        /// <summary>
        /// テキストからラベル（#Label-Text）を抽出し、辞書を作成する
        /// </summary>
        public Dictionary<string, string> ParseLabelDictionary(string text)
        {
            var labelDict = new Dictionary<string, string>();
            string[] chunks = text.Split('#');
            foreach (var chunk in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunk)) continue;
                int dashIndex = chunk.IndexOf('-');
                if (dashIndex < 0) continue;

                string key = chunk.Substring(0, dashIndex).Trim();
                string value = chunk.Substring(dashIndex + 1).Trim();
                labelDict[key] = value;
            }
            return labelDict;
        }

        /// <summary>
        /// テキストをページ単位（&区切り）に分割し、コマンドとテキストを解析する
        /// </summary>
        public Queue<Page> ParsePages(string text)
        {
            string[] rawPages = text.Split(SEPARATE_PAGE);
            var queue = new Queue<Page>();

            foreach (string raw in rawPages)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                if (raw[0] == SEPARATE_COMMAND)
                {
                    var cmdPage = new Page();
                    cmdPage.commands = ParseCommands(raw);
                    queue.Enqueue(cmdPage);
                }
                else
                {
                    var textPage = new Page();
                    string[] ts = raw.Split('「');
                    if (ts.Length >= 2)
                    {
                        textPage.name = ts[0];
                        string body = ts[1];
                        int lastBracket = body.LastIndexOf('」');
                        if (lastBracket >= 0) body = body.Remove(lastBracket);
                        textPage.text = body;
                        queue.Enqueue(textPage);
                    }
                }
            }
            return queue;
        }

        private List<Command> ParseCommands(string cmdLine)
        {
            var list = new List<Command>();
            // 先頭の '!' を削除してからコマンド分割（!bg=... !se=... の形式）
            string line = cmdLine.Remove(0, 1);
            string[] cmds = line.Split(SEPARATE_COMMAND);

            foreach (string c in cmds)
            {
                if (string.IsNullOrWhiteSpace(c)) continue;
                // "type=param" の形式で分割
                string[] parts = c.Split(COMMAND_SEPARATE_PARAM);
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
                        cmd.parameters[kvSplit[0].Trim()] = "";
                    }
                }
                list.Add(cmd);
            }
            return list;
        }
    }
}
