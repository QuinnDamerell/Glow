using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;

namespace GlowCommon.Interfaces
{
    // Listens for commands from the command server.
    public interface ICommandListener
    {
        // Fired when a command is sent
        void OnCommand(Command command);
    }
}
