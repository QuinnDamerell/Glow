using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects.Commands
{
    public class ManualColorCommands
    {
        public enum ManualColorCommand
        {
            SetColors
        }

        public class ManualColorLed
        {
            public double Red = 0.0;
            public double Green = 0.0;
            public double Blue = 0.0;
            public double Intensity = 0.0;

            public ManualColorLed(double red, double green, double blue, double intensity)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Intensity = intensity;
            }
        }

        public class SetColors
        {
            public List<ManualColorLed> ColorList = new List<ManualColorLed>(5);
        }
    }
}
