using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace CrunchDistressSignals.Models
{
    public class DistressGroup
    {
        public bool Enabled = false;
        public string Prefix = "Put the sender name here";
        public string Name { get; set; }
        public List<string> Aliases = new List<string>();
        public List<ulong> SteamIdsToSendTo { get; set; } = new List<ulong>();
        public ulong DiscordChannelIdToSendTo = 0;
        public bool SendToDiscord = true;
        public string BotToken = "put bot token here";
        public int r = 55;
        public int g = 55;
        public int b = 55;
        public Color Color => new Color(r, g, b);
      
    }
}
