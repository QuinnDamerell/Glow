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
            if (m_currentLedStateList != null)
            {
                CurrentLedStateList = m_currentLedStateList;
            }
        }

        //
        // Settings
        //
        public List<List<SerlizableLed>> CurrentLedStateList
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<List<List<SerlizableLed>>>.GetStringSeralizedValueOrDefault(ref m_currentLedStateList, null, new List<List<SerlizableLed>>(), "ManualColorSettings.CurrentLedStateList");
                }
                else
                {
                    return m_currentLedStateList;
                }
            }
            set
            {
                m_currentLedStateList = value;
                SettingsHelpers<List<List<SerlizableLed>>>.SetStringSeralizedNewValues(m_currentLedStateList, "ManualColorSettings.CurrentLedStateList");
            }
        }
        private List<List<SerlizableLed>> m_currentLedStateList = null;

        /// <summary>
        /// The amount of time between cycles in the color list
        /// </summary>
        public double ColorListCycleTimeSeconds
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<double>.GetStringSeralizedValueOrDefault(ref m_colorListCycleTimeSeconds, -1, 10, "ManualColorSettings.ColorListCycleTimeSeconds");
                }
                else
                {
                    return m_colorListCycleTimeSeconds;
                }
            }
            set
            {
                m_colorListCycleTimeSeconds = value;
                SettingsHelpers<double>.SetStringSeralizedNewValues(m_colorListCycleTimeSeconds, "ManualColorSettings.ColorListCycleTimeSeconds");
            }
        }
        private double m_colorListCycleTimeSeconds = -1;
    }
}
