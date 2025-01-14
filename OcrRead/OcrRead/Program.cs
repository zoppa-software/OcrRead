using OcrRead.Tips;
using OpenAI.Chat;
using SwitchAnalyzers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Pdf;
using Windows.Media.Protection;

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
                Console.WriteLine("  -h, --help       Display this information.");
                Console.WriteLine("  -r, --rectangle  Specify the rectangle to read.");
                Console.WriteLine("  -f, --file       Specify the file to read.");
                Console.WriteLine("  -a, --answer     Specify the answer to the question.");
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

                    // 矩形が指定されていたら切り出し
                    if (rectangle.Count > 0) {
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
            else {
                FormatXmlDocument(xmldoc, Console.Out);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="finfo"></param>
        /// <returns></returns>
        private static Bitmap LoadDocument(FileInfo finfo)
        {
            if (finfo.Extension == ".pdf") {
                using (var pdfStream = File.OpenRead(finfo.FullName))
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
                return new Bitmap(Image.FromFile(finfo.FullName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageFilePath"></param>
        private static void OcrRead(string imageFilePath, string areaSummary, XmlDocument xmlDoc)
        {
            ChatClient client = new ChatClient(
                ConfigurationManager.AppSettings["Model"],
                ConfigurationManager.AppSettings["AppKey"]
            );

            using (Stream imageStream = File.OpenRead(imageFilePath)) {
                BinaryData imageBytes = BinaryData.FromStream(imageStream);

                var messages = new List<ChatMessage> {
                    new SystemChatMessage(
                        ChatMessageContentPart.CreateTextPart(ConfigurationManager.AppSettings["SystemMessage"])
                    ),
                    new UserChatMessage(
                        ChatMessageContentPart.CreateTextPart(ConfigurationManager.AppSettings["UserMessage"]),
                        ChatMessageContentPart.CreateImagePart(imageBytes, "image/png")
                    )
                };

                ChatCompletion completion = client.CompleteChat(messages);

                Console.WriteLine($"Area:{areaSummary} Assistant:{completion.Content[0].Text}");

                var node = xmlDoc.CreateElement("Assistant");
                node.SetAttribute("Area", areaSummary);
                node.SetAttribute("Message", completion.Content[0].Text);
                xmlDoc.DocumentElement.AppendChild(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="stream"></param>
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
