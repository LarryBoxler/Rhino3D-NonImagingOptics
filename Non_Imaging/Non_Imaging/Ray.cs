using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Non_Imaging
{
    public class Ray
    {
        public double CurrentIOR { get; set; }
        public Vector3d Direction { get; set; }
        public List<Point3d> Raypath { get; set; }
        public double Flux { get; set; }
        public double Wavelength { get; set; }

        public Ray()
        {
            CurrentIOR = 1.000;
            Direction = new Vector3d(0, 0, 1);
            Raypath = new List<Point3d>();
            Raypath.Add(new Point3d(0, 0, 0));
            Flux = 1.0;
            Wavelength = 555.0;
        }

        public Ray(Curve rayCurve)
        {
            CurrentIOR = 1.000;
            Direction = new Vector3d((rayCurve.PointAtEnd - rayCurve.PointAtStart));
            Raypath = new List<Point3d>();
            Raypath.Add(rayCurve.PointAtStart);
            Flux = 1.0;
            Wavelength = 555.0;
        }

        public Boolean getIntersection(Curve[] traceCurves, out Vector3d normalVector, out OpticalInterface oInterface)
        {
            Point3d pointOnObject;
            int whichGeometry;
            double tangent;
            normalVector = new Vector3d();
            oInterface = new OpticalInterface();

            Direction = Direction * (1 / Direction.Length);

            Direction = Direction * 0.1;

            Line line = new Line(Raypath[Raypath.Count - 1], Direction);
            LineCurve inRay = new LineCurve(line);

            Curve curve = inRay.Extend(CurveEnd.End, CurveExtensionStyle.Line, traceCurves);
            if (curve == null)
                return false;

            Direction = Direction * -1;
            Line intline = new Line(curve.PointAtEnd, Direction);
            LineCurve intcurve = new LineCurve(intline);
            Direction = Direction * -1;

            var curveint = intcurve.ClosestPoints(traceCurves, out _, out pointOnObject, out whichGeometry, 0.1);
            if (curveint == false)
                return false;

            var tanPoint = traceCurves[whichGeometry].ClosestPoint(pointOnObject, out tangent);
            if (tanPoint == false)
                return false;

            Vector3d tanVector = traceCurves[whichGeometry].TangentAt(tangent);
            Vector3d axisVector = Vector3d.CrossProduct(Direction, tanVector);
            if (tanVector.Rotate(RhinoMath.ToRadians(90), axisVector) == false)
                return false;

            normalVector = tanVector;

            double angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, Direction));
            if (angleBetween < -90 || angleBetween > 90)
                normalVector = normalVector * -1;

            Raypath.Add(pointOnObject);

            oInterface.n1 = traceCurves[whichGeometry].UserDictionary.GetDouble("Index1");
            oInterface.n2 = traceCurves[whichGeometry].UserDictionary.GetDouble("Index2");
            oInterface.reflectivity = traceCurves[whichGeometry].UserDictionary.GetDouble("Reflectivity");
            oInterface.transmission = traceCurves[whichGeometry].UserDictionary.GetDouble("Transmission");

            return true;

        }

        public Boolean getIntersection(ObjRef[] traceGeometry, out Vector3d normalVector, out OpticalInterface oInterface)
        {
            Point3d pointOnObject;
            int whichGeometry;
            double surfU, surfV;
            normalVector = new Vector3d();
            oInterface = new OpticalInterface();
            Surface[] traceSurfaces = new Surface[traceGeometry.Length];
            for (int i = 0; i < traceGeometry.Length; i++)
            {
                traceSurfaces[i] = traceGeometry[i].Surface();
            }

            Direction = Direction * (1 / Direction.Length);

            Direction = Direction * 0.1;

            Line line = new Line(Raypath[Raypath.Count - 1], Direction);
            LineCurve inRay = new LineCurve(line);

            Curve curve = inRay.Extend(CurveEnd.End, CurveExtensionStyle.Line, traceSurfaces);
            if (curve == null)
                return false;

            Direction = Direction * -1;
            Line intline = new Line(curve.PointAtEnd, Direction);
            LineCurve intcurve = new LineCurve(intline);
            Direction = Direction * -1;

            var curveint = intcurve.ClosestPoints(traceSurfaces, out _, out pointOnObject, out whichGeometry, 0.1);
            if (curveint == false)
                return false;
            var normalPoint = traceSurfaces[whichGeometry].ClosestPoint(pointOnObject, out surfU, out surfV);
            normalVector = traceSurfaces[whichGeometry].NormalAt(surfU, surfV);

            double angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, Direction));
            if (angleBetween < -90 || angleBetween > 90)
                normalVector = normalVector * -1;

            Raypath.Add(pointOnObject);

            oInterface.n1 = traceGeometry[whichGeometry].Geometry().UserDictionary.GetDouble("Index1");
            oInterface.n2 = traceGeometry[whichGeometry].Geometry().UserDictionary.GetDouble("Index2");
            oInterface.reflectivity = traceGeometry[whichGeometry].Geometry().UserDictionary.GetDouble("Reflectivity");
            oInterface.transmission = traceGeometry[whichGeometry].Geometry().UserDictionary.GetDouble("Transmission");

            return true;
        }

        public Boolean Reflect(Vector3d normalVector)
        {
            Direction = Direction * (1 / Direction.Length);
            Direction = Direction - 2 * (Direction * normalVector) * normalVector;
            if (Direction == null)
                return false;
            return true;
        }

        public Boolean Refract(Vector3d normalVector, OpticalInterface oInterface)
        {

            double inIOR;
            double outIOR;
            Direction = Direction * (1 / Direction.Length);
            normalVector = normalVector * (1 / normalVector.Length);

            if (CurrentIOR == oInterface.n1)
            {
                inIOR = CurrentIOR;
                outIOR = oInterface.n2;
            }

            else if (CurrentIOR == oInterface.n2)
            {
                inIOR = CurrentIOR;
                outIOR = oInterface.n1;
            }

            else
                return false;

            double c1 = inIOR / outIOR;
            double c2 = Direction * normalVector;
            double angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, Direction));

            if (inIOR > outIOR)
            {

                double criticalAngle = RhinoMath.ToDegrees(Math.Asin(outIOR / inIOR));

                if (angleBetween < -90 || angleBetween > 90)
                {
                    normalVector = normalVector * -1;
                    angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, Direction));
                }

                if (angleBetween > criticalAngle)
                {
                    if (Reflect(normalVector) == true)
                        return true;
                    else
                        return false;
                }

                Direction = c1 * Direction + (-(c2) * c1 + Math.Sqrt(1 - c1 * c1 * (1.0 - c2 * c2))) * normalVector;
                CurrentIOR = outIOR;
                return true;

            }


            if (angleBetween < -90 || angleBetween > 90)
            {
                normalVector = normalVector * -1;
            }

            Direction = c1 * Direction + (-(c2) * c1 + Math.Sqrt(1 - c1 * c1 * (1.0 - c2 * c2))) * normalVector;
            CurrentIOR = outIOR;
            return true;

        }

    }

}
