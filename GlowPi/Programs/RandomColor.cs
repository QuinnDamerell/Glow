using GlowPi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;

namespace GlowPi.Programs
{
    class RandomColor : IProgram
    {
        const int c_timeUper = 3000;
        const int c_timeLower = 300;

        int[] m_expireTime = new int[5];
        Random m_random;
        IProgramController m_controller;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;
            m_random = new Random();
        }

        public void Activate()
        {
            // Setup the random times.
            for(int i = 0; i < m_expireTime.Length; i++)
            {
                m_expireTime[i] = m_random.Next(c_timeLower, c_timeUper);
            }
        }

        public Command CommandRecieved(Command command)
        {
            return null;
        }

        public void Deactivate()
        {

        }

        public void DoWork(uint timeElaspedMs)
        {
            for (int i = 0; i < m_expireTime.Length; i++)
            {
                m_expireTime[i] -= (int)timeElaspedMs;

                if(m_expireTime[i] < 0)
                {
                    // We need to update this LED
                    m_expireTime[i] = m_random.Next(c_timeLower, c_timeUper);
                    m_controller.GetLed(i).Animate(m_random.NextDouble(), m_random.NextDouble(), m_random.NextDouble(), 1, TimeSpan.FromMilliseconds(m_expireTime[i]), WindowsIotLedDriver.AnimationType.Linear);
                }                    
            }
        }
    }
}
