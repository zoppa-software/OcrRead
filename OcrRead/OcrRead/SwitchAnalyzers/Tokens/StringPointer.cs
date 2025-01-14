namespace SwitchAnalyzers.Tokens
{
    /// <summary>文字列ポインタです。</summary>
    public sealed class StringPointer
    {
        /// <summary>対象文字列。</summary>
        private readonly string text;

        /// <summary>現在の位置。</summary>
        private int index;

        /// <summary>コンストラクタ。</summary>
        public StringPointer(string text)
        {
            this.text = text;
            this.index = -1;
        }

        /// <summary>現在の文字を取得します。</summary>
        public char Current => this.text[this.index];

        /// <summary>現在の位置を取得します。</summary>
        public int Position => this.index;

        public bool IsEnd => this.index >= this.text.Length;

        /// <summary>次の文字に進めます。</summary>
        /// <returns>次の文字があれば真。</returns>
        public bool MoveNext()
        {
            this.index++;
            return this.IsEnd;
        }

        /// <summary>指定の文字に進めます。</summary>
        /// <returns>文字があれば真。</returns>
        public bool Move(int skip)
        {
            this.index += skip;
            return this.IsEnd;
        }

        /// <summary>現在位置より指定した数だけ先の文字を取得します。</summary>
        /// <param name="nest">指定数。</param>
        /// <returns>文字。</returns>
        public char GetNest(int nest)
        {
            return (this.index + nest < this.text.Length ? this.text[this.index + nest] : '\0');
        }

        /// <summary>指定した位置から指定した文字数の文字列を取得します。</summary>
        /// <param name="start">開始位置。</param>
        /// <param name="count">文字数。</param>
        /// <returns>文字列。</returns>
        public string SubString(int start, int count)
        {
            return this.text.Substring(start, count);
        }
    }
}
