using SwitchAnalyzers.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwitchAnalyzers
{
    /// <summary>スイッチ分析器。</summary>
    public static class SwitchParser
    {
        /// <summary>環境を生成します。</summary>
        public static SwitchEnvironments Create()
        {
            var tokens = SplitToken(Environment.CommandLine);
            return new SwitchEnvironments(tokens);
        }

        /// <summary>トークンに分割します。</summary>
        /// <param name="commandLine">コマンドライン。</param>
        /// <returns>トークンリスト。</returns>
        private static List<IToken> SplitToken(string commandLine)
        {
            var tokens = new List<IToken>();

            var reader = new StringPointer(commandLine);
            while (!reader.MoveNext()) {
                var c = reader.Current;

                if (!char.IsWhiteSpace(c)) {
                    switch (c) {
                        case '-':
                            if (reader.GetNest(1) == '-') {
                                tokens.AddRange(CreateSwitchToken(reader));
                            }
                            else {
                                tokens.AddRange(CreateSingleSwitchToken(reader));
                            }
                            break;

                        case '/':
                            tokens.AddRange(CreateSingleSwitchToken(reader));
                            break;

                        case '\'':
                            tokens.AddRange(CreateBucketIdentifierToken(reader, '\''));
                            break;

                        case '"':
                            tokens.AddRange(CreateBucketIdentifierToken(reader, '"'));
                            break;

                        case '{':
                            tokens.AddRange(CreateBracesIdentifierToken(reader));
                            break;

                        default:
                            tokens.AddRange(CreateIdentifierToken(reader));
                            break;
                    }
                }
            }

            return tokens;
        }

        /// <summary>スイッチトークンを生成します。</summary>
        /// <param name="reader">文字列ポインタ。</param>
        /// <returns>トークンリスト。</returns>
        private static IEnumerable<IToken> CreateSwitchToken(StringPointer reader)
        {
            var start = reader.Position;

            // スペース、:が出るまで読み込む
            reader.MoveNext();
            while (!reader.MoveNext()) {
                var c = reader.Current;
                if (char.IsWhiteSpace(c) || c == ':') {
                    break;
                }
            }

            // トークンを生成
            var result = new List<IToken>();
            if (reader.Position > start) {
                result.Add(new NameSwitchToken(start, reader.SubString(start, reader.Position - start)));
            }
            return result;
        }

        /// <summary>単一文字のスイッチトークンを生成します。</summary>
        /// <param name="reader">文字列ポインタ。</param>
        /// <returns>トークンリスト。</returns>
        private static IEnumerable<IToken> CreateSingleSwitchToken(StringPointer reader)
        {
            var chars = new List<char>();
            var start = reader.Position;

            // スペースが出るまで読み込む
            while (!reader.MoveNext()) {
                var c = reader.Current;

                if (!char.IsWhiteSpace(c)) {
                    chars.Add(c);
                }
                else {
                    break;
                }
            }

            // トークンを生成
            var result = new List<IToken>();
            if (chars.Count > 1) {
                result.Add(new SingleSwitchListToken(chars.Select(c => new SingleSwitchToken(c, start, reader.SubString(start, chars.Count + 1)))));
            }
            else if (chars.Count > 0) {
                result.Add(new SingleSwitchToken(chars[0], start, reader.SubString(start, chars.Count + 1)));
            }
            return result;
        }

        /// <summary>識別子トークンを生成します（ダブルコーテーション、シングルコーテーション）</summary>
        /// <param name="reader">文字列ポインタ。</param>
        /// <param name="bktChar">使用したクウォート。</param>
        /// <returns>トークンリスト。</returns>
        /// <exception cref="SwitchAnalysisException">解析エラー例外。</exception>
        private static IEnumerable<IToken> CreateBucketIdentifierToken(StringPointer reader, char bktChar)
        {
            var buf = new StringBuilder();
            var start = reader.Position;
            var closed = false;

            // 閉じるが出るまで読み込む
            while (!reader.MoveNext()) {
                var c = reader.Current;

                if (c == '\\' && reader.GetNest(1) == bktChar) {
                    buf.Append(bktChar);
                    reader.Move(2);
                }
                else if (c == bktChar && reader.GetNest(1) == bktChar) {
                    buf.Append(bktChar);
                    reader.Move(2);
                }
                else if (c == bktChar) {
                    closed = true;
                    break;
                }
                else {
                    buf.Append(c);
                }
            }

            reader.MoveNext();

            // トークンを生成
            var result = new List<IToken>();
            if (closed) {
                result.Add(new IdentifierToken(buf.ToString(), start, reader.SubString(start, buf.Length + 2)));
            }
            else {
                throw new SwitchAnalysisException($"文字列リテラルが閉じられていません。:{reader.SubString(start, buf.Length + 1)}");
            }
            return result;
        }

        /// <summary>識別子トークンを生成します。</summary>
        /// <param name="reader">文字列ポインタ。</param>
        /// <returns>トークンリスト。</returns>
        private static IEnumerable<IToken> CreateIdentifierToken(StringPointer reader)
        {
            var buf = new StringBuilder();
            var start = reader.Position;

            // スペースが出るまで読み込む
            while (!reader.MoveNext()) {
                var c = reader.Current;

                if (char.IsWhiteSpace(c)) {
                    break;
                }
            }

            // トークンを生成
            var result = new List<IToken>();
            if (reader.Position > start) {
                var str = reader.SubString(start, reader.Position - start);
                result.Add(new IdentifierToken(str, start, str));
            }
            return result;
        }

        /// <summary>識別子トークンを生成します（中括弧）</summary>
        /// <param name="reader">文字列ポインタ。</param>
        /// <returns>トークンリスト。</returns>
        /// <exception cref="SwitchAnalysisException">解析エラー例外。</exception>
        private static IEnumerable<IToken> CreateBracesIdentifierToken(StringPointer reader)
        {
            var buf = new StringBuilder();
            var start = reader.Position;
            var closed = false;
            var nest = 1;

            // 閉じるが出るまで読み込む
            while (!reader.MoveNext()) {
                var c = reader.Current;

                if (c == '\\' && (reader.GetNest(1) == '{' || reader.GetNest(1) == '}')) {
                    buf.Append(reader.GetNest(1));
                    reader.Move(2);
                }
                else if (c == '{') {
                    nest++;
                    buf.Append(c);
                }
                else if (c == '}') {
                    nest--;
                    if (nest == 0) {
                        closed = true;
                        break;
                    }
                    buf.Append(c);
                }
                else {
                    buf.Append(c);
                }
            }

            reader.MoveNext();

            // トークンを生成
            var result = new List<IToken>();
            if (closed) {
                result.Add(new IdentifierToken(buf.ToString(), start, reader.SubString(start, buf.Length + 2)));
            }
            else {
                throw new SwitchAnalysisException($"文字列リテラルが閉じられていません。:{reader.SubString(start, buf.Length + 1)}");
            }
            return result;
        }
    }
}
