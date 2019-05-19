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
    [System.Runtime.InteropServices.Guid("61e37ace-b14d-4f65-adec-286c4242b6d7")]
    public class NICartesianOvalParallel : Command
    {
        static NICartesianOvalParallel _instance;
        public NICartesianOvalParallel()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NICartesianOvalParallel command.</summary>
        public static NICartesianOvalParallel Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NICartesianOvalParallel"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Choose first focal point F");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptF = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose point P on Cartesian Oval");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptP = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            OptionDouble index1 = new OptionDouble(1.000);
            OptionDouble index2 = new OptionDouble(1.489);

            double n1 = index1.CurrentValue;
            double n2 = index2.CurrentValue;

            GetOption go1 = new GetOption();
            go1.SetCommandPrompt("Index of Refraction for Interface ");
            go1.AddOptionDouble("n1", ref index1);
            go1.AddOptionDouble("n2", ref index2);

            while (go1.Get() != GetResult.Cancel)
            {
                if (go1.Get() != GetResult.NoResult)
                {
                    n1 = index1.CurrentValue;
                    n2 = index2.CurrentValue;
                }
            }

            OpticalInterface oInterface = new OpticalInterface(n1, n2, 0.0, 1.0);

            index1 = new OptionDouble(-15.0);
            index2 = new OptionDouble(15.0);
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

            index1 = new OptionDouble(0.0);
            double alpha = index1.CurrentValue * Math.PI / 180.0;

            GetOption go3 = new GetOption();
            go3.SetCommandPrompt("Tilt Angle of Collimated Rays");
            go3.AddOptionDouble("Angle1", ref index1);

            while (go3.Get() != GetResult.Cancel)
            {
                if (go3.Get() != GetResult.NoResult)
                {
                    alpha = index1.CurrentValue * Math.PI / 180.0;
                }
            }

            List<NI2dPoint> ovalPts = NIUtilities.cartesianOvalParallel(ptF, n1, n2, ptP, alpha, phi1, phi2, stepsize);
            List<Point3d> ptList = new List<Point3d>();

            foreach (NI2dPoint point in ovalPts)
            {
                ptList.Add(Utility.GetRhino3dPtfromNI2dPt(doc, point));
            }


            Curve ovalCurve = Rhino.Geometry.Curve.CreateInterpolatedCurve(ptList, 3);

            ovalCurve.UserDictionary.Set("Index1", oInterface.n1);
            ovalCurve.UserDictionary.Set("Index2", oInterface.n2);
            ovalCurve.UserDictionary.Set("Reflectivity", oInterface.reflectivity);
            ovalCurve.UserDictionary.Set("Transmission", oInterface.transmission);

            if (doc.Objects.Add(ovalCurve) != Guid.Empty)
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
