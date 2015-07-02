using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIotLedDriver;

namespace GlowPi.Interfaces
{
    interface IProgramController
    {
        AnimatedLed GetLed(int ledNumber);

        void SetWorkRate(uint workRateMs);
    }
}
