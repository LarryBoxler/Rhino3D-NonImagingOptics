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
    [System.Runtime.InteropServices.Guid("947f7706-00cf-4df0-b616-24b38f922451")]
    public class NIWindingInvolute : Command
    {
        static NIWindingInvolute _instance;
        public NIWindingInvolute()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NIWindingInvolute command.</summary>
        public static NIWindingInvolute Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NIWindingInvolute"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Choose first focal point F");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptF = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose Point P on Involute");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptP = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());


            OptionDouble index1 = new OptionDouble(1.000);
            double radius = index1.CurrentValue;

            GetOption go1 = new GetOption();
            go1.SetCommandPrompt("radius of ");
            go1.AddOptionDouble("r", ref index1);

            while (go1.Get() != GetResult.Cancel)
            {
                if (go1.Get() != GetResult.NoResult)
                {
                    radius = index1.CurrentValue;
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

            List<NI2dPoint> involutePts = NIUtilities.windingInvolute(ptP, ptF, radius,  phi1, phi2, stepsize);
            List<Point3d> ptList = new List<Point3d>();

            foreach (NI2dPoint point in involutePts)
            {
                ptList.Add(Utility.GetRhino3dPtfromNI2dPt(doc, point));
            }

            Curve involuteCurve = Rhino.Geometry.Curve.CreateInterpolatedCurve(ptList, 3);
 

            if (doc.Objects.Add(involuteCurve) != Guid.Empty)
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
