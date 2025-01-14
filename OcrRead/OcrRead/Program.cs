using OcrRead.Tips;
using OpenAI.Chat;
using SwitchAnalyzers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Windows.Data.Pdf;

namespace OcrRead
{
    internal class Program
    {
        /// <summary>エントリポイントです。</summary>
        /// <param name="args">アプリケーション引数。</param>
        static void Main(string[] args)
        {
            // スイッチ環境を作成
            var switchAnalisys = SwitchParser.Create().
                AddSwitch('h', "help", 0).
                AddSwitch('r', "rectangle", 1).
                AddSwitch('f', "file", 1).
                AddSwitch('a', "answer", 1).
                Analisys();

            // ヘルプが指定された場合はヘルプを表示
            if (switchAnalisys.HasSwitch('h', "help")) {
                Console.WriteLine("Usage: OcrRead [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help       ヘルプを表示します。");
                Console.WriteLine("  -r, --rectangle  読み込む矩形を指定します。{{左位置,上位置,右位置,下位置}[,{繰り返し}...]}");
                Console.WriteLine("  -f, --file       読込対象ファイルを指定します。");
                Console.WriteLine("  -a, --answer     読込結果XMLファイル。");
                return;
            }

            // ファイルが指定されていない場合はエラー
            if (!switchAnalisys.HasSwitch('f', "file")) {
                throw new ArgumentException("ファイルが指定されていません。");
            }

            // 矩形が指定されていたら取得
            var rectangle = new List<Rect>();
            if (switchAnalisys.HasSwitch('r', "rectangle")) {
                var recstr = switchAnalisys.GetSwitchValue('r', "rectangle").SelectMany(v => v);
                foreach (var rec in recstr) {
                    rectangle.AddRange(Rect.FromString(rec));
                }
            }

            // 出力先を作成
            var xmldoc = new XmlDocument();
            xmldoc.AppendChild(xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null));
            xmldoc.AppendChild(xmldoc.CreateElement("Reaults"));

            //
            int tmpCount = 0;
            foreach (var ph in switchAnalisys.GetSwitchValue('f', "file").SelectMany(v => v)) {
                try {
                    // ファイルが存在するか確認
                    var finfo = new FileInfo(ph);
                    if (!finfo.Exists) {
                        throw new ArgumentException("ファイルが存在しません。");
                    }

                    // ドキュメントを読み込み
                    Console.Error.WriteLine($"File:{finfo.FullName}");
                    var targetDoc = LoadDocument(finfo);

                    if (rectangle.Count > 0) {
                        // 矩形が指定されていたら切り出してOCR
                        foreach (var rec in rectangle) {
                            using (var bmp = new Bitmap((int)(rec.Width + 0.5), (int)(rec.Height + 0.5))) {
                                using (var g = Graphics.FromImage(bmp)) {
                                    g.DrawImage(targetDoc, new Rectangle(0, 0, bmp.Width, bmp.Height), 
                                        (int)(rec.X + 0.5), (int)(rec.Y + 0.5), (int)(bmp.Width + 0.5), (int)(bmp.Height + 0.5), GraphicsUnit.Pixel);
                                }
                                var tempPath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(finfo.Name)}_{tmpCount++}.png");
                                bmp.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
                                OcrRead(tempPath, rec.ToString(), xmldoc);
                            }
                        }
                    }
                    else {
                        // 矩形が指定されていない場合は全体をOCR
                        var tempPath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(finfo.Name)}_{tmpCount++}.png");
                        targetDoc.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
                        OcrRead(tempPath, "all", xmldoc);
                    }
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"ERROR:{ex.Message}");
                }
            }

            // 結果を保存
            Console.WriteLine("----- Xml Reasult -----");
            if (switchAnalisys.HasSwitch('a', "answer")) {
                using (var sw = new StreamWriter(switchAnalisys.GetSwitchValue('a', "answer").First().First(), false, Encoding.UTF8)) {
                    FormatXmlDocument(xmldoc, sw);
                }
            }
            FormatXmlDocument(xmldoc, Console.Out);
        }

        /// <summary>画像ファイルを読み込みます。</summary>
        /// <param name="finfo">画像ファイル。</param>
        /// <returns>ビットマップ。</returns>
        private static Bitmap LoadDocument(FileInfo finfo)
        {
            if (finfo.Extension == ".pdf") {
                using (var pdfStream = File.OpenRead(finfo.FullName))
                using (var winrtStream = pdfStream.AsRandomAccessStream()) {
                    // PDFドキュメントを読み込み
                    var task = PdfDocument.LoadFromStreamAsync(winrtStream).AsTask();
                    task.Wait();

                    // ページをビットマップに変換
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
                return new Bitmap(Image.FromFile(finfo.FullName));
            }
        }

        /// <summary>OCRを実行します。</summary>
        /// <param name="imageFilePath">対象画像。</param>
        /// <param name="areaSummary">領域を表す文字列。</param>
        /// <param name="xmlDoc">出力先XMLドキュメント。</param>
        private static void OcrRead(string imageFilePath, string areaSummary, XmlDocument xmlDoc)
        {
            // OpenAI Chat APIを使用してOCRを実行
            // https://platform.openai.com/docs/guides/chat
            ChatClient client = new ChatClient(
                ConfigurationManager.AppSettings["Model"],
                ConfigurationManager.AppSettings["AppKey"]
            );

            // 画像ファイルを読み込み
            using (Stream imageStream = File.OpenRead(imageFilePath)) {
                BinaryData imageBytes = BinaryData.FromStream(imageStream);

                // チャットメッセージを作成
                var messages = new List<ChatMessage> {
                    new SystemChatMessage(
                        ChatMessageContentPart.CreateTextPart(ConfigurationManager.AppSettings["SystemMessage"])
                    ),
                    new UserChatMessage(
                        ChatMessageContentPart.CreateTextPart(ConfigurationManager.AppSettings["UserMessage"]),
                        ChatMessageContentPart.CreateImagePart(imageBytes, "image/png")
                    )
                };

                // チャットを実行
                ChatCompletion completion = client.CompleteChat(messages);

                Console.WriteLine($"Area:{areaSummary} Assistant:{completion.Content[0].Text}");

                // 結果をXMLに追加
                var node = xmlDoc.CreateElement("Assistant");
                node.SetAttribute("Area", areaSummary);
                node.SetAttribute("Message", completion.Content[0].Text);
                xmlDoc.DocumentElement.AppendChild(node);
            }
        }

        /// <summary>XMLドキュメントを整形して出力します。</summary>
        /// <param name="xmlDocument">XMLドキュメント。</param>
        /// <param name="stream">出力先。</param>
        private static void FormatXmlDocument(XmlDocument xmlDocument, TextWriter stream)
        {
            using (XmlTextWriter writer = new XmlTextWriter(stream)) {
                // インデント設定
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                writer.IndentChar = ' ';

                // 書式を指定して書き込む
                xmlDocument.WriteContentTo(writer);
            }
        }
    }
}
