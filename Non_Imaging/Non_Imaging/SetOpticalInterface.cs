using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Non_Imaging
{
    [System.Runtime.InteropServices.Guid("cfe1803b-9c8c-42b0-a539-a28c895809b9")]
    public class NISetOpticalInterface : Command
    {
        static NISetOpticalInterface _instance;
        public NISetOpticalInterface()
        {
            _instance = this;
        }

        ///<summary>The only instance of the NISetOpticalInterface command.</summary>
        public static NISetOpticalInterface Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NISetOpticalInterface"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            string interfaceType = "nothing";

            GetOption go = new GetOption();
            go.SetCommandPrompt("Optical Interface Type to Create:");
            go.AddOption("Reflective");
            go.AddOption("Refractive");
            go.AddOption("Absorbing");

            if (go.Get() == GetResult.Option)
                interfaceType = go.Option().EnglishName;

            OpticalInterface oInterface = new OpticalInterface();

            OptionDouble index1 = new OptionDouble(1.000);
            OptionDouble index2 = new OptionDouble(1.489);


            if (interfaceType == "Reflective")
                oInterface = new OpticalInterface(1.0, 1.0);

            else if (interfaceType == "Absorbing")
                oInterface = new OpticalInterface(0.0, 0.0);

            else if (interfaceType == "Refractive")
            {
                GetOption go2 = new GetOption();
                go2.SetCommandPrompt("Index of Refraction for Interface:");
                go2.AddOptionDouble("n1", ref index1);
                go2.AddOptionDouble("n2", ref index2);
                oInterface = new OpticalInterface(index1.CurrentValue, index2.CurrentValue);

                while (go2.Get() != GetResult.Cancel)
                {
                    if (go2.Get() != GetResult.NoResult)
                    {

                        oInterface.n1 = index1.CurrentValue;
                        oInterface.n2 = index2.CurrentValue;
                    }
                }
            }

            else
                oInterface = new OpticalInterface(0.0, 0.0);

            GetObject getInterfaces = new GetObject();
            getInterfaces.SetCommandPrompt("Select Curves to set Optical Interface " + interfaceType);
            getInterfaces.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            getInterfaces.GetMultiple(1, 0);
            if (getInterfaces.CommandResult() != Rhino.Commands.Result.Success)
                return getInterfaces.CommandResult();

            Rhino.DocObjects.ObjRef[] geomCurves = getInterfaces.Objects();

            for (int i = 0; i < geomCurves.Length; i++)
            {
                geomCurves[i].Geometry().UserDictionary.Set("Index1", oInterface.n1);
                geomCurves[i].Geometry().UserDictionary.Set("Index2", oInterface.n2);
                geomCurves[i].Geometry().UserDictionary.Set("Reflectivity", oInterface.reflectivity);
                geomCurves[i].Geometry().UserDictionary.Set("Transmission", oInterface.transmission);
            }


            return Result.Success;
        }
    }
}
