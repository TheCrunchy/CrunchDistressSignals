using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchDistressSignals
{
    public class Config
    {
        public string StoragePath = "Default";
        public int SecondsCooldown = 300;
        public long PricePerUse = 300;
        public bool AllowGlobalSignals = true;

    }
}
