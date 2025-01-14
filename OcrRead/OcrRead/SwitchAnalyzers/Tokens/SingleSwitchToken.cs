using System.Collections.Generic;

namespace SwitchAnalyzers.Tokens
{
    /// <summary>単一文字のスイッチトークンです。</summary>
    internal class SingleSwitchToken : IToken
    {
        /// <summary>トークンの内容を取得します。</summary>
        public string Contents { get; }

        /// <summary>トークンの位置を取得します。</summary>
        public int Position { get; }

        /// <summary>トークンのソースを取得します。</summary>
        public string Source { get; }

        /// <summary>トークンの種類を取得します。</summary>
        public TokenType TokenType => TokenType.CharSwitch;

        /// <summary>子トークンのリストを取得します。</summary>
        public List<IToken> SubToken => new List<IToken>();

        /// <summary>コンストラクタ。</summary>
        /// <param name="c">スイッチ文字。</param>
        /// <param name="pos">位置。</param>
        /// <param name="src">元の文字列。</param>
        public SingleSwitchToken(char c, int pos, string src)
        {
            this.Contents = c.ToString();
            this.Position = pos;
            this.Source = src;
        }

        /// <summary>文字列表現を取得します。</summary>
        /// <returns>文字列表現。</returns>
        override public string ToString()
        {
            return $"SingleSwitch: {this.Contents}";
        }
    }
}
