using GlowCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glow.Interfaces
{
    public interface IProgramController
    {
        bool GetProgramState(GlowPrograms program);

        void ToggleProgram(GlowPrograms program, bool enable);
    }
}
