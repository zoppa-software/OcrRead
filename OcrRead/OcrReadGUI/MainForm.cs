using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Windows.Data.Pdf;

namespace OcrReadGUI
{
    /// <summary>メインフォームクラスです。</summary>
    public partial class MainForm : Form
    {
        /// <summary>ロガー。</summary>
        private ZoppaLogger.Logger logger;

        /// <summary>コンストラクタ。</summary>
        public MainForm()
        {
            InitializeComponent();
            this.logger = Program.Logger;
        }

        /// <summary>ファイル選択ボタンがクリックされたときの処理です。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void FileBtn_Click(object sender, EventArgs e)
        {
            try {
                this.logger.LoggingInformation("ファイル選択ボタンがクリックされました。");
                OpenFileDialog ofd = new OpenFileDialog {
                    Filter = "PDFファイル(*.pdf)|*.pdf|画像ファイル(*.png)|*.png",
                    Title = "ファイルを選択",
                    RestoreDirectory = true
                };
                if (ofd.ShowDialog() == DialogResult.OK) {
                    this.TargetFileTxt.Text = ofd.FileName;
                    this.logger.LoggingInformation($"ファイルを選択しました。{ofd.FileName}");
                }

                var fi = new FileInfo(this.TargetFileTxt.Text);
                if (fi.Exists) {
                    this.logger.LoggingInformation("画像読込開始");
                    this.DocCtrl.Document = this.LoadDocument(fi);
                    this.logger.LoggingInformation("画像読込終了");
                    this.DocCtrl.Refresh();
                }
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>画像読込。</summary>
        /// <param name="finfo">ファイル情報。</param>
        /// <returns>画像。</returns>
        private Bitmap LoadDocument(FileInfo finfo)
        {
            if (finfo.Extension == ".pdf") {
                this.logger.LoggingInformation("PDF画像");
                using (var pdfStream = File.OpenRead("sample.pdf"))
                using (var winrtStream = pdfStream.AsRandomAccessStream()) {
                    var task = PdfDocument.LoadFromStreamAsync(winrtStream).AsTask();
                    task.Wait();

                    var doc = task.Result;
                    using (var page = doc.GetPage(0))
                    using (var memStream = new MemoryStream())
                    using (var outStream = memStream.AsRandomAccessStream()) {
                        page.RenderToStreamAsync(outStream).AsTask().Wait();
                        return new Bitmap(Image.FromStream(memStream));
                    }
                }
            }
            else {
                this.logger.LoggingInformation("イメージ画像");
                return new Bitmap(Image.FromFile(finfo.FullName));
            }
        }

        /// <summary>クリアボタンがクリックされたときの処理です。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            try {
                this.logger.LoggingInformation("クリアボタンがクリックされました。");
                this.DocCtrl.ClearLayout();
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
            }
        }

        /// <summary>実行ボタンがクリックされたときの処理です。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void RunBtn_Click(object sender, EventArgs e)
        {
            const string ANS_FILE = "ans.xml";
            try {
                this.logger.LoggingInformation("実行ボタンがクリックされました。");

                // ファイルが存在しない場合はエラー
                var fi = new FileInfo(this.TargetFileTxt.Text);
                if (!fi.Exists) {
                    throw new Exception("ファイルが存在しません。");
                }

                // ファイルが存在する場合はOCRアプリケーションのパラメータを作成
                var cmd = new StringBuilder();
                cmd.AppendFormat("-a {0} ", ANS_FILE);
                if (this.DocCtrl.Rectangles.Count > 0) {
                    var recs = this.DocCtrl.Rectangles.Select(r => string.Format("{{{0},{1},{2},{3}}}", r.X, r.Y, r.Right, r.Bottom));
                    cmd.AppendFormat("-r {{{0}}} ", string.Join(",", recs));
                }
                cmd.AppendFormat("-f \"{0}\"", fi.FullName);
                this.logger.LoggingInformation($"OCRアプリケーションのパラメータ:{cmd.ToString()}");

                // OCRアプリケーションを起動
                ProcessStartInfo psInfo = new ProcessStartInfo {
                    FileName = "OcrRead.exe",
                    Arguments = cmd.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                Process p = Process.Start(psInfo);
                p.WaitForExit();

                // 結果を読み込み
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(ANS_FILE);
                var msgs = new List<string>();
                foreach (XmlNode node in xmlDoc.SelectNodes("Reaults/Assistant")) {
                    var msg = node.Attributes["Message"].Value;
                    if (msg.StartsWith("「") && msg.EndsWith("」")) {
                        msg = msg.Substring(1, msg.Length - 2);
                    }
                    msgs.Add(msg);
                }

                // スクリプトを実行
                ReadedAction.Run(this.ScriptTxtBox.Text, fi, msgs.ToArray());
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
    }
}
