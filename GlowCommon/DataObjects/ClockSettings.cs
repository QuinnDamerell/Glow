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
            Color = 0,
            GrayScale,            
            None,
        }

        bool m_readSettings = false;

        public ClockSettings(bool readSeattings = false)
        {
            m_readSettings = readSeattings;
        }

        //
        // Settings
        //
        public ColorTypes ColorType
        {
            get
            {
                if (m_readSettings)
                {
                    return (ColorTypes)SettingsHelpers<int>.GetValueOrDefault(ref m_colorType, (int)ColorTypes.None, (int)ColorTypes.Color, "Clock.ColorType");
                }
                else
                {
                    return (ColorTypes)m_colorType;
                }
            }
            set
            {
                m_colorType = (int)value;
                SettingsHelpers<int>.SetNewValues(m_colorType, "Clock.ColorType");
            }
        }
        private int m_colorType = (int)ColorTypes.None;
        
        //
        // Constructor
        //        
    }
}
