using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowPi.Interfaces;
using WindowsIotLedDriver;

namespace GlowPi.Programs
{
    class Clock : IProgram
    {
        IProgramController m_controller;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;
        }

        public void Activate()
        {
            // Turn off the 5th ball since we don't use it.
            m_controller.GetLed(4).Animate(0, 0, 0, 1.0, new TimeSpan(0, 0, 1), AnimationType.Linear);

            // Set the work rate lower so we get faster callbacks.
            m_controller.SetWorkRate(100);
        }

        public void Deactivate()
        {
            // Do nothing for deactivate
        }

        public void DoWork()
        {
            // Get the time now. 
            DateTime now = DateTime.Now;

            // Get the current time percentages
            int currentHours = now.Hour > 12 ? now.Hour - 12 : now.Hour;
            double days = now.Day / 31.0;
            double hours = currentHours / 12.0;
            double minutes = now.Minute / 60.0;
            double seconds = now.Second / 60.0;

            // For gray scale, just send those values down for all.
            m_controller.GetLed(0).Animate(days, days, days, 1.0, new TimeSpan(0, 0, 1), AnimationType.Linear);
            m_controller.GetLed(1).Animate(hours, hours, hours, 1.0, new TimeSpan(0, 0, 1), AnimationType.Linear);
            m_controller.GetLed(2).Animate(minutes, minutes, minutes, 1.0, new TimeSpan(0, 0, 1), AnimationType.Linear);
            m_controller.GetLed(3).Animate(seconds, seconds, seconds, 1.0, new TimeSpan(0, 0, 0, 0, 500), AnimationType.Linear);
        }
    }
}
