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
    class GlowSmarts : ICommandListener, IProgramController
    {
        //
        // Private Vars
        //

        // Command logic
        CommandServer m_commandServer;
        List<Command> m_commands = new List<Command>();

        // Led Logic
        LedController m_ledController;

        // Thread logic
        Task m_workerThread;
        bool m_continueWorking = false;
        uint m_workRateMs = 500;
        DateTime m_lastWorkTime;

        // Program logic
        List<IProgram> m_programs = new List<IProgram>();
        GlowPrograms m_currentProgram = GlowPrograms.None;
        GlowPrograms m_newProgram = GlowPrograms.Clock;

        // The main entry point for glow smarts.
        public void Run()
        {
            // Create the LED controller
            m_ledController = new LedController();

            // Create the programs, note these must match the order of the enum.
            m_programs.Add(new ManualColor());
            m_programs.Add(new Clock());
            m_programs.Add(new Weather());
            m_programs.Add(new WeatherCam());

            m_programs[0].InitProgram(this);
            m_programs[1].InitProgram(this);
            m_programs[2].InitProgram(this);
            m_programs[3].InitProgram(this);

            // Create a command listener
            m_commandServer = new CommandServer(this);
            m_commandServer.Setup(22112);

            // Setup the main worker thread.
            m_continueWorking = true;
            m_lastWorkTime = DateTime.Now;
            m_workerThread = new Task(WorkLoop);
            m_workerThread.Start(); 
        }

        // Fired when a new command has arrived.
        public void OnCommand(Command command)
        {
            // Add the command to the command list. This will be take care of in the 
            // main work loop.
            lock(m_commands)
            {
                m_commands.Add(command);
            }
        }

        // The main work loop.
        private void WorkLoop()
        {
            while(m_continueWorking)
            {
                try
                {
                    DateTime begin = DateTime.Now;

                    // 1: See if there are any commands we should service
                    lock (m_commands)
                    {
                        if(m_commands.Count > 0)
                        {
                            // #todo handle commands
                        }
                    }

                    // 2: See if we need to change programs
                    if(m_currentProgram != m_newProgram)
                    {
                        // We need to change programs, first kill the old one.
                        if(m_currentProgram != GlowPrograms.None)
                        {
                            m_programs[(int)m_currentProgram].Deactivate();
                        }

                        // Now we need to activate the new one
                        m_currentProgram = m_newProgram;
                        m_programs[(int)m_currentProgram].Activate();
                    }

                    // 3: Let the program do work
                    
                    TimeSpan workTimeDiff = DateTime.Now - m_lastWorkTime;
                    m_programs[(int)m_currentProgram].DoWork((uint)workTimeDiff.TotalMilliseconds);

                    // 4: Sleep
                    int sleepTime =  (int)m_workRateMs - (int)(DateTime.Now - begin).TotalMilliseconds;
                    if(sleepTime > 0)
                    {
                        System.Threading.Tasks.Task.Delay(sleepTime).Wait();
                    }                    
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception in main work loop! message: "+e.Message);
                }
            }
        }

        // Called from the programs to get leds
        public AnimatedLed GetLed(int ledNumber)
        {
            return m_ledController.GetLed(ledNumber);
        }

        // Called from the programs to change the work callback rate
        public void SetWorkRate(uint workRateMs)
        {
            m_workRateMs = workRateMs;
        }
    }
}
