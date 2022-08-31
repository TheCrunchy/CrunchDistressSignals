using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchDistressSignals.Helpers
{
    public static class CooldownHelpers
    {
        public static string GetCooldownMessage(DateTime time)
        {
            var diff = time.Subtract(DateTime.Now);
            return $"{diff.Seconds} Seconds until command can be used.";
        }
    }
}
