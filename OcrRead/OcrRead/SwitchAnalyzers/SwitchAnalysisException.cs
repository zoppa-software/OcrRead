using System;

namespace SwitchAnalyzers
{
    /// <summary>スイッチ分析器例外です。</summary>
    public sealed class SwitchAnalysisException : Exception
    {
        /// <summary>コンストラクタ。</summary>
        /// <param name="message">例外メッセージ。</param>
        public SwitchAnalysisException(string message) : 
            base(message)
        {
        }

        /// <summary>コンストラクタ。</summary>
        /// <param name="message">例外メッセージ。</param>
        /// <param name="innerException">内部例外。</param>
        public SwitchAnalysisException(string message, Exception innerException) : 
            base(message, innerException)
        {
        }
    }
}
