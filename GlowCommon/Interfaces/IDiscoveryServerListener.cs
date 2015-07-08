using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowCommon.Interfaces
{
    public interface IDiscoveryServerListener
    {
        void OnClientFound(string ipAddres);
    }
}
