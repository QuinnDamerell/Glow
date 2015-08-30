using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIotLedDriver;
using GlowCommon;
using GlowCommon.DataObjects;

namespace GlowPi.Interfaces
{
    interface IProgramController
    {
        AnimatedLed GetLed(int ledNumber);

        void SetWorkRate(uint workRateMs);

        void ToggleProgram(GlowPrograms program, bool enable);

        bool IsProgramEnabled(GlowPrograms program);

        void AnimateMasterIntensity(double intensity, TimeSpan animationTime);

        double GetMasterIntensity();

        bool IsMasterIntensityAnimating();
    }
}
