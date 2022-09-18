using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace CrunchDistressSignals.Models
{
    public class DistressSignal
    {
        public string PlayerName;
        public Vector3D GPS;
        public bool SendToGlobal = false;
        public List<long> FactionsToSendTo = new List<long>();
        public List<ulong> SteamIds = new List<ulong>();
        public Color Color;
        public string Reason;
        public string Name;
    }
}
