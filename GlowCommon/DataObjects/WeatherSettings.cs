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

        public class WeatherCondition
        {
            public enum Condition
            {
                Sunny,
                Cloudy,
                Rain,
                Storm
            }

            public Condition CurrentCondition;
            public int High;
            public int Low;
            public int Humidity;
            public int WindSpeed;
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
        
        /// <summary>
        /// The amount of time we will spend on each program when cycling.
        /// </summary>
        public WeatherCondition CurrentCondition
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<WeatherCondition>.GetStringSeralizedValueOrDefault(ref m_currentCondition, null, null, "Weather.CurrentCondition");
                }
                else
                {
                    return m_currentCondition;
                }
            }
            set
            {
                m_currentCondition = value;
                SettingsHelpers<WeatherCondition>.SetStringSeralizedNewValues(m_currentCondition, "Weather.CurrentCondition");
            }
        }
        private WeatherCondition m_currentCondition = null;


        /// <summary>
        /// The amount of time we will spend on each program when cycling.
        /// </summary>
        public DateTime LastUpdateTime
        {
            get
            {
                if (m_readSettings)
                {
                    return SettingsHelpers<DateTime>.GetStringSeralizedValueOrDefault(ref m_lastUpdateTime, new DateTime(0), new DateTime(0), "Weather.LastUpdateTime");
                }
                else
                {
                    return m_lastUpdateTime;
                }
            }
            set
            {
                m_lastUpdateTime = value;
                SettingsHelpers<DateTime>.SetStringSeralizedNewValues(m_lastUpdateTime, "Weather.LastUpdateTime");
            }
        }
        private DateTime m_lastUpdateTime = new DateTime(0);
    }
}
