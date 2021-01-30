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
    public static class Utility
    {
        public static NI2dPoint GetNI2dPtfromRhinoPt(RhinoDoc doc, Point3d point)
        {
            RhinoView view = doc.Views.ActiveView;
            if (view == null)
                return null;

            Plane localPlane = view.ActiveViewport.ConstructionPlane();

            Non_Imaging_Optics.NI2dPoint ptF = new NI2dPoint(0,0);
            Vector3d scalar = point - localPlane.Origin;
            ptF.x1 = scalar * localPlane.XAxis;
            ptF.x2 = scalar * localPlane.YAxis;

            return ptF;
        }

        public static Point3d GetRhino3dPtfromNI2dPt(RhinoDoc doc, NI2dPoint point)
        {
            RhinoView view = doc.Views.ActiveView;
            if (view == null)
                return new Point3d(0, 0, 0);
            string viewport_name = view.MainViewport.Name;

            Point3d pt = new Point3d(0,0,0);

            switch (viewport_name)
            {
                case "Front":
                    pt = new Point3d(point.x1,0.0,point.x2);
                    break;
                case "Top":
                    pt = new Point3d(point.x1, point.x2, 0.0);
                    break;
                case "Right":
                    pt = new Point3d(0.0, point.x1, point.x2);
                    break;
            }

            return pt;
        }

    }
}