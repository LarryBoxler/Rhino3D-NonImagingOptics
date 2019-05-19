using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Non_Imaging
{
    public class OpticalInterface
    {
        public double reflectivity { get; set; }
        public double transmission { get; set; }
        public double n1 { get; set; }
        public double n2 { get; set; }

        public OpticalInterface()
        {
            reflectivity = 0.0;
            transmission = 0.0;
            n1 = 1.0;
            n2 = 1.0;
        }

        public OpticalInterface(double index1, double index2)
        {
            n1 = index1;
            n2 = index2;
            if (index1 == index2)
            {
                reflectivity = 1.0;
                transmission = 0.0;
            }

            else
            {
                reflectivity = 0.0;
                transmission = 1.0;
            }
        }

        public OpticalInterface(double index1, double index2, double Reflection, double Transmission)
        {
            n1 = index1;
            n2 = index2;
            reflectivity = Reflection;
            transmission = Transmission;
        }

    }
}
