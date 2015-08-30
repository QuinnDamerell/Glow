using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowPi.Interfaces;
using GlowCommon;
using GlowCommon.DataObjects;

namespace GlowPi.Programs
{
    class GlowControl : IProgram
    {
        IProgramController m_controller;
        GlowControlSettings m_settings = new GlowControlSettings(true);

        // Cycle vars
        int m_nextCycleTimeMs;
        bool m_userActionCycle = false;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;

            // Enable the background programs
            m_controller.ToggleProgram(GlowPrograms.GlowControl, true);
            m_controller.ToggleProgram(GlowPrograms.DiscoveryControl, true);

            // Check if the settings need inited
            if (m_settings.ProgramStateList.Count == 0)
            {
                // Create all of the programs with their defaults.
                m_settings.ProgramStateList.Clear();
                m_settings.ProgramStateList.Add(GlowPrograms.GlowControl, GlowControlSettings.ProgramState.Background);
                m_settings.ProgramStateList.Add(GlowPrograms.DiscoveryControl, GlowControlSettings.ProgramState.Background);
                m_settings.ProgramStateList.Add(GlowPrograms.ManualColors, GlowControlSettings.ProgramState.Disabled);
                m_settings.ProgramStateList.Add(GlowPrograms.Clock, GlowControlSettings.ProgramState.Disabled);
                m_settings.ProgramStateList.Add(GlowPrograms.Weather, GlowControlSettings.ProgramState.Disabled);
                m_settings.ProgramStateList.Add(GlowPrograms.WeatherCam, GlowControlSettings.ProgramState.Disabled);
                m_settings.ProgramStateList.Add(GlowPrograms.RandomColor, GlowControlSettings.ProgramState.Eligible);
                m_settings.SaveSettings();
            }

            // Negate the cycle time so we pick up a program.
            m_nextCycleTimeMs = -10;
        }

        public void Activate()
        {
            // We don't need to do anything here
        }

        public void Deactivate()
        {
            // This should never be called
        }

