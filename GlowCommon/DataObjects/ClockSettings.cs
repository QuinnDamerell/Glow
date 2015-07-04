using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects
{
    public class ClockSettings
    {
        public enum ColorTypes
        {
            None,
            GrayScale,
            Color
        }

        //
        // Settings
        //
        public ColorTypes ColorType
        {
            get
            {
                return SettingsHelpers<ColorTypes>.GetValueOrDefault(ref m_colorType, ColorTypes.None, ColorTypes.GrayScale, "Clock.ColorType");
            }
            set
            {
                m_colorType = value;
                SettingsHelpers<ColorTypes>.SetNewValues(m_colorType, "Clock.ColorType");
            }
        }
        private ColorTypes m_colorType = ColorTypes.None;
        
        //
        // Constructor
        //        
    }
}
