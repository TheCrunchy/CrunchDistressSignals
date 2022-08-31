using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrunchDistressSignals.Helpers;
using CrunchDistressSignals.Models;
using Sandbox.Game.World;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using VRageMath;

namespace CrunchDistressSignals
{
    public class Commands : CommandModule
    {
        public static Dictionary<long, DateTime> distressCooldowns = new Dictionary<long, DateTime>();
        public static int distressCount = 0;
        [Command("distress", "distress signals")]
        [Permission(MyPromoteLevel.None)]
        public void distress(string reason = "")
        {
            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
            if (playerFac == null)
            {
                Context.Respond("You dont have a faction.");
                return;
            }
            if (reason != "")
            {
                reason = Context.RawArgs;
            }

            if (distressCooldowns.TryGetValue(Context.Player.IdentityId, out DateTime time))
            {
                if (DateTime.Now < time)
                {
                    Context.Respond(CooldownHelpers.GetCooldownMessage(time));
                    return;
                }

                distressCooldowns[Context.Player.IdentityId] = DateTime.Now.AddSeconds(30);
            }
            else
            {
                distressCooldowns.Add(Context.Player.IdentityId, DateTime.Now.AddSeconds(30));

            }

            if (Core.AlliancePluginInstalled)
            {
                var methodInput = new object[] { playerFac.Tag };
                var members = (List<long>) Core.GetAllianceMembers?.Invoke(null, methodInput);

                var distress = new DistressSignal
                {
                    GPS = Context.Player.Character.PositionComp.GetPosition(),
                    PlayerName = Context.Player.DisplayName,
                    Color = Color.Yellow,
                    FactionsToSendTo = members,
                    SendToGlobal = false,
                    Reason = reason
                };
                Context.Respond("Sending signal");
                Core.SendToMQ(MQPatching.MQPluginPatch.DistressSignals, distress);
            }
        }
    }
}
