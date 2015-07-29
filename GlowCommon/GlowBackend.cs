using GlowCommon.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon
{
    /// <summary>
    /// A common LED class that can be serialized. Use in the settings classes
    /// </summary>
    public class SerlizableLed
    {
        public double Red = 0.0;
        public double Green = 0.0;
        public double Blue = 0.0;
        public double Intensity = 0.0;
        public int ChangeSpeedMs = 0;

        public SerlizableLed(double red, double green, double blue, double intensity, int chagneTimeMs = 1000)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Intensity = intensity;
            ChangeSpeedMs = chagneTimeMs;
        }
    }

    /// <summary>
    /// A list of ever possible program for Glow
    /// </summary>
    public enum GlowPrograms
    {
        GlowControl = 0,
        DiscoveryControl,
        ManualColors,
        Clock,
        Weather,
        WeatherCam,
        RandomColor,
        None
    }

    public class GlowBackend
    {
        //
        // Public classes
        //
        public AppSettings AppSetting;
        public AppConnectionManager ConnectionManager;

        //
        // Private vars
        //
        bool m_isApp;

        public GlowBackend(bool isApp)
        {
            m_isApp = isApp;

            if(m_isApp)
            {
                // Only create these classes if we are an app
                AppSetting = new AppSettings();
                ConnectionManager = new AppConnectionManager();
            }
        }
    }
}
