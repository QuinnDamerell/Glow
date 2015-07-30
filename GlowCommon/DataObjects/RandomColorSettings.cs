using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects
{
    public class RandomColorSettings
    {
        /// <summary>
        /// Indicates if we should read settings from the store.
        /// </summary>
        private bool m_readSettings = false;

        public RandomColorSettings(bool readSettings = false)
        {
            m_readSettings = readSettings;
        }

        /// <summary>
        /// The max time for a color change
        /// </summary>
        public int TransistionMaxMs
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<int>.GetValueOrDefault(ref m_transistionMaxMs, -1, 3 * 1000, "RandomColor.TransistionMax");
                }
                else
                {
                    return m_transistionMaxMs;
                }
            }
            set
            {
                m_transistionMaxMs = value;
                SettingsHelpers<int>.SetNewValues(m_transistionMaxMs, "RandomColor.TransistionMax");
            }
        }
        private int m_transistionMaxMs = -1;

        /// <summary>
        /// The min time for a color change
        /// </summary>
        public int TransistionMinMs
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<int>.GetValueOrDefault(ref m_transistionMinMs, -1, 300, "RandomColor.TransistionMinMs");
                }
                else
                {
                    return m_transistionMinMs;
                }
            }
            set
            {
                m_transistionMinMs = value;
                SettingsHelpers<int>.SetNewValues(m_transistionMinMs, "RandomColor.TransistionMinMs");
            }
        }
        private int m_transistionMinMs = -1;

        /// <summary>
        /// The max time between new color changes.
        /// </summary>
        public int NextUpdateTimeMaxMs
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<int>.GetValueOrDefault(ref m_nextUpdateTimeMaxMs, -1, 300, "RandomColor.NextUpdateTimeMaxMs");
                }
                else
                {
                    return m_nextUpdateTimeMaxMs;
                }
            }
            set
            {
                m_nextUpdateTimeMaxMs = value;
                SettingsHelpers<int>.SetNewValues(m_nextUpdateTimeMaxMs, "RandomColor.NextUpdateTimeMaxMs");
            }
        }
        private int m_nextUpdateTimeMaxMs = -1;

        /// <summary>
        /// The min time between new color changes.
        /// </summary>
        public int NextUpdateTimeMinMs
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<int>.GetValueOrDefault(ref m_nextUpdateTimeMinMs, -1, 300, "RandomColor.NextUpdateTimeMinMs");
                }
                else
                {
                    return m_nextUpdateTimeMinMs;
                }
            }
            set
            {
                m_nextUpdateTimeMinMs = value;
                SettingsHelpers<int>.SetNewValues(m_nextUpdateTimeMinMs, "RandomColor.NextUpdateTimeMinMs");
            }
        }
        private int m_nextUpdateTimeMinMs = -1;
    }
}
