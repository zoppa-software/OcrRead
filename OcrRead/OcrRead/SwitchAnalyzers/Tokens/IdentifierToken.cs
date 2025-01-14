using System.Collections.Generic;

namespace SwitchAnalyzers.Tokens
{
    /// <summary>識別子トークンです。</summary>
    internal class IdentifierToken : IToken
    {
        /// <summary>トークンの内容を取得します。</summary>
        public string Contents { get; }

        /// <summary>子トークンのリストを取得します。</summary>
        public List<IToken> SubToken => new List<IToken>();

        /// <summary>トークンの位置を取得します。</summary>
        public int Position { get; }

        /// <summary>トークンのソースを取得します。</summary>
        public string Source { get; }

        /// <summary>トークンの種類を取得します。</summary>
        public TokenType TokenType => TokenType.Identifier;

        /// <summary>コンストラクタ。</summary>
        /// <param name="pos">位置。</param>
        /// <param name="src">元の文字列。</param>
        public IdentifierToken(string ident, int pos, string src)
        {
            this.Contents = ident;
            this.Position = pos;
            this.Source = src;
        }

        /// <summary>文字列表現を取得します。</summary>
        /// <returns>文字列表現。</returns>
        override public string ToString()
        {
            return $"Identifier: {this.Contents}";
        }
    }
}