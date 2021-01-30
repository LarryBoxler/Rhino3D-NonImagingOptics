using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Non_Imaging
{
    [System.Runtime.InteropServices.Guid("ad90e7ca-7994-43c7-916f-cdaed6bf823f")]
    public class NIRayTrace : Command
    {
        static NIRayTrace _instance;
        public NIRayTrace()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NIRayTrace command.</summary>
        public static NIRayTrace Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NIRayTrace"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetObject getRays = new GetObject();
            getRays.SetCommandPrompt("Select Rays to Trace");
            getRays.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            getRays.GetMultiple(1, 0);
            if (getRays.CommandResult() != Rhino.Commands.Result.Success)
                return getRays.CommandResult();

            GetObject getCurves = new GetObject();
            getCurves.DisablePreSelect();
            getCurves.SetCommandPrompt("Select Curves to Trace");
            getCurves.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            getCurves.GetMultiple(1, 0);
            if (getCurves.CommandResult() != Rhino.Commands.Result.Success)
                return getCurves.CommandResult();


            Rhino.DocObjects.ObjRef[] geomCurves = getCurves.Objects();
            Curve[] traceCurves = new Curve[geomCurves.Length];
            for (int i = 0; i < geomCurves.Length; i++)
            {
                traceCurves[i] = geomCurves[i].Curve();
                if (traceCurves[i].HasUserData != true)
                {
                    geomCurves[i].Geometry().UserDictionary.Set("Index1", 0.0);
                    geomCurves[i].Geometry().UserDictionary.Set("Index2", 0.0);
                    geomCurves[i].Geometry().UserDictionary.Set("Reflectivity", 0.0);
                    geomCurves[i].Geometry().UserDictionary.Set("Transmission", 0.0);
                }

                else if (traceCurves[i].UserDictionary.ContainsKey("Index1") != true)
                {
                    geomCurves[i].Geometry().UserDictionary.Set("Index1", 0.0);
                    geomCurves[i].Geometry().UserDictionary.Set("Index2", 0.0);
                    geomCurves[i].Geometry().UserDictionary.Set("Reflectivity", 0.0);
                    geomCurves[i].Geometry().UserDictionary.Set("Transmission", 0.0);
                }

            }


            Rhino.DocObjects.ObjRef[] tempCurves = getRays.Objects();
            Curve[] rayCurves = new Curve[tempCurves.Length];
            for (int i = 0; i < tempCurves.Length; i++)
            {
                rayCurves[i] = tempCurves[i].Curve();
            }



            Vector3d normalVector = new Vector3d();
            int maxdepth = 30;
            OpticalInterface oInterface = new OpticalInterface();

            for (int i = 0; i < rayCurves.Length; i++)
            {

                int depth = 0;

                Ray traceray = new Ray(rayCurves[i]);

                var rayint = traceray.getIntersection(traceCurves, out normalVector, out oInterface);
                if (rayint == false)
                    continue;

                do
                {

                    if (oInterface.n1 == 0)
                    {
                        rayint = false;
                        break;
                    }

                    else if (oInterface.n1 == oInterface.n2)
                        traceray.Reflect(normalVector);
                    else
                        traceray.Refract(normalVector, oInterface);

                    rayint = traceray.getIntersection(traceCurves, out normalVector, out oInterface);

                    depth++;

                    if (depth > maxdepth)
                        break;

                } while (rayint == true);


                if (oInterface.n1 == 0)
                    traceray.Raypath.Add(traceray.Raypath[traceray.Raypath.Count - 1]);
                else
                    traceray.Raypath.Add(traceray.Raypath[traceray.Raypath.Count - 1] + (30 * traceray.Direction));


                doc.Objects.AddCurve(new PolylineCurve(traceray.Raypath));

                doc.Views.Redraw();

            }

            return Result.Success;
        }
    }
}
