using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Display;
using Non_Imaging_Optics;

namespace Non_Imaging
{
    [System.Runtime.InteropServices.Guid("ce72572f-df6f-4677-b55a-2347bd9fff56")]
    public class NITiltedParabola : Command
    {
        static NITiltedParabola _instance;
        public NITiltedParabola()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NITiltedParabola command.</summary>
        public static NITiltedParabola Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NITiltedParabola"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Choose Focus of Parabola F");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptF = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose Point P that Parabola will Pass Through");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptP = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            OptionDouble index1 = new OptionDouble(0.0);
            GetOption go3 = new GetOption();
            go3.SetCommandPrompt("Tilt Angle alpha of Parabola Axis ");
            go3.AddOptionDouble("alpha", ref index1);
            double alpha = index1.CurrentValue * Math.PI / 180.0;

            while (go3.Get() != GetResult.Cancel)
            {
                if (go3.Get() != GetResult.NoResult)
                {
                    alpha = index1.CurrentValue * Math.PI / 180.0;
                }
            }


            index1 = new OptionDouble(-15.0);
            OptionDouble index2 = new OptionDouble(15.0);
            OptionDouble index3 = new OptionDouble(1.0);

            GetOption go2 = new GetOption();
            go2.SetCommandPrompt("Angles to construct curve (from, to) in degrees and stepsize");
            go2.AddOptionDouble("Angle1", ref index1);
            go2.AddOptionDouble("Angle2", ref index2);
            go2.AddOptionDouble("StepSize", ref index3);


            double phi1 = index1.CurrentValue * Math.PI / 180.0;
            double phi2 = index2.CurrentValue * Math.PI / 180.0;
            double stepsize = index3.CurrentValue * Math.PI / 180.0;

            while (go2.Get() != GetResult.Cancel)
            {
                if (go2.Get() != GetResult.NoResult)
                {
                    phi1 = index1.CurrentValue * Math.PI / 180.0;
                    phi2 = index2.CurrentValue * Math.PI / 180.0;
                    stepsize = index3.CurrentValue * Math.PI / 180.0;
                }
            }

            List<NI2dPoint> parabolaPts = NIUtilities.tiltedParabola(alpha, ptF, ptP, phi1, phi2, stepsize);
            List<Point3d> ptList = new List<Point3d>();

            foreach (NI2dPoint point in parabolaPts)
            {
                ptList.Add(Utility.GetRhino3dPtfromNI2dPt(doc, point));
            }

            Curve parabolaCurve = Rhino.Geometry.Curve.CreateInterpolatedCurve(ptList, 3);


            if (doc.Objects.Add(parabolaCurve) != Guid.Empty)
            {
                doc.Views.Redraw();
            }

            if (doc.Objects.AddPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptF)) != Guid.Empty)
            {
                doc.Views.Redraw();
            }

            if (doc.Objects.AddPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptP)) != Guid.Empty)
            {
                doc.Views.Redraw();
            }
            return Result.Success;
        }
    }
}
