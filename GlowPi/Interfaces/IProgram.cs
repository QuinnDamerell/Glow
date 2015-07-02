using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
