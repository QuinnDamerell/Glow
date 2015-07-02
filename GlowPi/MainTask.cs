using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using GlowCommon;
using GlowCommon.Interfaces;
using GlowCommon.DataObjects;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace GlowPi
{
    public sealed class MainTask : IBackgroundTask
    {
        //
        // Private Vars
        //
        BackgroundTaskDeferral m_deferral;
        GlowSmarts m_glowSmarts;

        // The main entry point for IOT Apps
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // If we are running from the PiRunner this will be null.
            if(taskInstance != null)
            {
                // Grab the background deferral so we can run forever.
                m_deferral = taskInstance.GetDeferral();
            }           

            // Make the smarts and run away.
            m_glowSmarts = new GlowSmarts();
            m_glowSmarts.Run();
        }
    }
}
