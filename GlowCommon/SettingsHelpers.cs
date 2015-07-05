using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Foundation.Collections;

namespace GlowCommon
{
    class SettingsHelpers<T>
    {
        private static IPropertySet m_roamingSettings;

        private static IPropertySet GetRomaingSettings()
        {
            if(m_roamingSettings == null)
            {
                m_roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings.Values;
            }
            return m_roamingSettings;
        }

       
        public static T GetValueOrDefault(ref T currentValue, T emptyValue, T defaultValue, string valueName)
        {
            if((currentValue == null && emptyValue == null) || currentValue.Equals(emptyValue))
            {
                // We need to try to get the setting
                if(GetRomaingSettings().ContainsKey(valueName))
                {
                    // We can get it
                    currentValue = (T)GetRomaingSettings()[valueName];
                }
                else
                {
                    // We don't have it
                    currentValue = defaultValue;
                }
            }
            return currentValue;
        }

        public static void SetNewValues(T newValue, string valueName)
        {
            GetRomaingSettings()[valueName] = newValue;
        }

        //
        // Helper to serialize objects that can't be done by WinRT 
        //
        public static T GetStringSeralizedValueOrDefault(ref T currentValue, T emptyValue, T defaultValue, string valueName)
        {
            if ((currentValue == null && emptyValue == null) || currentValue.Equals(emptyValue))
            {
                // We need to try to get the setting
                if (GetRomaingSettings().ContainsKey(valueName))
                {
                    // We can get it
                    string objString = (string)GetRomaingSettings()[valueName];
                    currentValue = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objString);
                }
                else
                {
                    // We don't have it
                    currentValue = defaultValue;
                }
            }
            return currentValue;
        }

        public static void SetStringSeralizedNewValues(T newValue, string valueName)
        {
            GetRomaingSettings()[valueName] = Newtonsoft.Json.JsonConvert.SerializeObject(newValue, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
