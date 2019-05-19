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
    [System.Runtime.InteropServices.Guid("f03198d9-84fa-43d6-82cb-f31d9174050a")]
    public class NICartesianOvalConverge : Command
    {
        public NICartesianOvalConverge()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static NICartesianOvalConverge Instance
        {
            get;
            private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "NICartesianOvalConverge"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Choose first focal point F");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptF = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose second focal point G");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptG = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose point P on Cartesian Oval");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptP = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            OptionDouble index1 = new OptionDouble(1.489);
            OptionDouble index2 = new OptionDouble(1.000);

            double n1 = index1.CurrentValue;
            double n2 = index2.CurrentValue;

            GetOption go1 = new GetOption();
            go1.SetCommandPrompt("Index of Refraction for Interface (n1>n2)");
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
                    phi1 = index1.CurrentValue*Math.PI/180.0;
                    phi2 = index2.CurrentValue*Math.PI/180.0;
                    stepsize = index3.CurrentValue * Math.PI / 180.0;
                }
            }


            List<NI2dPoint> ovalPts = NIUtilities.cartesianOvalConverge(ptF, n1, ptG, n2, ptP, phi1, phi2, stepsize);
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

            if (doc.Objects.AddPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptG)) != Guid.Empty)
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
