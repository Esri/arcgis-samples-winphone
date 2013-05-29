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
    public class BasicGraphData : IGraphData, IComparable
    {
        public int   x;
        public float y; // float to use less memory

        public BasicGraphData(int x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public int CompareTo(object obj) { return this.x.CompareTo(((BasicGraphData)obj).x); }
        public double GetX() { return this.x; }
        public double GetY() { return this.y; }
        public string GetXText(double x)
        {
            return String.Format("{0}", (int)(x + 0.5) );
        }
        public string GetYText(double y)
        {
            return String.Format("{0:0.0}", y + 0.05);
        }
    }
}
