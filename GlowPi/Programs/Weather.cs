using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using GlowPi.Interfaces;

namespace GlowPi.Programs
{
    class Weather : IProgram
    {
        public void InitProgram(IProgramController controller)
        {

        }

        public void Activate()
        {
      
        }

        public void Deactivate()
        {
          
        }

        public void DoWork(uint timeElaspedMs)
        {
            System.Diagnostics.Debug.WriteLine("Weather do work");
        }

        public void CommandRecieved(Command command)
        {
            throw new NotImplementedException();
        }
    }
}
