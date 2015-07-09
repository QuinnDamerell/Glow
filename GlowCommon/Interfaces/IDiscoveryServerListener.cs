using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.Interfaces
{
    public interface IDiscoveryServerListener
    {
        /// <summary>
        /// Fired when new server has been found.
        /// </summary>
        /// <param name="ipAddres"></param>
        void OnClientFound(string ipAddres);
    }
}
