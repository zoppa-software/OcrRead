using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrRead.Tips
{
    public struct Rect
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;

        public Rect(double left, double top, double right, double bottom)
        {
            this.X = left;
            this.Y = top;
            this.Width = right - left;
            this.Height = bottom - top;
        }

        public double Left => X;

        public double Top => Y;

        public double Right => X + Width;

        public double Bottom => Y + Height;

        public override string ToString()
        {
            return $"{this.X},{this.Y},{this.Width},{this.Height}";
        }

        public static List<Rect> FromString(string input)
        {
            var rstr = new List<string>();
            var split = true;

            for (int i = 0; i < input.Length; ++i) {
                var c = input[i];
                switch (c) {
                    case '{':
                        if (!split) {
                            throw new ArgumentException("Invalid format.");
                        }

                        i++;
                        var buf = new StringBuilder();
                        for (; i < input.Length; ++i) {
                            if (input[i] == '}') {
                                rstr.Add(buf.ToString());
                                break;
                            }
                            else {
                                buf.Append(input[i]);
                            }
                        }
                        break;

                    case ',':
                        split = true;
                        break;

                    default:
                        if (!char.IsWhiteSpace(c)) {
                            throw new ArgumentException("Invalid format.");
                        }
                        break;
                }
            }

            var result = new List<Rect>();
            foreach (var s in rstr) {
                var r = s.Split(',').Select(x => double.Parse(x)).ToArray();
                if (r.Length != 4) {
                    throw new ArgumentException("Invalid format.");
                }
                result.Add(new Rect(r[0], r[1], r[2], r[3]));
            }
            return result;
        }
    }
}
