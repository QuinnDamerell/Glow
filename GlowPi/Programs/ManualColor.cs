using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using GlowPi.Interfaces;
using GlowCommon.DataObjects.Commands;
using GlowCommon;

namespace GlowPi.Programs
{
    class ManualColor : IProgram
    {
        bool m_isActive = false;
        bool m_isFirstSet = true;         
        ManualColorSettings m_settings = new ManualColorSettings(true);
        IProgramController m_controller;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;
        }

        public void Activate()
        {
            m_isFirstSet = true;
            m_isActive = true;            

            // Lower the work time since we don't use the callback
            m_controller.SetWorkRate(500);

            // Set the LEDs to the current values.
            SetCurrentValues();
        }

        public void Deactivate()
        {
            m_isActive = false;
        }

        public void DoWork(uint timeElaspedMs)
        {
           // We only will do work with command updates.
        }

        public Command CommandRecieved(Command command)
        {
            // Switch on message type
            switch(command.MessageId)
            {
                case (uint)Command.COMMAND_RECIEVE_SETTINGS:
                    UpdateSettings(command);
                    break;
                case (uint)Command.COMMAND_GET_SETTINGS:
                    return GetSettingsCommand();
            }
            return null;
        }

        private void SetCurrentValues()
        {
            // Don't set the LEDs if we aren't active.
            if(!m_isActive)
            {
                return;
            }

            if(m_settings.CurrentLedStateList.Count > 0)
            {
                List<SerlizableLed> localList = m_settings.CurrentLedStateList[0];
                m_controller.GetLed(0).Animate(localList[0].Red, localList[0].Green, localList[0].Blue, localList[0].Intensity, TimeSpan.FromMilliseconds(m_isFirstSet ? 1000 : localList[0].ChangeSpeedMs), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(1).Animate(localList[1].Red, localList[1].Green, localList[1].Blue, localList[1].Intensity, TimeSpan.FromMilliseconds(m_isFirstSet ? 1000 : localList[1].ChangeSpeedMs), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(2).Animate(localList[2].Red, localList[2].Green, localList[2].Blue, localList[2].Intensity, TimeSpan.FromMilliseconds(m_isFirstSet ? 1000 : localList[2].ChangeSpeedMs), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(3).Animate(localList[3].Red, localList[3].Green, localList[3].Blue, localList[3].Intensity, TimeSpan.FromMilliseconds(m_isFirstSet ? 1000 : localList[3].ChangeSpeedMs), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(4).Animate(localList[4].Red, localList[4].Green, localList[4].Blue, localList[4].Intensity, TimeSpan.FromMilliseconds(m_isFirstSet ? 1000 : localList[4].ChangeSpeedMs), WindowsIotLedDriver.AnimationType.Linear);
            }
            else
            {
                // Reset them
                m_controller.GetLed(0).Animate(0,0,0,1,TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(1).Animate(0,0,0,1,TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(2).Animate(0,0,0,1,TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(3).Animate(0,0,0,1,TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(4).Animate(0,0,0,1,TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
            }

            m_isFirstSet = false;
        }

        // Updates the settings given a settings command.
        private void UpdateSettings(Command command)
        {
            // Get the settings from the message
            ManualColorSettings newSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<ManualColorSettings>(command.Message);
            if(newSettings == null)
            {
                return;
            }

            // Set the new settings
            m_settings = newSettings;
            m_settings.SaveSettings();

            // Update the LEDs
            SetCurrentValues();     
        }

        // Returns a command for the current settings
        private Command GetSettingsCommand()
        {
            // Create the message
            Command cmd = new Command();
            cmd.Program = GlowPrograms.ManualColors;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(m_settings);
            return cmd;
        }
    }
}
