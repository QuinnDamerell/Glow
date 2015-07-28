using GlowCommon.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glow.Interfaces
{
    interface IProgramPane
    {
        void OnCommand(Command cmd);

        void OnProgramListChanged();
    }
}
