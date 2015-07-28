using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.DataObjects
{
    public class WeatherSettings
    {
        public class Location
        {
            public string City;
            public string State;
        }

        public class CurrentCondition
        {

        }

        /// <summary>
        /// Indicates if we should attempt to read the current settings or not.
        /// </summary>
        bool m_readSettings = false;

        public WeatherSettings(bool readSettings = false)
        {
            m_readSettings = readSettings;
        }

        /// <summary>
        /// The amount of time we will spend on each program when cycling.
        /// </summary>
        public Location CurrentLocation
        {
            get
            {
                if (m_readSettings)
                {
                    // Default to 30 seconds
                    return SettingsHelpers<Location>.GetStringSeralizedValueOrDefault(ref m_currentLocation, null, null, "Weather.CurrentLocation");
                }
                else
                {
                    return m_currentLocation;
                }
            }
            set
            {
                m_currentLocation = value;
                SettingsHelpers<Location>.SetStringSeralizedNewValues(m_currentLocation, "Weather.CurrentLocation");
            }
        }
        private Location m_currentLocation = null;
    }
}