        public void DoWork(uint timeElaspedMs)
        {
            // Subtract time remaining
            m_nextCycleTimeMs -= (int)timeElaspedMs;

            if(m_nextCycleTimeMs < 0)
            {
                // Set the intensity
                double desiredIntensity = -1;
                TimeSpan now = DateTime.Now.TimeOfDay;
                // If the end time is greater than the start time, do the normal compare (start > currentTime > end)
                // If the end time is less than the start time, then if the current time is greater than the start time we are good, or if the current time is less than than end time we are good.
                if((m_settings.SleepyTimeEnd.CompareTo(m_settings.SleepyTimeStart) > 0 && now.CompareTo(m_settings.SleepyTimeStart) > 0 && now.CompareTo(m_settings.SleepyTimeEnd) < 0) ||
                   (m_settings.SleepyTimeEnd.CompareTo(m_settings.SleepyTimeStart) < 0 && (now.CompareTo(m_settings.SleepyTimeStart) > 0 || now.CompareTo(m_settings.SleepyTimeEnd) < 0)))
                {
                    desiredIntensity = m_settings.SleepyTimeIntensity;
                }
                else
                {
                    desiredIntensity = m_settings.MasterIntensity;
                }

                // Clamp the intensity
                desiredIntensity = Math.Min(1.0, Math.Max(0, desiredIntensity));

                // Check to see if we need to update the intensity. Note we always want to do it now
                // if this was from a user action, but if not we don't want to do it if we are already
                // running the animation.
                if(desiredIntensity != m_controller.GetMasterIntensity() &&
                    (m_userActionCycle || !m_controller.IsMasterIntensityAnimating()))
                {
                    m_controller.AnimateMasterIntensity(desiredIntensity, m_userActionCycle ? new TimeSpan(0,0,0,0,200) : new TimeSpan(0, 0, 10));
                }

                GlowPrograms runningProgram = GlowPrograms.None;
                GlowPrograms nextProgram = GlowPrograms.None;
                foreach(KeyValuePair<GlowPrograms, GlowControlSettings.ProgramState> programState in m_settings.ProgramStateList)
                {
                    // Forget about anything that is running in the background.
                    if(programState.Value != GlowControlSettings.ProgramState.Background)
                    {
                        // If we found a program, look for the next possible program to run.
                        if(runningProgram != GlowPrograms.None && programState.Value == GlowControlSettings.ProgramState.Eligible)
                        {
                            nextProgram = programState.Key;
                            break;
                        }

                        if(m_controller.IsProgramEnabled(programState.Key))
                        {
                            // We found the running program
                            runningProgram = programState.Key;
                        }
                    }   
                }

                if (runningProgram != GlowPrograms.None)
                {
                    // We found the running, lets see if we have a next
                    if(nextProgram == GlowPrograms.None)
                    {
                        // We need to loop back through the first part of the loop to find the
                        // next program.
                        foreach (KeyValuePair<GlowPrograms, GlowControlSettings.ProgramState> programState in m_settings.ProgramStateList)
                        {
                            if (programState.Key == runningProgram)
                            {
                                // We looped all the way through and didn't find anything.
                                break;
                            }

                            if (programState.Value == GlowControlSettings.ProgramState.Eligible)
                            {
                                // We found a new program
                                nextProgram = programState.Key;
                                break;
                            }
                        }
                    }

                    if(nextProgram != GlowPrograms.None)
                    {
                        // We found the running and the next, switch it out!
                        m_controller.ToggleProgram(runningProgram, false);
                        m_controller.ToggleProgram(nextProgram, true);
                    }
                    else
                    {
                        // Check if this program should be running.
                        if(m_settings.ProgramStateList[runningProgram] != GlowControlSettings.ProgramState.Eligible)
                        {
                            // We need to stop this program and put the system to sleep.      
                            m_controller.ToggleProgram(runningProgram, false);
                            GoToSleepState();
                        }
                    }                    
                }
                else
                {
                    // We didn't find anything running nor anything to be next. Loop through to see if we can find
                    // a program that we can run.
                    bool programFound = false;
                    foreach (KeyValuePair<GlowPrograms, GlowControlSettings.ProgramState> programState in m_settings.ProgramStateList)
                    {
                        if(programState.Value == GlowControlSettings.ProgramState.Eligible)
                        {
                            m_controller.ToggleProgram(programState.Key, true);
                            programFound = true;
                            break;
                        }
                    }

                    // If we didn't find anything we are off.
                    if(!programFound)
                    {
                        GoToSleepState();
                    }
                }

                // Reset the timers
                m_nextCycleTimeMs = (int)m_settings.ProgramCycleTimeMs;
                m_userActionCycle = false;
            }
        }

        public Command CommandRecieved(Command command)
        {
            switch(command.MessageId)
            {
                case Command.COMMAND_GET_SETTINGS:
                    return GetCurrentSettings();
                case Command.COMMAND_RECIEVE_SETTINGS:
                    HandleNewSettings(command.Message);
                    break;
            }
            return null;
        }

        private Command GetCurrentSettings()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(m_settings);

            // Make the command
            Command cmd = new Command();
            cmd.Message = json;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Program = GlowPrograms.GlowControl;
            return cmd;
        }

        private void HandleNewSettings(string json)
        {
            // Parse the new settings.
            m_settings = Newtonsoft.Json.JsonConvert.DeserializeObject<GlowControlSettings>(json);
            m_settings.SaveSettings();

            // Set the work time to something that will make it operate next time.
            m_nextCycleTimeMs = -10;
            m_userActionCycle = true;
        }
        
        private void GoToSleepState()
        {
            // In the sleep state we want to turn off the balls...
            m_controller.GetLed(0).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 5), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(1).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 5), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(2).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 5), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(3).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 5), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(4).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 5), WindowsIotLedDriver.AnimationType.Linear);

            // And slow down the work tick.
            m_controller.SetWorkRate(500);

            // Note the work tick will still fire which will cause us to keep listening for commands.
        }
    }
}
