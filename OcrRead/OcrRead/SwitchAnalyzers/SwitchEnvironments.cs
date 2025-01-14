using SwitchAnalyzers.Tokens;
using System.Collections.Generic;

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
            /// <summary>スイッチリスト。</summary>
            private readonly Dictionary<string, List<List<string>>> switches;

            /// <summary>パラメータリスト。</summary>
            private readonly List<string> parameters = new List<string>();

            /// <summary>コンストラクタ。</summary>
            /// <param name="swts">スイッチリスト。</param>
            /// <param name="used">使用済みフラグ。</param>
            /// <param name="tokens">トークンリスト。</param>
            public Result(Dictionary<string, List<List<string>>> swts, bool[] used, List<IToken> tokens)
            {
                // スイッチリストを設定
                this.switches = swts;

                // パラメータリストを設定
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

            /// <summary>スイッチが存在するかどうかを取得します。</summary>
            /// <param name="cname">単一文字スイッチ。</param>
            /// <param name="wname">文字列スイッチ。</param>
            /// <returns>存在したら真。</returns>
            public bool HasSwitch(char cname, string wname)
            {
                return this.HasSwitch(cname) || this.HasSwitch(wname);
            }

            /// <summary>スイッチが存在するかどうかを取得します。</summary>
            /// <param name="name">単一文字スイッチ。</param>
            /// <returns>存在したら真。</returns>
            public bool HasSwitch(char name)
            {
                return this.HasSwitch(name.ToString());
            }

            /// <summary>スイッチが存在するかどうかを取得します。</summary>
            /// <param name="name">文字列スイッチ。</param>
            /// <returns>存在したら真。</returns>
            public bool HasSwitch(string name)
            {
                return this.switches.ContainsKey(name);
            }

            /// <summary>スイッチの値を取得します。</summary>
            /// <param name="cname">単一文字スイッチ。</param>
            /// <param name="wname">文字列スイッチ。</param>
            /// <returns>スイッチの値</returns>
            /// <exception cref="KeyNotFoundException">スイッチ無しの場合のエラー。</exception>
            public List<List<string>> GetSwitchValue(char cname, string wname)
            {
                if (this.HasSwitch(cname)) {
                    return this.GetSwitchValue(cname);
                }
                if (this.HasSwitch(wname)) {
                    return this.GetSwitchValue(wname);
                }
                throw new KeyNotFoundException("指定のスイッチがありません。");
            }

            /// <summary>スイッチの値を取得します。</summary>
            /// <param name="name">単一文字スイッチ。</param>
            /// <returns>スイッチの値。</returns>
            public List<List<string>> GetSwitchValue(char name)
            {
                return this.GetSwitchValue(name.ToString());
            }

            /// <summary>スイッチの値を取得します。</summary>
            /// <param name="name">文字列スイッチ。</param>
            /// <returns>スイッチの値。</returns>
            public List<List<string>> GetSwitchValue(string name)
            {
                return this.switches[name];
            }

            /// <summary>パラメータリストを取得します。</summary>
            public List<string> Parameters => new List<string>(this.parameters);
        }
    }
}
