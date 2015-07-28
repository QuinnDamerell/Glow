using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects
{
    public class ManualColorSettings
    {
        /// <summary>
        /// Indicates if we should attempt to read the current settings or not.
        /// </summary>
        bool m_readSettings = false;

        public ManualColorSettings(bool readSettings = false)
        {
            m_readSettings = readSettings;
        }

        //
        // Helpers
        //

        public void SaveSettings()
        {
            // Set this explicitly so it will save the entire dictionary.
            if (m_currentLedState != null)
            {
                CurrentLedState = m_currentLedState;
            }
        }

        //
        // Settings
        //
        public List<SerlizableLed> CurrentLedState
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<List<SerlizableLed>>.GetStringSeralizedValueOrDefault(ref m_currentLedState, null, new List<SerlizableLed>(), "ManualColorSettings.CurrentLedState");
                }
                else
                {
                    return m_currentLedState;
                }               
            }
            set
            {
                m_currentLedState = value;
                SettingsHelpers<List<SerlizableLed>>.SetStringSeralizedNewValues(m_currentLedState, "ManualColorSettings.CurrentLedState");
            }
        }
        private List<SerlizableLed> m_currentLedState = null;
    }
}
