using System.Collections.Generic;
using System.Linq;

namespace SwitchAnalyzers.Tokens
{
    /// <summary>単一文字のスイッチリストトークンです。</summary>
    internal class SingleSwitchListToken : IToken
    {
        /// <summary>トークンの内容を取得します。</summary>
        public string Contents { get; }

        /// <summary>トークンの位置を取得します。</summary>
        public int Position { get; }

        /// <summary>トークンのソースを取得します。</summary>
        public string Source { get; }

        /// <summary>トークンの種類を取得します。</summary>
        public TokenType TokenType => TokenType.SwitchList;

        /// <summary>子トークンのリストを取得します。</summary>
        public List<IToken> SubToken => new List<IToken>();

        /// <summary>コンストラクタ。</summary>
        /// <param name="subTokens">サブトークンリスト。</param>
        public SingleSwitchListToken(IEnumerable<SingleSwitchToken> subTokens)
        {
            this.Contents = string.Join("", subTokens.Select(t => t.Contents));
            this.Position = subTokens.First().Position;
            this.Source = subTokens.First().Source;
            this.SubToken.AddRange(subTokens);
        }

        /// <summary>文字列表現を取得します。</summary>
        /// <returns>文字列表現。</returns>
        override public string ToString()
        {
            var subTokenStr = string.Join(", ", this.SubToken.Select(t => t.ToString()));
            return $"SingleSwitchList: {subTokenStr}";
        }
    }
}

