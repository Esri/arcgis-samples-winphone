using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GraphSample.CustomControls;

namespace GraphSample
{
    public class CloseData : IGraphData, IComparable
    {
        public DateTime Date;
        public float Close;

        public CloseData(DateTime date, float close)
        {
            this.Date = date;
            this.Close = close;
        }

        public int CompareTo(object obj) { return this.Date.CompareTo(((CloseData)obj).Date); }
        public double GetX() { return this.Date.ToFileTimeUtc(); }
        public double GetY() { return this.Close; }
        public string GetXText(double x)
        {
            DateTime time = DateTime.FromFileTime((long)x);
            int year = time.Year % 100;
            return String.Format("{0}/{1}/{2}{3}", time.Month, time.Day, year < 10 ? "0" : "", year);
        }
        public string GetYText(double y)
        {
            if (y >= 1000000000)
                return String.Format("{0:0.0}b", y / 1000000000f + 0.05);
            if (y >= 1000000)
                return String.Format("{0:0.0}m", y / 1000000f + 0.05);
            if (y >= 1000)
                return String.Format("{0:0.0}k", y / 1000f + 0.05);
            return String.Format("{0:0.0}", y + 0.05);
        }
    }
}
