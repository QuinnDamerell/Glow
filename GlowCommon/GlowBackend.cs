using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon
{
    public class SerlizableLed
    {
        public double Red = 0.0;
        public double Green = 0.0;
        public double Blue = 0.0;
        public double Intensity = 0.0;

        public SerlizableLed(double red, double green, double blue, double intensity)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Intensity = intensity;
        }
    }

    public enum GlowPrograms
    {
        GlowControl = 0,
        ManualColors,
        Clock,
        Weather,
        WeatherCam,
        None
    }

    public class GlowBackend
    {


    }
}
