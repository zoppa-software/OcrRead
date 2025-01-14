using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OcrReadGUI
{
    /// <summary>ドキュメントコントロールクラスです。</summary>
    public partial class DocumentControl : Control
    {
        /// <summary>レイアウトフォルダ。</summary>
        public const string FL_FOLDER = "layout";

        /// <summary>レイアウトファイル。</summary>
        public const string FL_FILE = "drawing.txt";

        /// <summary>ロガー。</summary>
        private ZoppaLogger.Logger logger;

        /// <summary>ドラッグ開始位置。</summary>
        private Point _startPoint = new Point(-1, -1);

        /// <summary>ドラッグ終了位置。</summary>
        private Point _endPoint;

        /// <summary>ズーム率を取得します。</summary>
        public double Zoom { get; private set; } = 1.0;

        /// <summary>ドキュメントを取得または設定します。</summary>
        public Bitmap Document { get; set; } = null;

        /// <summary>選択された矩形のリストを取得または設定します。</summary>
        public List<Rectangle> Rectangles { get; } = new List<Rectangle>();

        /// <summary>OCRレイアウト名を取得または設定します。</summary>
        public string OcrLayout { get; set; } = null;

        /// <summary>コンストラクタ。</summary>
        public DocumentControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.logger = Program.Logger;
        }

        /// <summary>コントロールが作成されたときに呼び出されます。</summary>
        protected override void OnCreateControl()
        {
            try {
                base.OnCreateControl();

                // レイアウトの読み込み
                this.LoadLayout();
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
            }
        }

        /// <summary>レイアウトを保存します。</summary>
        public void SaveLayout()
        {
            // フォルダが存在しない場合は作成
            if (!Directory.Exists(FL_FOLDER)) {
                Directory.CreateDirectory(FL_FOLDER);
            }

            // レイアウトを保存
            using (var sw = new StreamWriter($"{FL_FOLDER}\\{FL_FILE}", false)) {
                foreach (var r in this.Rectangles) {
                    sw.WriteLine($"{r.X},{r.Y},{r.Width},{r.Height}");
                }
            }
        }

        /// <summary>レイアウトを読み込みます。</summary>
        public void LoadLayout()
        {
            // レイアウトをクリア
            this.Rectangles.Clear();

            // ファイルが存在する場合は読み込み
            if (File.Exists($"{FL_FOLDER}\\{FL_FILE}")) {
                using (var sr = new StreamReader($"{FL_FOLDER}\\{FL_FILE}")) {
                    while (!sr.EndOfStream) {
                        var line = sr.ReadLine();
                        var parts = line.Split(',');
                        this.Rectangles.Add(new Rectangle(
                            int.Parse(parts[0]),
                            int.Parse(parts[1]),
                            int.Parse(parts[2]),
                            int.Parse(parts[3])
                        ));
                    }
                }
            }
        }

        /// <summary>レイアウトをクリアします。</summary>
        public void ClearLayout()
        {
            try {
                // ドキュメントが存在する場合はクリア
                if (this.Document != null) {
                    this.Rectangles.Clear();
                    this.Refresh();
                    this.SaveLayout();
                }
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
            }
        }

        /// <summary>マウスダウン時の処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            try {
                base.OnMouseDown(e);
                this._startPoint = e.Location;
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
            }
        }

        /// <summary>マウス移動時の処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {

            try {
                base.OnMouseMove(e);
                if (e.Button == MouseButtons.Left &&
                    this._startPoint.X > -1 && this._startPoint.Y > -1) {
                    // ドラッグ中の終了位置を更新
                    this._endPoint = e.Location;

                    // ドラッグ中の矩形を描画
                    (int x, int y, int w, int h) = CalcDragAreaPosition();
                    this.Invalidate(new Rectangle(x - 100, y - 100, w + 200, h + 200));
                }
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
            }
        }

        /// <summary>マウスアップ時の処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try {
                base.OnMouseUp(e);
                if (e.Button == MouseButtons.Left &&
                    this._startPoint.X > -1 && this._startPoint.Y > -1) {
                    // 移動量が一定以上の場合は矩形を追加
                    if (Math.Abs(this._startPoint.X - this._endPoint.X) > 5 &&
                        Math.Abs(this._startPoint.Y - this._endPoint.Y) > 5) {
                        // ドラッグ中の終了位置を更新
                        this._endPoint = e.Location;

                        // 矩形を追加
                        (int x, int y, int w, int h) = CalcDragAreaPosition();
                        var rect = new Rectangle(
                            (int)(x / this.Zoom),
                            (int)(y / this.Zoom),
                            (int)(w / this.Zoom),
                            (int)(h / this.Zoom)
                        );
                        this.Rectangles.Add(rect);

                        // 再描画
                        this.Invalidate();

                        // レイアウトを保存
                        this.SaveLayout();
                    }
                }
                this._startPoint = new Point(-1, -1);
            }
            catch (Exception ex) {
                this.logger.LoggingError(ex.Message);
            }
        }

        /// <summary>サイズ変更時の処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Refresh();
        }

        /// <summary>描画イベントハンドラです。</summary>
        /// <param name="pe">ペイントイベントオブジェクト。</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            // 描画オブジェクトを取得
            var g = pe.Graphics;

            // 背景をクリア
            g.Clear(Color.LightGray);

            if (this.Document != null) {
                // ズーム率を計算
                this.Zoom = Math.Min(
                    1.0,
                    Math.Min(this.Width / (double)this.Document.Width,
                             this.Height / (double)this.Document.Height)
                );

                // ズーム後のサイズを計算
                int newWidth = (int)(this.Document.Width * this.Zoom);
                int newHeight = (int)(this.Document.Height * this.Zoom);

                // 画像を描画
                var frame = new Rectangle(0, 0, newWidth, newHeight);
                g.DrawImage(this.Document, frame);
                g.DrawRectangle(Pens.Gray, frame);

                // 選択領域の矩形を描画
                using (var fnt = new Font("Meiryo UI", 9, FontStyle.Bold)) {
                    for (int i = 0; i < this.Rectangles.Count; ++i) {
                        var r = this.Rectangles[i];
                        var x = (int)(r.X * this.Zoom);
                        var y = (int)(r.Y * this.Zoom);
                        var w = (int)(r.Width * this.Zoom);
                        var h = (int)(r.Height * this.Zoom);
                        g.DrawRectangle(Pens.Blue, new Rectangle(x, y, w, h));

                        if (this.Rectangles.Count > 1) {
                            g.DrawString($"{i + 1}", fnt, Brushes.Blue, x, y);
                        }
                    }
                }

                // ドラッグ中の矩形を描画
                if (this._startPoint.X > -1 && this._startPoint.Y > -1) {
                    (int x, int y, int w, int h) = CalcDragAreaPosition();
                    g.DrawRectangle(
                        Pens.Red,
                        new Rectangle(x, y, w, h)
                    );
                }
            }
        }

        /// <summary>ドラッグエリアの位置を計算します。</summary>
        /// <returns>ドラッグエリア。</returns>
        private (int x, int y, int w, int h) CalcDragAreaPosition()
        {
            return (
                Math.Min(this._startPoint.X, this._endPoint.X),
                Math.Min(this._startPoint.Y, this._endPoint.Y),
                Math.Abs(this._startPoint.X - this._endPoint.X),
                Math.Abs(this._startPoint.Y - this._endPoint.Y)
            );
        }
    }
}
