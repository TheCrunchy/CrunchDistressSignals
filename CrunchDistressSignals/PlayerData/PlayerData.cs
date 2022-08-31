using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchDistressSignals.PlayerData
{
    public class PlayerData
    {
        public bool SendToAlliance = true;
        public bool SendToGlobal = true;
        public List<long> FactionsToSendTo = new List<long>();
        public DateTime NextUse;
    }
}
