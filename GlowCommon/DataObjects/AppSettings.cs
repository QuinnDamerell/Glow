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
    }
}
