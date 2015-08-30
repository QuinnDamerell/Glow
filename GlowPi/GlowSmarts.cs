using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon;
using GlowCommon.Interfaces;
using GlowCommon.DataObjects;
using GlowPi.Interfaces;
using GlowPi.Programs;
using WindowsIotLedDriver;

namespace GlowPi
{
    class GlowSmarts : ICommandServerListener, IProgramController
    {
        //
        // Private Vars
        //

        // Command logic
        CommandServer m_commandServer;

        // Led Logic
        LedController m_ledController;

        // Thread logic
        Task m_workerThread;
        bool m_continueWorking = false;
        uint m_workRateMs = 500;
        DateTime m_lastWorkTime;
        bool m_stopWorkAndLoop = false;

        // Program logic

        // This dictionary is used to hold all of the programs we know of
        Dictionary<GlowPrograms, IProgram> m_programCache = new Dictionary<GlowPrograms, IProgram>();
        // This dictionary is used to hold all of the active programs.
        Dictionary<GlowPrograms, IProgram> m_activePrograms = new Dictionary<GlowPrograms, IProgram>();
        // This list holds all modifications to the program list
        List<KeyValuePair<GlowPrograms, bool>> m_programModifications = new List<KeyValuePair<GlowPrograms, bool>>();

        // The main entry point for glow smarts.
        public void Run()
        {
            // Create the LED controller
            m_ledController = new LedController();

            // Create all of the programs and add them to the cache.
            IProgram program = new GlowControl();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.GlowControl, program);

            program = new DiscoveryControl();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.DiscoveryControl, program);

            program = new ManualColor();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.ManualColors, program);

            program = new Clock();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.Clock, program);

            program = new Weather();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.Weather, program);

            program = new WeatherCam();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.WeatherCam, program);

            program = new RandomColor();
            program.InitProgram(this);
            m_programCache.Add(GlowPrograms.RandomColor, program);

            // Create a command listener
            m_commandServer = new CommandServer(this, CommandServer.CommmandServerMode.Server);

            // Setup the main worker thread.
            m_continueWorking = true;
            m_lastWorkTime = DateTime.Now;
            m_workerThread = new Task(WorkLoop);
            m_workerThread.Start(); 
        }

        // Fired when a new command has arrived.
        // If a command is returned it will be sont back.
        public Command OnCommand(Command command)
        {
            // Make sure we have the program
            if(!m_programCache.ContainsKey(command.Program))
            {
                return null;
            }

            // Check the version is current
            if(command.Version != Command.COMMAND_VERSION)
            {
                System.Diagnostics.Debug.WriteLine("A old message was thrown away.");
                return null;
            }

            // Get the program
            IProgram program = m_programCache[command.Program];

            // Send the commands
            return program.CommandRecieved(command);
        }

        // Called from the programs to get leds
        public AnimatedLed GetLed(int ledNumber)
        {
            return m_ledController.GetLed(ledNumber);
        }

        public double GetMasterIntensity()
        {
            return m_ledController.GetMasterIntensity();
        }

        public void AnimateMasterIntensity(double intensity, TimeSpan animationTime)
        {
            m_ledController.AnimateMasterIntensity(intensity, animationTime);
        }

        public bool IsMasterIntensityAnimating()
        {
            return m_ledController.IsMasterIntensityAnimating();
        }

        // Called from the programs to change the work callback rate
        public void SetWorkRate(uint workRateMs)
        {
            // Make sure we at least cycle once a second.
            workRateMs = Math.Min(1000, workRateMs);
            m_workRateMs = workRateMs;
        }

        // Called from the glow control when a program should be added or removed.
        public void ToggleProgram(GlowPrograms program, bool enable)
        {
            lock(m_programModifications)
            {
                // Add it to the list of pending changes, this will be updated on the next tick.
                m_programModifications.Add(new KeyValuePair<GlowPrograms, bool>(program, enable));
            }

            // Prevent programs from doing work if we need to change them.
            m_stopWorkAndLoop = true;
        }

        public bool IsProgramEnabled(GlowPrograms program)
        {
            return m_activePrograms.ContainsKey(program);
        }

        // The main work loop.
        private void WorkLoop()
        {
            while(m_continueWorking)
            {
                try
                {
                    DateTime begin = DateTime.Now;

                    // 1: See if we need to change programs
                    if (m_programModifications.Count > 0)
                    {
                        // We need to change up some programs.
                        HandelProgramChanges();
                    }       

                    // 2: Let all of the programs do work.
                    lock(m_activePrograms)
                    {
                        foreach(KeyValuePair<GlowPrograms, IProgram> program in m_activePrograms)
                        {
                            TimeSpan workTimeDiff = DateTime.Now - m_lastWorkTime;
                            program.Value.DoWork((uint)workTimeDiff.TotalMilliseconds);
                            if(m_stopWorkAndLoop)
                            {
                                break;
                            }
                        }
                    }

                    // Update the last work time
                    m_lastWorkTime = DateTime.Now;

                    // 3: Sleep
                    int sleepTime =  (int)m_workRateMs - (int)(DateTime.Now - begin).TotalMilliseconds;
                    if(sleepTime > 0 && !m_stopWorkAndLoop)
                    {
                        System.Threading.Tasks.Task.Delay(sleepTime).Wait();
                    }
                    m_stopWorkAndLoop = false;
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in main work loop! message: "+e.Message);
                }
            }
        }

        private void HandelProgramChanges()
        {
            // First lock the list
            lock (m_programModifications)
            {
                // Loop through all of the changes.
                foreach(KeyValuePair<GlowPrograms, bool> change in m_programModifications)
                {
                    if(change.Value)
                    {
                        // We are adding a program= make sure it isn't already active.
                        lock(m_activePrograms)
                        {
                            if(m_activePrograms.ContainsKey(change.Key))
                            {
                                // The program is already active, continue.
                                break;
                            }
                        }

                        // Now activate it
                        IProgram addProgram = m_programCache[change.Key];
                        addProgram.Activate();

                        // And add it to the list.
                        lock(m_activePrograms)
                        {
                            m_activePrograms.Add(change.Key, addProgram);
                        }
                    }
                    else
                    {
                        // We are removing a program, make sure it exists
                        lock (m_activePrograms)
                        {
                            if (!m_activePrograms.ContainsKey(change.Key))
                            {
                                // The program already doesn't exist
                                break;
                            }
                        }

                        // Deactivate it
                        IProgram addProgram = m_programCache[change.Key];
                        addProgram.Deactivate();

                        // Remove it
                        lock (m_activePrograms)
                        {
                            m_activePrograms.Remove(change.Key);
                        }
                    }
                }

                // Empty the list
                m_programModifications.Clear();
            }
        }

        public void OnConnect()
        {
            // ignore for now
        }

        public void OnDisconnected()
        {
            // ignore for now
        }

        public void OnFatalError()
        {
            // ignore for now
        }
    }
}
