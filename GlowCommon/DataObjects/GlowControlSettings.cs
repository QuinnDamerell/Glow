using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon;

namespace GlowCommon.DataObjects
{
    public class GlowControlSettings
    {
        /// <summary>
        /// Defines the state of any given program.
        ///     None - Not set
        ///     Background - A program that runs in the background not effecting the glows
        ///     Eligible - A program that is eligible to be ran
        ///     Disabled - A program that is eligible to be ran.
        /// </summary>
        public enum ProgramState
        {
            None = 0,
            Background,
            Eligible,
            Disabled,
        }

        /// <summary>
        /// Indicates if we should read settings from the store.
        /// </summary>
        public bool ReadSettings = false;

        public GlowControlSettings(bool readSettings = false)
        {
            ReadSettings = readSettings;
        }

        //
        // Helpers
        //
        public void SaveSettings()
        {
            // Set this explicitly so it will save the entire dictionary.
            if (m_programStateList != null)
            {
                ProgramStateList = m_programStateList;
            }          
        }

        //
        // Settings
        //
        public Dictionary<GlowPrograms, ProgramState> ProgramStateList 
        {
            get
            {
                if (ReadSettings)
                {
                    return SettingsHelpers<Dictionary<GlowPrograms, ProgramState>>.GetStringSeralizedValueOrDefault(ref m_programStateList, null, new Dictionary<GlowPrograms, ProgramState>(), "GlowControl.ProgramStateList");
                }
                else
                {
                    return m_programStateList;
                }
            }
            set
            {
                m_programStateList = value;
                SettingsHelpers<Dictionary<GlowPrograms, ProgramState>>.SetStringSeralizedNewValues(m_programStateList, "GlowControl.ProgramStateList");
            }
        }
        private Dictionary<GlowPrograms, ProgramState> m_programStateList = null;

        /// <summary>
        /// The amount of time we will spend on each program when cycling.
        /// </summary>
        public uint ProgramCycleTimeMs
        {
            get
            {
                if (ReadSettings)
                {
                    // Default to 30 seconds
                    return SettingsHelpers<uint>.GetValueOrDefault(ref m_programCycleTimeMs, 0, 30 * 1000, "GlowControl.ProgramCycleTimeMs");
                }
                else
                {
                    return m_programCycleTimeMs;
                }
            }
            set
            {
                m_programCycleTimeMs = value;
                SettingsHelpers<uint>.SetNewValues(m_programCycleTimeMs, "GlowControl.ProgramCycleTimeMs");
            }
        }
        private uint m_programCycleTimeMs = 0;


        /// <summary>
        /// The over all intensity.
        /// </summary>
        public double MasterIntensity
        {
            get
            {
                if (ReadSettings)
                {
                    return SettingsHelpers<double>.GetValueOrDefault(ref m_masterIntensity, -1, 1, "GlowControl.MasterIntensity");
                }
                else
                {
                    return m_masterIntensity;
                }
            }
            set
            {
                m_masterIntensity = value;
                SettingsHelpers<double>.SetNewValues(m_programCycleTimeMs, "GlowControl.MasterIntensity");
            }
        }
        private double m_masterIntensity = -1;

        /// <summary>
        /// Intensity while in sleepy time
        /// </summary>
        public double SleepyTimeIntensity
        {
            get
            {
                if (ReadSettings)
                {
                    return SettingsHelpers<double>.GetValueOrDefault(ref m_sleepyTimeIntensity, -1, 1, "GlowControl.SleepyTimeIntensity");
                }
                else
                {
                    return m_sleepyTimeIntensity;
                }
            }
            set
            {
                m_sleepyTimeIntensity = value;
                SettingsHelpers<double>.SetNewValues(m_sleepyTimeIntensity, "GlowControl.SleepyTimeIntensity");
            }
        }
        private double m_sleepyTimeIntensity = -1;

        /// <summary>
        /// Sleepy time start
        /// </summary>
        public TimeSpan SleepyTimeStart
        {
            get
            {
                if (ReadSettings)
                {
                    return SettingsHelpers<TimeSpan>.GetStringSeralizedValueOrDefault(ref m_sleepyTimeStart, new TimeSpan(12, 0, 0), new TimeSpan(12, 0, 0), "GlowControl.SleepyTimeStart");
                }
                else
                {
                    return m_sleepyTimeStart;
                }
            }
            set
            {
                m_sleepyTimeStart = value;
                SettingsHelpers<TimeSpan>.SetStringSeralizedNewValues(m_sleepyTimeStart, "GlowControl.SleepyTimeStart");
            }
        }
        private TimeSpan m_sleepyTimeStart = new TimeSpan(12, 0, 0);

        /// <summary>
        /// Sleepy time end
        /// </summary>
        public TimeSpan SleepyTimeEnd
        {
            get
            {
                if (ReadSettings)
                {
                    return SettingsHelpers<TimeSpan>.GetStringSeralizedValueOrDefault(ref m_sleepyTimeEnd, new TimeSpan(12, 0, 0), new TimeSpan(12, 0, 0), "GlowControl.SleepyTimeEnd");
                }
                else
                {
                    return m_sleepyTimeEnd;
                }
            }
            set
            {
                m_sleepyTimeEnd = value;
                SettingsHelpers<TimeSpan>.SetStringSeralizedNewValues(m_sleepyTimeEnd, "GlowControl.SleepyTimeEnd");
            }
        }
        private TimeSpan m_sleepyTimeEnd = new TimeSpan(12, 0, 0);
    }
}
