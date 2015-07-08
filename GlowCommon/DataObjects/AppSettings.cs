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
        public string DeviceIp
        {
            get
            {
                return SettingsHelpers<string>.GetValueOrDefault(ref m_deviceIp, "", "", "AppSettings.DeviceIp");
            }
            set
            {
                m_deviceIp = value;
                SettingsHelpers<string>.SetNewValues(m_deviceIp, "AppSettings.DeviceIp");
            }
        }
        private string m_deviceIp = "";
    }
}
