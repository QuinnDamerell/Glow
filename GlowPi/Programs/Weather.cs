using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using GlowPi.Interfaces;
using Windows.Devices.Geolocation;
using System.Diagnostics;
using System.Net;
using Windows.Web.Http;
using Newtonsoft.Json;
using WindowsIotLedDriver;

namespace GlowPi.Programs
{
    class Weather : IProgram
    {
        const string c_weahterUrl = "http://api.wunderground.com/api/c11ef011a1df72b4/forecast/q/";
        WeatherSettings m_settings = new WeatherSettings(true);
        IProgramController m_controller;
        bool m_reqeusting = false;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;
        }

        public void Activate()
        {
            m_controller.SetWorkRate(500);
        }

        public void Deactivate()
        {          
        }

        public void DoWork(uint timeElaspedMs)
        {
            // Update if we need to
            TimeSpan diff = DateTime.Now - m_settings.LastUpdateTime;
            if(diff.TotalMinutes > 20)
            {
                UpdateWeather();
            }

            // Set the lights
            if(m_settings.CurrentCondition != null)
            {
                // First the condition
                switch(m_settings.CurrentCondition.CurrentCondition)
                {
                    case WeatherSettings.WeatherCondition.Condition.Cloudy:
                        // White
                        m_controller.GetLed(0).Animate(1, 1, 1, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                        break;
                    case WeatherSettings.WeatherCondition.Condition.Sunny:
                        // Yellow
                        m_controller.GetLed(0).Animate(1, 1, 0, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);

                        break;
                    case WeatherSettings.WeatherCondition.Condition.Rain:
                        // Blue
                        m_controller.GetLed(0).Animate(0, 0, 1, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                        break;
                    case WeatherSettings.WeatherCondition.Condition.Storm:
                        // Purple
                        m_controller.GetLed(0).Animate(1, 0, 1, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                        break;
                }

                // Set the rest.
                double high = Math.Max(Math.Min(100, m_settings.CurrentCondition.High), 0) / 100.0;
                double low = Math.Max(Math.Min(100, m_settings.CurrentCondition.Low), 0) / 100.0;
                double wind = Math.Max(Math.Min(20, m_settings.CurrentCondition.WindSpeed), 0) / 20.0;
                double humd = Math.Max(Math.Min(100, m_settings.CurrentCondition.Humidity), 0) / 100.0;

                SetTempValue(m_controller.GetLed(1), high);
                SetTempValue(m_controller.GetLed(2), low);
                SetWindValue(m_controller.GetLed(3), wind);
                SetWindValue(m_controller.GetLed(4), humd);
            }
            else
            {
                m_controller.GetLed(0).Animate(1, 0, 0, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(1).Animate(0, 1, 0, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(2).Animate(0, 0, 1, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(3).Animate(1, 0, 1, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
                m_controller.GetLed(4).Animate(0, 1, 1, 1, TimeSpan.FromSeconds(1), WindowsIotLedDriver.AnimationType.Linear);
            }
        }

        public Command CommandRecieved(Command command)
        {
            // Switch on message type
            switch (command.MessageId)
            {
                case (uint)Command.COMMAND_RECIEVE_SETTINGS:
                    UpdateSettings(command);
                    break;
            }
            return null;
        }

        // Updates the settings given a settings command.
        private void UpdateSettings(Command command)
        {
            // Get the settings from the message
            WeatherSettings newSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherSettings>(command.Message);
            if (newSettings == null)
            {
                return;
            }

            // Update the location if needed
            if(m_settings.CurrentLocation.State != newSettings.CurrentLocation.State ||
                m_settings.CurrentLocation.City != newSettings.CurrentLocation.City)
            {
                m_settings.CurrentLocation = newSettings.CurrentLocation;
                UpdateWeather();
            }
        }

        private async void UpdateWeather()
        {
            if(m_settings.CurrentLocation == null)
            {
                return;
            }

            if(m_reqeusting)
            {
                return;
            }
            m_reqeusting = true;

            try
            {
                // Make the web request
                HttpClient client = new HttpClient();
                Uri address = new Uri(c_weahterUrl + m_settings.CurrentLocation.State +"/"+ m_settings.CurrentLocation.City + ".json", UriKind.Absolute);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, address);
                var result = await client.SendRequestAsync(request);
                string content = await result.Content.ReadAsStringAsync();

                // Parse the Json
                WeatherJsonRoot weather = JsonConvert.DeserializeObject<WeatherJsonRoot>(content);

                // Convert the condition
                WeatherSettings.WeatherCondition.Condition condition;
                switch(weather.forecast.simpleforecast.forecastday[0].icon)
                {
                    case "chanceflurries":
                    case "chancerain":
                    case "chancesleet":
                    case "chancesnow":
                    case "flurries":
                    case "rain":
                    case "snow":
                    case "sleet":
                        {
                            condition = WeatherSettings.WeatherCondition.Condition.Rain;
                            break;
                        }
                    case "chancetstorms":
                    case "tstorms":
                    case "unknown":
                        {
                            condition = WeatherSettings.WeatherCondition.Condition.Storm;
                            break;
                        }
                    case "clear":
                    case "fog":
                    case "hazy":
                    case "mostlysunny":
                    case "partlycloudy":
                    case "partlysunny":
                    case "sunny":
                        {
                            condition = WeatherSettings.WeatherCondition.Condition.Sunny;
                            break;
                        }
                    case "cloudy":
                    case "mostlycloudy":
                        {
                            condition = WeatherSettings.WeatherCondition.Condition.Cloudy;
                            break;
                        }
                    default:
                        {
                            condition = WeatherSettings.WeatherCondition.Condition.Rain;
                            break;
                        }
                }

                // Make the object
                WeatherSettings.WeatherCondition weahterCondition = new WeatherSettings.WeatherCondition
                {
                    High = weather.forecast.simpleforecast.forecastday[0].high.fahrenheit,
                    Low = weather.forecast.simpleforecast.forecastday[0].low.fahrenheit,
                    Humidity = weather.forecast.simpleforecast.forecastday[0].avehumidity,
                    WindSpeed = weather.forecast.simpleforecast.forecastday[0].avewind.mph,
                    CurrentCondition = condition
                };

                // Set it
                m_settings.CurrentCondition = weahterCondition;
                m_settings.LastUpdateTime = DateTime.Now;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("weather request failed "+e.Message);
            }

            m_reqeusting = false;
        }

        private void SetTempValue(AnimatedLed led, double temp)
        {
            // Fade between blue and red.
            double blue = 1 - temp;
            double red = temp;
            led.Animate(red, 0, blue, 1, TimeSpan.FromSeconds(1), AnimationType.Linear);
        }

        private void SetWindValue(AnimatedLed led, double wind)
        {
            // Fade from blue to white.
            led.Animate(wind, wind, 1.0, 1, TimeSpan.FromSeconds(1), AnimationType.Linear);
        }

#pragma warning disable CS0649
        class Temp
        {
            public int fahrenheit;          
        }

        class Wind
        {
            public int mph;
        }

        class ForcastDay
        {
            public string icon;
            public Temp high;
            public Temp low;
            public int avehumidity;
            public Wind avewind;
        }

        class SimpleForcast
        {
            public List<ForcastDay> forecastday;
        }

        class Forcast
        {
            public SimpleForcast simpleforecast;
        }

        class WeatherJsonRoot
        {
            public Forcast forecast;
        }
#pragma warning restore
    }
}
