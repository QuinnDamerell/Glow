using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using GlowPi.Interfaces;
using WindowsIotLedDriver;

namespace GlowPi.Programs
{
    class Clock : IProgram
    {
        IProgramController m_controller;
        ClockSettings m_settings = new ClockSettings(true);

        double[] m_redRange =   { 1, 1, 0, 0, 0, 1 };
        double[] m_blueRange =  { 0, 1, 1, 1, 0, 0 };
        double[] m_greenRange = { 0, 0, 0, 1, 1, 1 };

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

        public void DoWork(uint timeElaspedMs)
        {
            // Get the time now. 
            DateTime now = DateTime.Now;

            // Get the current time percentages
            int currentHours = now.Hour > 12 ? now.Hour - 12 : now.Hour;
            double month = now.Month / 12.0;
            double days = now.Day / 31.0;
            double hours = currentHours / 12.0;
            double minutes = now.Minute / 60.0;
            double seconds = now.Second / 60.0;

            if(m_settings.ColorType == ClockSettings.ColorTypes.GrayScale)
            {
                m_controller.GetLed(0).Animate(month, month, month, 1.0, new TimeSpan(0, 0, 3), AnimationType.Linear);
                m_controller.GetLed(1).Animate(days, days, days, 1.0, new TimeSpan(0, 0, 3), AnimationType.Linear);
                m_controller.GetLed(2).Animate(hours, hours, hours, 1.0, new TimeSpan(0, 0, 3), AnimationType.Linear);
                m_controller.GetLed(3).Animate(minutes, minutes, minutes, 1.0, new TimeSpan(0, 0, 3), AnimationType.Linear);
                m_controller.GetLed(4).Animate(seconds, seconds, seconds, 1.0, new TimeSpan(0, 0, 0, 0, 500), AnimationType.Linear);
            }
            else
            {
                SetColor(m_controller.GetLed(0), month, new TimeSpan(0, 0, 3));
                SetColor(m_controller.GetLed(1), days, new TimeSpan(0, 0, 3));
                SetColor(m_controller.GetLed(2), hours, new TimeSpan(0, 0, 3));
                SetColor(m_controller.GetLed(3), minutes, new TimeSpan(0, 0, 3));
                SetColor(m_controller.GetLed(4), seconds, new TimeSpan(0, 0, 0, 0, 500));
            }
        }

        public Command CommandRecieved(Command command)
        {
            switch(command.MessageId)
            {
                case Command.COMMAND_RECIEVE_SETTINGS:
                    {
                        m_settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ClockSettings>(command.Message);
                        break;
                    }
                case Command.COMMAND_GET_SETTINGS:
                    {
                        // Send the settings
                        Command settingsCommand = new Command();
                        settingsCommand.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
                        settingsCommand.Program = GlowCommon.GlowPrograms.Clock;
                        settingsCommand.Message = Newtonsoft.Json.JsonConvert.SerializeObject(m_settings);
                        return settingsCommand;
                    }
            }
            return null;
        }

        private void SetColor(AnimatedLed led, double value, TimeSpan time)
        {
            double red = 0, green = 0, blue = 0;

            // Wrap the value if we hit 1.
            if (value == 1)
            {
                value = 0;
            }

            // Find what bucket we are in
            int rangeBot = (int)Math.Floor(((value) * m_blueRange.Length));
            int rangeTop = rangeBot + 1;
            if (rangeTop == m_blueRange.Length)
            {
                rangeTop = 0;
            }

            // Find where we are in that bucket-
            double placeInRange = ((value * m_blueRange.Length * m_blueRange.Length) % m_blueRange.Length) / 6.0;
            if(value == 1.0)
            {
                // Special case
                placeInRange = 1;
            }

            // Find the values per color
            red = m_redRange[rangeBot] + (m_redRange[rangeTop] - m_redRange[rangeBot]) * placeInRange;
            green = m_greenRange[rangeBot] + (m_greenRange[rangeTop] - m_greenRange[rangeBot]) * placeInRange;
            blue = m_blueRange[rangeBot] + (m_blueRange[rangeTop] - m_blueRange[rangeBot]) * placeInRange;

            // Animate the LED
            led.Animate(red, green, blue, 1.0, time, AnimationType.Linear);
        }
    }
}
