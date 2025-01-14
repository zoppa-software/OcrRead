using System.Collections.Generic;

namespace SwitchAnalyzers.Tokens
{
    /// <summary>トークンのインターフェースです。</summary>
    public interface IToken
    {
        /// <summary>トークンの内容を取得します。</summary>
        string Contents { get; }

        /// <summary>子トークンのリストを取得します。</summary>
        List<IToken> SubToken { get; }

        /// <summary>トークンの位置を取得します。</summary>
        int Position { get; }

        /// <summary>トークンのソースを取得します。</summary>
        string Source { get; }

        /// <summary>トークンの種類を取得します。</summary>
        TokenType TokenType { get; }
    }
}
