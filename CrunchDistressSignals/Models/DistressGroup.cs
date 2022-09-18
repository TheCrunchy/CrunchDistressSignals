using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchDistressSignals.Models
{
    public class DistressGroup
    {
        public bool Enabled = false;
        public string Name { get; set; }
        public List<string> Aliases = new List<string>();
        public List<ulong> SteamIdsToSendTo { get; set; } = new List<ulong>();
        public long DiscordChannelIdToSendTo = 0;
        public bool SendToDiscord = true;
    }
}
