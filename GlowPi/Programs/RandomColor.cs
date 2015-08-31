using GlowPi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using GlowCommon;
using WindowsIotLedDriver;

namespace GlowPi.Programs
{
    class RandomColor : IProgram
    {
        IProgramController m_controller;
        RandomColorSettings m_settings = new RandomColorSettings(true);
        int[] m_expireTime = new int[5];
        Random m_random;

        double[] m_redRange = { 1, 1, 0, 0, 0, 1 };
        double[] m_blueRange = { 0, 1, 1, 1, 0, 0 };
        double[] m_greenRange = { 0, 0, 0, 1, 1, 1 };

        public GlowPrograms GlowProgram { get; private set; }

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
                m_expireTime[i] = m_random.Next(m_settings.NextUpdateTimeMinMs, m_settings.NextUpdateTimeMaxMs);
            }

            // Set the timer low in case we need it
            m_controller.SetWorkRate(16);
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
                    // We need to update this LED, update the next time we will update this led
                    m_expireTime[i] = m_random.Next(m_settings.NextUpdateTimeMinMs, m_settings.NextUpdateTimeMaxMs);

                    // Create the animation time.
                    int animationTime = m_random.Next(m_settings.TransistionMinMs, m_settings.TransistionMaxMs);
                    SetColor(m_controller.GetLed(i), m_random.NextDouble(), TimeSpan.FromMilliseconds(animationTime));                   
                }                    
            }
        }

        public Command CommandRecieved(Command command)
        {
            // Switch on message type
            switch (command.MessageId)
            {
                case (uint)Command.COMMAND_RECIEVE_SETTINGS:
                    UpdateSettings(command);
                    break;
                case (uint)Command.COMMAND_GET_SETTINGS:
                    return GetSettingsCommand();
            }
            return null;
        }

        // Returns a command for the current settings
        private Command GetSettingsCommand()
        {
            // Create the message
            Command cmd = new Command();
            cmd.Program = GlowPrograms.RandomColor;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(m_settings);
            return cmd;
        }

        // Updates the settings given a settings command.
        private void UpdateSettings(Command command)
        {
            // Get the settings from the message
            RandomColorSettings newSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<RandomColorSettings>(command.Message);
            if (newSettings == null)
            {
                return;
            }

            // Set the new settings
            m_settings = newSettings;
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
            if (value == 1.0)
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
