using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;

namespace GlowPi.Interfaces
{
    interface IProgram
    {
        void InitProgram(IProgramController controller);

        void Activate();

        void Deactivate();

        /// <summary>
        /// Tells the program to do work.
        /// </summary>
        /// <param name="timeElaspedMs">time elasped since the last call</param>
        void DoWork(uint timeElaspedMs);

        /// <summary>
        /// Called when a command has been received for the program.
        /// If a command is returned it will be sent back to the sender.
        /// </summary>
        /// <param name="command">The recieved commands</param>
        Command CommandRecieved(Command command);
    }
}
