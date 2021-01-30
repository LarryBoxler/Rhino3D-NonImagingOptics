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
    [System.Runtime.InteropServices.Guid("84e64671-59aa-4018-82a7-2f3d7267ae67")]
    public class NIConvergingOvalPtPt : Command
    {
        static NIConvergingOvalPtPt _instance;
        public NIConvergingOvalPtPt()
        {
            _instance = this;
        }

        ///<summary>The only instance of the ConvergingOvalPtPt command.</summary>
        public static NIConvergingOvalPtPt Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NIConvergingOvalPtPt"; }
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

            OptionDouble index1 = new OptionDouble(1.489);
            OptionDouble index2 = new OptionDouble(1.000);

            double n1 = index1.CurrentValue;
            double n2 = index2.CurrentValue;

            GetOption go1 = new GetOption();
            go1.SetCommandPrompt("Index of Refraction for F (n1) and G (n2)");
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
            NI2dPoint raybegin = Utility.GetNI2dPtfromRhinoPt(doc,(Point3d)(pt_endray1 - Utility.GetRhino3dPtfromNI2dPt(doc, ptF)));
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

            NI2dPoint ovalPt = NIUtilities.convergingOvalptpt(ptF, n1, v, ptG, n2, S);

            if (doc.Objects.AddPoint(Utility.GetRhino3dPtfromNI2dPt(doc, ovalPt)) != Guid.Empty)
            {
                doc.Views.Redraw();
            }

            return Result.Success;
        }
    }
}
