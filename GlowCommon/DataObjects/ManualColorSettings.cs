using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects
{
    public class ManualColorSettings
    {
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
                //return SettingsHelpers< List<SerlizableLed>>.GetStringSeralizedValueOrDefault(ref m_currentLedState, null, new List<SerlizableLed>(), "ManualColorSettings.CurrentLedState");
                if(m_currentLedState == null)
                {
                    m_currentLedState = new List<SerlizableLed>();
                }
                return m_currentLedState;
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
