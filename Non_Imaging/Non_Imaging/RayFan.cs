using System;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Non_Imaging
{
    [System.Runtime.InteropServices.Guid("be681d82-4ada-439d-96d4-2e71ec81301b")]
    public class NIRayFan : Command
    {
        static NIRayFan _instance;
        public NIRayFan()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NIRayFan command.</summary>
        public static NIRayFan Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NIRayFan"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Choose focal point of Ray Fan");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_start = gp.Point();

            gp.SetCommandPrompt("Choose endpoint of first ray in fan");
            gp.SetBasePoint(pt_start, false);
            gp.DrawLineFromPoint(pt_start, true);
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_endray1 = gp.Point();

            Vector3d raybegin = pt_endray1 - pt_start;
            if (raybegin.IsTiny(Rhino.RhinoMath.ZeroTolerance))
                return Rhino.Commands.Result.Nothing;

            gp.SetCommandPrompt("Choose endpoint of last ray in fan");
            gp.SetBasePoint(pt_start, false);
            gp.DrawLineFromPoint(pt_start, true);
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_endray2 = gp.Point();

            Vector3d rayend = pt_endray2 - pt_start;
            if (rayend.IsTiny(Rhino.RhinoMath.ZeroTolerance))
                return Rhino.Commands.Result.Nothing;

            GetInteger gi = new GetInteger();
            gi.SetCommandPrompt("Enter the total number of rays desired");
            gi.SetDefaultInteger(3);
            gi.Get();
            if (gi.CommandResult() != Result.Success)
                return gi.CommandResult();

            Int32 numrays = gi.Number();

            if (numrays < 3)
            {
                return Rhino.Commands.Result.Failure;
            }

            double angbetween = Rhino.Geometry.Vector3d.VectorAngle(raybegin, rayend);
            double rotangle = angbetween / (numrays - 1);

            Vector3d rotvector = Vector3d.CrossProduct(raybegin, rayend);
            LineCurve ray = new LineCurve(pt_start, pt_endray1);

            if (doc.Objects.Add(ray) != Guid.Empty)
            {
                doc.Views.Redraw();
            }

            for (int i = 0; i < (numrays - 1); i++)
            {
                ray.Rotate(rotangle, rotvector, pt_start);
                if (doc.Objects.Add(ray) != Guid.Empty)
                {
                    doc.Views.Redraw();
                }

            }

            return Rhino.Commands.Result.Success;
        }
    }
}
