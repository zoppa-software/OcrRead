using System;
using System.Reflection;
using System.Windows.Forms;

namespace OcrReadGUI
{
    /// <summary>アプリケーションのメインクラスです。</summary>
    internal static class Program
    {
        /// <summary>アプリケーションのロガーを取得します。</summary>
        public static ZoppaLogger.Logger Logger { get; private set; }

        /// <summary>アプリケーションのメイン エントリ ポイントです。</summary>
        [STAThread]
        static void Main()
        {
            Logger = ZoppaLogger.Logger.Use(maxLogSize: 200 * 1024, logFilePath: "logs\\application.log");
            Logger.LoggingInformation("-----------------------------");
            Logger.LoggingInformation(" OcrReadGUI v0.0.9");
            Logger.LoggingInformation("    author zoppa software");
            Logger.LoggingInformation("----- application start -----");

            // カレントディレクトリを実行ファイルの場所に変更
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = myAssembly.Location;
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(path);

            // アプリケーションの実行
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            Logger.LoggingInformation("----- application end -----");
            Logger.WaitFinish();
        }
    }
}
