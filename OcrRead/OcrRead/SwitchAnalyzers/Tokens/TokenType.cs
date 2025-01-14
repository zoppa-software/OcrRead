namespace SwitchAnalyzers.Tokens
{
    public enum TokenType
    {
        /// <summary>不明なトークン。</summary>
        Unknown,

        /// <summary>識別子。</summary>
        Identifier,

        /// <summary>空白。</summary>
        WhiteSpace,

        /// <summary>文字スイッチ。</summary>
        CharSwitch,

        /// <summary>文字列スイッチ。</summary>
        WordSwitch,

        /// <summary>スイッチリスト。</summary>
        SwitchList,
    }
}
