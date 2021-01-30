using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace Non_Imaging
{
    public class NISurfaceTrace : Command
    {
        static NISurfaceTrace _instance;
        public NISurfaceTrace()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NISurfaceTrace command.</summary>
        public static NISurfaceTrace Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NISurfaceTrace"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetObject getRays = new GetObject();
            getRays.SetCommandPrompt("Select Rays to Trace");
            getRays.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            getRays.GetMultiple(1, 0);
            if (getRays.CommandResult() != Rhino.Commands.Result.Success)
                return getRays.CommandResult();


            GetObject getSurfaces = new GetObject();
            getSurfaces.DisablePreSelect();
            getSurfaces.SetCommandPrompt("Select Surfaces to Trace");
            getSurfaces.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
            getSurfaces.GetMultiple(1, 0);
            if (getSurfaces.CommandResult() != Rhino.Commands.Result.Success)
                return getSurfaces.CommandResult();

            Rhino.DocObjects.ObjRef[] geomSurfaces = getSurfaces.Objects();
            GeometryBase[] traceGeometry = new GeometryBase[geomSurfaces.Length];
            //Surface[] traceSurfaces = new Surface[geomSurfaces.Length];
            for (int i = 0; i < geomSurfaces.Length; i++)
            {
                traceGeometry[i] = geomSurfaces[i].Geometry();
                //traceSurfaces[i] = geomSurfaces[i].Surface();
                if (geomSurfaces[i].Geometry().HasUserData != true)
                {
                    traceGeometry[i].UserDictionary.Set("Index1", 0.0);
                    traceGeometry[i].UserDictionary.Set("Index2", 0.0);
                    traceGeometry[i].UserDictionary.Set("Reflectivity", 0.0);
                    traceGeometry[i].UserDictionary.Set("Transmission", 0.0);
                }

                else if (geomSurfaces[i].Geometry().UserDictionary.ContainsKey("Index1") != true)
                {
                    traceGeometry[i].UserDictionary.Set("Index1", 0.0);
                    traceGeometry[i].UserDictionary.Set("Index2", 0.0);
                    traceGeometry[i].UserDictionary.Set("Reflectivity", 0.0);
                    traceGeometry[i].UserDictionary.Set("Transmission", 0.0);
                }

            }

            ObjRef[] tempCurves = getRays.Objects();
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

                var rayint = traceray.getIntersection(geomSurfaces, out normalVector, out oInterface);
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

                    rayint = traceray.getIntersection(geomSurfaces, out normalVector, out oInterface);

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