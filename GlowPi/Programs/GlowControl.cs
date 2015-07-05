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
        GlowControlSettings m_settings = new GlowControlSettings();

        // Cycle vars
        int m_nextCycleTimeMs;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;

            // Enable the default programs
            m_controller.ToggleProgram(GlowPrograms.GlowControl, true);
            m_controller.ToggleProgram(GlowPrograms.ManualColors, true);

            // Check if the settings need inited
            //if (m_settings.ProgramStateList.Count == 0) #todo renable
            {
                // Create all of the programs with their defaults.
                m_settings.ProgramStateList.Clear();
                m_settings.ProgramStateList.Add(GlowPrograms.GlowControl, GlowControlSettings.ProgramState.Background);
                m_settings.ProgramStateList.Add(GlowPrograms.ManualColors, GlowControlSettings.ProgramState.Eligible);
                m_settings.ProgramStateList.Add(GlowPrograms.Clock, GlowControlSettings.ProgramState.Disabled);
                m_settings.ProgramStateList.Add(GlowPrograms.Weather, GlowControlSettings.ProgramState.Disabled);
                m_settings.ProgramStateList.Add(GlowPrograms.WeatherCam, GlowControlSettings.ProgramState.Disabled);
                m_settings.SaveSettings();
            }

            // Get the current cycle time
            m_nextCycleTimeMs = (int)m_settings.ProgramCycleTimeMs;
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
                        // We didn't find one, just keep the running program running.
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
            }
        }

        public Command CommandRecieved(Command command)
        {
            return null;
        }
        
        private void GoToSleepState()
        {
            // In the sleep state we want to turn off the balls...
            m_controller.GetLed(0).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 3), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(1).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 3), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(2).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 3), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(3).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 3), WindowsIotLedDriver.AnimationType.Linear);
            m_controller.GetLed(4).Animate(0.0, 0.0, 0.0, 1.0, new TimeSpan(0, 0, 3), WindowsIotLedDriver.AnimationType.Linear);

            // And slow down the work tick.
            m_controller.SetWorkRate(500);

            // Note the work tick will still fire which will cause us to keep listening for commands.
        }
    }
}
