using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects
{
    public class AppSettings
    {
        //
        // Settings
        //
        public bool ShowShortIntro
        {
            get
            {
                return SettingsHelpers<bool>.GetValueOrDefault(ref m_showShortIntro, true, true, "AppSettings.ShowShortIntro");
            }
            set
            {
                m_showShortIntro = value;
                SettingsHelpers<bool>.SetNewValues(m_showShortIntro, "AppSettings.ShowShortIntro");
            }
        }
        private bool m_showShortIntro = true;

        public GlowPrograms LastShownProgram
        {
            get
            {
                return (GlowPrograms)SettingsHelpers<int>.GetStringSeralizedValueOrDefault(ref m_lastShownProgram, (int)GlowPrograms.None, (int)GlowPrograms.GlowControl, "AppSettings.LastShownProgramInt");
            }
            set
            {
                m_lastShownProgram = (int)value;
                SettingsHelpers<int>.SetStringSeralizedNewValues(m_lastShownProgram, "AppSettings.LastShownProgramInt");
            }
        }
        private int m_lastShownProgram = (int)GlowPrograms.None;
    }
}
