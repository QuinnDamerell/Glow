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
        /// <summary>
        /// Fired when a command is received. If a command is returned it will be sent back
        /// to the sender of the first command.
        /// </summary>
        /// <param name="command">The reciefed command</param>
        /// <returns>A return command if needed.</returns>
        Command OnCommand(Command command);
    }
}
