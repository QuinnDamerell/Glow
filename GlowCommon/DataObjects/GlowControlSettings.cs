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

        //
        // Helpers
        //
        public void SaveSettings()
        {
            // Set this explicitly so it will save the entire dictionary.
            ProgramStateList = m_programStateList;
        }

        //
        // Settings
        //
        public Dictionary<GlowPrograms, ProgramState> ProgramStateList 
        {
            get
            {
                return SettingsHelpers<Dictionary<GlowPrograms, ProgramState>>.GetStringSeralizedValueOrDefault(ref m_programStateList, null, new Dictionary<GlowPrograms, ProgramState>(), "GlowControl.ProgramStateList");
            }
            set
            {
                m_programStateList = value;
                // #todo fix
                //SettingsHelpers<Dictionary<GlowPrograms, ProgramState>>.SetStringSeralizedNewValues(m_programStateList, "GlowControl.ProgramStateList");
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
                // Default to 30 seconds
                return SettingsHelpers<uint>.GetValueOrDefault(ref m_programCycleTimeMs, 0, 30 * 1000, "GlowControl.ProgramCycleTimeMs");
            }
            set
            {
                m_programCycleTimeMs = value;
                SettingsHelpers<uint>.SetNewValues(m_programCycleTimeMs, "GlowControl.ProgramCycleTimeMs");
            }
        }
        private uint m_programCycleTimeMs = 0;

    }
}
