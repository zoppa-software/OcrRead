using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OcrReadGUI
{
    public partial class DocumentControl : Control
    {
        public double Zoom { get; set; } = 1.0;

        public Bitmap Document { get; set; } = null;

        public List<Rectangle> Rectangles { get; set; } = new List<Rectangle>();

        public string OcrLayout { get; set; } = null;

        private Point _startPoint = new Point(-1, -1);

        private Point _endPoint;

        public DocumentControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.LoadLayout();
        }

        public void SaveLayout()
        {
            if (!Directory.Exists("layout")) {
                Directory.CreateDirectory("layout");
            }

            using (var sw = new StreamWriter($"layout\\drawing.txt")) {
                foreach (var r in this.Rectangles) {
                    sw.WriteLine($"{r.X},{r.Y},{r.Width},{r.Height}");
                }
            }
        }

        public void LoadLayout()
        {
            this.Rectangles.Clear();
            if (File.Exists("layout\\drawing.txt")) {
                using (var sr = new StreamReader("layout\\drawing.txt")) {
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

        public void ClearLayout()
        {
            if (this.Document != null) {
                this.Rectangles.Clear();
                this.Refresh();
                this.SaveLayout();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this._startPoint = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left &&
                this._startPoint.X > -1 && this._startPoint.Y > -1) {
                this._endPoint = e.Location;
                (int x, int y, int w, int h) = CalcDragAreaPosition();
                this.Invalidate(new Rectangle(x - 100, y - 100, w + 200, h + 200));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && 
                this._startPoint.X > -1 && this._startPoint.Y > -1) {
                if (Math.Abs(this._startPoint.X - this._endPoint.X) > 5 &&
                    Math.Abs(this._startPoint.Y - this._endPoint.Y) > 5) {
                    this._endPoint = e.Location;
                    (int x, int y, int w, int h) = CalcDragAreaPosition();
                    var rect = new Rectangle(
                        (int)(x / this.Zoom),
                        (int)(y / this.Zoom),
                        (int)(w / this.Zoom),
                        (int)(h / this.Zoom)
                    );
                    this.Rectangles.Add(rect);

                    this.Invalidate();
                    this.SaveLayout();
                }
            }
            this._startPoint = new Point(-1, -1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            var g = pe.Graphics;

            g.Clear(Color.LightGray);

            if (this.Document != null) {
                this.Zoom = Math.Min(
                    1.0,
                    Math.Min(this.Width / (double)this.Document.Width,
                             this.Height / (double)this.Document.Height)
                );
                int newWidth = (int)(this.Document.Width * this.Zoom);
                int newHeight = (int)(this.Document.Height * this.Zoom);
                var frame = new Rectangle(0, 0, newWidth, newHeight);
                g.DrawImage(this.Document, frame);
                g.DrawRectangle(Pens.Gray, frame);

                using (var fnt = new Font("Meiryo UI", 9, FontStyle.Bold)) {
                    for (int i = 0; i < this.Rectangles.Count; ++i) {
                        var r = this.Rectangles[i];
                        var x = (int)(r.X * this.Zoom);
                        var y = (int)(r.Y * this.Zoom);
                        var w = (int)(r.Width * this.Zoom);
                        var h = (int)(r.Height * this.Zoom);
                        g.DrawRectangle(Pens.Blue, new Rectangle(x, y, w, h));

                        if (this.Rectangles.Count > 0) {
                            g.DrawString($"{i + 1}", fnt, Brushes.Blue, x, y);
                        }
                    }
                }

                if (this._startPoint.X > -1 && this._startPoint.Y > -1) {
                    (int x, int y, int w, int h) = CalcDragAreaPosition();
                    g.DrawRectangle(
                        Pens.Red,
                        new Rectangle(x, y, w, h)
                    );
                }
            }
        }

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
