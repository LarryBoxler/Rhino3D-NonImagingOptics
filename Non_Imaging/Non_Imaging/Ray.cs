using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Non_Imaging
{
    public class Ray
    {
        public double currentIOR { get; set; }
        public Vector3d direction { get; set; }
        public List<Point3d> raypath { get; set; }

        public Ray()
        {
            currentIOR = 1.000;
            direction = new Vector3d(0, 0, 1);
            raypath = new List<Point3d>();
            raypath.Add(new Point3d(0, 0, 0));
        }

        public Ray(Curve rayCurve)
        {
            currentIOR = 1.000;
            direction = new Vector3d((rayCurve.PointAtEnd - rayCurve.PointAtStart));
            raypath = new List<Point3d>();
            raypath.Add(rayCurve.PointAtStart);
        }

        public Boolean getIntersection(Curve[] traceCurves, out Vector3d normalVector, out OpticalInterface oInterface)
        {
            Point3d pointOnCurve;
            Point3d pointOnObject;
            int whichGeometry;
            double tangent;
            normalVector = new Vector3d();
            oInterface = new OpticalInterface();

            direction = direction * (1 / direction.Length);

            direction = direction * 0.1;

            Line line = new Line(raypath[raypath.Count - 1], direction);
            LineCurve inRay = new LineCurve(line);

            Curve curve = inRay.Extend(CurveEnd.End, CurveExtensionStyle.Line, traceCurves);
            if (curve == null)
                return false;

            direction = direction * -1;
            Line intline = new Line(curve.PointAtEnd, direction);
            LineCurve intcurve = new LineCurve(intline);
            direction = direction * -1;

            var curveint = intcurve.ClosestPoints(traceCurves, out pointOnCurve, out pointOnObject, out whichGeometry, 0.1);
            if (curveint == false)
                return false;

            var tanPoint = traceCurves[whichGeometry].ClosestPoint(pointOnObject, out tangent);
            if (tanPoint == false)
                return false;

            Vector3d tanVector = traceCurves[whichGeometry].TangentAt(tangent);
            Vector3d axisVector = Vector3d.CrossProduct(direction, tanVector);
            if (tanVector.Rotate(RhinoMath.ToRadians(90), axisVector) == false)
                return false;

            normalVector = tanVector;

            double angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, direction));
            if (angleBetween < -90 || angleBetween > 90)
                normalVector = normalVector * -1;

            raypath.Add(pointOnObject);

            oInterface.n1 = traceCurves[whichGeometry].UserDictionary.GetDouble("Index1");
            oInterface.n2 = traceCurves[whichGeometry].UserDictionary.GetDouble("Index2");
            oInterface.reflectivity = traceCurves[whichGeometry].UserDictionary.GetDouble("Reflectivity");
            oInterface.transmission = traceCurves[whichGeometry].UserDictionary.GetDouble("Transmission");

            return true;

        }

        public Boolean Reflect(Vector3d normalVector)
        {
            direction = direction * (1 / direction.Length);
            direction = direction - 2 * (direction * normalVector) * normalVector;
            if (direction == null)
                return false;
            return true;
        }

        public Boolean Refract(Vector3d normalVector, OpticalInterface oInterface)
        {

            double inIOR;
            double outIOR;
            direction = direction * (1 / direction.Length);
            normalVector = normalVector * (1 / normalVector.Length);

            if (currentIOR == oInterface.n1)
            {
                inIOR = currentIOR;
                outIOR = oInterface.n2;
            }

            else if (currentIOR == oInterface.n2)
            {
                inIOR = currentIOR;
                outIOR = oInterface.n1;
            }

            else
                return false;

            double c1 = inIOR / outIOR;
            double c2 = direction * normalVector;
            double angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, direction));

            if (inIOR > outIOR)
            {

                double criticalAngle = RhinoMath.ToDegrees(Math.Asin(outIOR / inIOR));

                if (angleBetween < -90 || angleBetween > 90)
                {
                    normalVector = normalVector * -1;
                    angleBetween = RhinoMath.ToDegrees(Vector3d.VectorAngle(normalVector, direction));
                }

                if (angleBetween > criticalAngle)
                {
                    if (Reflect(normalVector) == true)
                        return true;
                    else
                        return false;
                }

                direction = c1 * direction + (-(c2) * c1 + Math.Sqrt(1 - c1 * c1 * (1.0 - c2 * c2))) * normalVector;
                currentIOR = outIOR;
                return true;

            }


            if (angleBetween < -90 || angleBetween > 90)
            {
                normalVector = normalVector * -1;
            }

            direction = c1 * direction + (-(c2) * c1 + Math.Sqrt(1 - c1 * c1 * (1.0 - c2 * c2))) * normalVector;
            currentIOR = outIOR;
            return true;

        }

    }

}
