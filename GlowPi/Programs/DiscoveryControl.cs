using GlowPi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.DataObjects;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using GlowCommon;

namespace GlowPi.Programs
{
    class DiscoveryControl : IProgram
    {
        IProgramController m_controller;
        DiscoveryServer m_discovery;

        public void InitProgram(IProgramController controller)
        {
            m_controller = controller;
            m_discovery = new DiscoveryServer(DiscoveryServer.DiscoveryMode.Broadcast);
        }

        public void Activate()
        {
            // Ignore
        }

        public Command CommandRecieved(Command command)
        {
            // Ignore
            return null;
        }

        public void Deactivate()
        {
            // Ignore
        }

        public void DoWork(uint timeElaspedMs)
        {
            // Forward the call along.
            m_discovery.DoWork(timeElaspedMs);
        }
    }
}
