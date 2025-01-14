using System.Collections.Generic;

namespace SwitchAnalyzers.Tokens
{
    /// <summary>名前付きのスイッチトークンです。</summary>
    internal class NameSwitchToken : IToken
    {
        /// <summary>トークンの内容を取得します。</summary>
        public string Contents { get; }

        /// <summary>トークンの位置を取得します。</summary>
        public int Position { get; }

        /// <summary>トークンのソースを取得します。</summary>
        public string Source { get; }

        /// <summary>トークンの種類を取得します。</summary>
        public TokenType TokenType => TokenType.WordSwitch;

        /// <summary>子トークンのリストを取得します。</summary>
        public List<IToken> SubToken => new List<IToken>();

        /// <summary>コンストラクタ。</summary>
        /// <param name="pos">位置。</param>
        /// <param name="src">元の文字列。</param>
        public NameSwitchToken(int pos, string src)
        {
            this.Contents = src.Substring(2);
            this.Position = pos;
            this.Source = src;
        }

        /// <summary>文字列表現を取得します。</summary>
        /// <returns>文字列表現。</returns>
        override public string ToString()
        {
            return $"NameSwitch: {this.Contents}";
        }
    }
}