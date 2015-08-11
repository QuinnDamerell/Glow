using GlowPi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using GlowCommon;

namespace GlowPi.Programs
{
    class RandomColor : IProgram
    {
        IProgramController m_controller;
        RandomColorSettings m_settings = new RandomColorSettings(true);
        int[] m_expireTime = new int[5];
        Random m_random;

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
                    m_controller.GetLed(i).Animate(m_random.NextDouble(), m_random.NextDouble(), m_random.NextDouble(), 1, TimeSpan.FromMilliseconds(animationTime), WindowsIotLedDriver.AnimationType.Linear);
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
    }
}
