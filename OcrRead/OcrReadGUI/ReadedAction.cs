using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OcrReadGUI
{
    /// <summary>読み取り後、アクションクラスです。</summary>
    public static class ReadedAction
    {
        static string cs = @"
using System;
using System.IO;
using System.Windows.Forms;
public class ReadedAction {{
    public static void Main(FileInfo target, params string[] readed) {{
{0}
    }}
}}";

        /// <summary>アクションを実行します。</summary>
        /// <param name="actionPath">アクションファイル。</param>
        /// <param name="target">読み取り対象ファイル。</param>
        /// <param name="readed">読み取った文字列リスト。</param>
        public static void Run(string actionPath, FileInfo target, params string[] readed)
        {
            var logger = Program.Logger;
            try {
                logger.LoggingInformation("アクションを実行します。");

                // アクションファイルが存在しない場合はエラー
                if (!File.Exists(actionPath)) {
                    throw new FileNotFoundException("アクションファイルが見つかりません。", actionPath);
                }
                string action = string.Format(cs, File.ReadAllText(actionPath));
                logger.LoggingInformation($"アクションファイルを読み込みました。{actionPath}");
                logger.LoggingInformation($"アクション:{action}");

                logger.LoggingInformation($"アクションをコンパイルします。");
                // コンパイルプロバイダを作成
                CSharpCodeProvider cscp = new CSharpCodeProvider();

                // コンパイルパラメータを設定
                CompilerParameters param = new CompilerParameters();
                param.GenerateInMemory = true;
                param.ReferencedAssemblies.Add("System.Windows.Forms.dll");

                // コンパイル
                CompilerResults cr = cscp.CompileAssemblyFromSource(param, action);

                // エラーがあればエラーを表示
                if (cr.Errors.HasErrors) {
                    var errors = cr.Errors.Cast<CompilerError>().
                        Where(e => e.Line > 6).
                        Select(e => $"行:{e.Line - 6} 位置:{e.Column} {e.ErrorNumber} {e.ErrorText}");
                    throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
                }

                // アクションクラスを取得して実行
                Assembly asm = cr.CompiledAssembly;
                Type type = asm.GetType("ReadedAction");
                MethodInfo mi = type.GetMethod("Main");
                mi.Invoke(null, new object[] { target, readed });
            }
            catch (Exception ex) {
                logger.LoggingError(ex.Message);
                throw;
            }
        }
    }
}
