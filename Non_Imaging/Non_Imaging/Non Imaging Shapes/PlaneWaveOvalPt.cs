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
    [System.Runtime.InteropServices.Guid("0263f5b3-17d6-462e-be36-b2455201e2c2")]
    public class NIPlaneWaveOvalPt : Command
    {
        static NIPlaneWaveOvalPt _instance;
        public NIPlaneWaveOvalPt()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NIPlaneWaveOvalPt command.</summary>
        public static NIPlaneWaveOvalPt Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NIPlaneWaveOvalPt"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Choose focal point F");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptF = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose Plane Wave Point Q");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            NI2dPoint ptQ = Utility.GetNI2dPtfromRhinoPt(doc, gp.Point());

            gp.SetCommandPrompt("Choose direction of Plane Wave Propogation");
            gp.SetBasePoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptQ), false);
            gp.DrawLineFromPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptQ), true);
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_endnormal = gp.Point();
            NI2dPoint raynormal = Utility.GetNI2dPtfromRhinoPt(doc, (Point3d)(pt_endnormal - Utility.GetRhino3dPtfromNI2dPt(doc, ptQ)));
            NI2dVector n = new NI2dVector(raynormal);

            OptionDouble index1 = new OptionDouble(1.489);
            OptionDouble index2 = new OptionDouble(1.000);

            double n1 = index1.CurrentValue;
            double n2 = index2.CurrentValue;

            GetOption go1 = new GetOption();
            go1.SetCommandPrompt("Index of Refraction for F (n1) and Q (n2)");
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

            gp.SetCommandPrompt("Choose direction of ray to solve for");
            gp.SetBasePoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptF), false);
            gp.DrawLineFromPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ptF), true);
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_endray1 = gp.Point();
            NI2dPoint raybegin = Utility.GetNI2dPtfromRhinoPt(doc, (Point3d)(pt_endray1 - Utility.GetRhino3dPtfromNI2dPt(doc, ptF)));
            NI2dVector v = new NI2dVector(raybegin);

            index1 = new OptionDouble(10);
            double S = index1.CurrentValue;

            GetOption go2 = new GetOption();
            go2.SetCommandPrompt("Path Lenghth S to be used for calculating Surface");
            go2.AddOptionDouble("S", ref index1);

            while (go2.Get() != GetResult.Cancel)
            {
                if (go2.Get() != GetResult.NoResult)
                {
                    S = index1.CurrentValue;

                }
            }

            NI2dPoint ovalPt = NIUtilities.planewaveOvalpt(ptF, n1, v, ptQ, n2, n, S);

            if (doc.Objects.AddPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ovalPt)) != Guid.Empty)
            {
                doc.Views.Redraw();
            }
            return Result.Success;
        }
    }
}
