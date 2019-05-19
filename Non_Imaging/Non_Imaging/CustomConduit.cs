using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Non_Imaging
{
    class CustomConduit : Rhino.Display.DisplayConduit
    {
        public System.Collections.Generic.List<Rhino.Geometry.Curve> _curve { get; set; }

        protected override void DrawForeground(Rhino.Display.DrawEventArgs e)
        {
            foreach (Rhino.Geometry.Curve curve in _curve)
            {
                e.Display.DrawCurve(curve, System.Drawing.Color.Red);
            }
        }
    }
}
