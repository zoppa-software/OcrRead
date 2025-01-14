using SwitchAnalyzers.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchAnalyzers
{
    /// <summary>スイッチ環境です。</summary>
    public sealed class SwitchEnvironments
    {
        /// <summary>トークンリスト。</summary>
        private readonly List<IToken> tokens;

        /// <summary>スイッチ。</summary>
        /// <summary>文字スイッチ。</summary>
        private readonly Dictionary<string, (string sw, int cnt)> switches;

        /// <summary>コンストラクタ。</summary>
        /// <param name="tokens">トークンリスト。</param>
        public SwitchEnvironments(List<IToken> tokens)
        {
            this.tokens = tokens;
            this.switches = new Dictionary<string, (string sw, int cnt)>();
        }

        /// <summary>文字スイッチを追加します。</summary>
        /// <param name="charSwitch">スイッチ文字。</param>
        /// <param name="parameterCount">パラメータ数。</param>
        /// <returns>スイッチ環境。</returns>
        public SwitchEnvironments AddSwitch(char charSwitch, int parameterCount)
        {
            var charSwitchStr = charSwitch.ToString();
            if (this.switches.ContainsKey(charSwitchStr)) {
                this.switches[charSwitchStr] = (charSwitchStr, parameterCount);
            }
            else {
                this.switches.Add(charSwitchStr, (charSwitchStr, parameterCount));
            }
            return this;
        }

        /// <summary>文字列スイッチを追加します。</summary>
        /// <param name="wordSwitch">スイッチ文字列。</param>
        /// <param name="parameterCount">パラメータ数。</param>
        /// <returns>スイッチ環境。</returns>
        public SwitchEnvironments AddSwitch(string wordSwitch, int parameterCount)
        {
            if (this.switches.ContainsKey(wordSwitch)) {
                this.switches[wordSwitch] = (wordSwitch, parameterCount);
            }
            else {
                this.switches.Add(wordSwitch, (wordSwitch, parameterCount));
            }
            return this;
        }

        /// <summary>スイッチを追加します。</summary>
        /// <param name="charSwitch">スイッチ文字。</param>
        /// <param name="wordSwitch">スイッチ文字列。</param>
        /// <returns>スイッチ環境。</returns>
        public SwitchEnvironments AddSwitch(char charSwitch, string wordSwitch, int parameterCount)
        {
            this.AddSwitch(charSwitch, parameterCount);
            this.AddSwitch(wordSwitch, parameterCount);
            return this;
        }

        /// <summary>スイッチを解析します。</summary>
        /// <returns>解析結果。</returns>
        public Result Analisys()
        {
            // スイッチリスト
            var swts = new Dictionary<string, List<List<string>>>();

            // 使用済みフラグ
            var used = new bool[this.tokens.Count];

            // トークンを解析し、スイッチとパラメータリストを作成
            for (var i = 0; i < this.tokens.Count; i++) {
                switch (this.tokens[i].TokenType) {
                    case TokenType.SwitchList:
                        foreach (var subTkn in this.tokens[i].SubToken) {
                            CreateSwitch(swts, subTkn.Contents, used, i);
                        }
                        break;

                    case TokenType.CharSwitch:
                    case TokenType.WordSwitch:
                        CreateSwitch(swts, this.tokens[i].Contents, used, i);
                        break;
                }
            }

            // 解析結果を返す
            return new Result(swts, used, this.tokens);
        }

        /// <summary>スイッチを作成します。</summary>
        /// <param name="swts">スイッチリスト。</param>
        /// <param name="name">スイッチ名。</param>
        /// <param name="used">使用済みフラグ。</param>
        /// <param name="start">読込開始位置。</param>
        private void CreateSwitch(Dictionary<string, List<List<string>>> swts, string name, bool[] used, int start)
        {
            var parameters = new List<string>();
            used[start] = true;
            (string sw, int cnt) swt;
            if (this.switches.TryGetValue(name, out swt)) {
                // パラメータを取得
                for (int j = 0; j < swt.cnt &&
                                start + 1 + j < this.tokens.Count &&
                                this.tokens[start + 1 + j].TokenType == TokenType.Identifier; ++j) {
                    var idx = start + 1 + j;
                    used[idx] = true;
                    parameters.Add(this.tokens[idx].Contents);
                }

                // スイッチリストに追加
                if (!swts.ContainsKey(swt.sw)) {
                    swts.Add(swt.sw, new List<List<string>>());
                }
                swts[swt.sw].Add(parameters);
            }
        }

        /// <summary>解析結果。</summary>
        public sealed class Result
        {
            private readonly Dictionary<string, List<List<string>>> switches;

            private readonly List<string> parameters = new List<string>();

            public Result(Dictionary<string, List<List<string>>> swts, bool[] used, List<IToken> tokens)
            {
                this.switches = swts;
                for (var i = used.Length - 1; i >= 0; --i) {
                    if (!used[i] && tokens[i].TokenType == TokenType.Identifier) {
                        this.parameters.Add(tokens[i].Contents);
                    }
                    else {
                        break;
                    }
                }
                this.parameters.Reverse();
            }

            public bool HasSwitch(char cname, string wname)
            {
                return this.HasSwitch(cname) || this.HasSwitch(wname);
            }

            public bool HasSwitch(char name)
            {
                return this.HasSwitch(name.ToString());
            }

            public bool HasSwitch(string name)
            {
                return this.switches.ContainsKey(name);
            }

            public List<List<string>> GetSwitchValue(char cname, string wname)
            {
                if (this.HasSwitch(cname)) {
                    return this.GetSwitchValue(cname);
                }
                if (this.HasSwitch(wname)) {
                    return this.GetSwitchValue(wname);
                }
                throw new KeyNotFoundException();
            }

            public List<List<string>> GetSwitchValue(char name)
            {
                return this.GetSwitchValue(name.ToString());
            }

            public List<List<string>> GetSwitchValue(string name)
            {
                return this.switches[name];
            }
        }
    }
}
