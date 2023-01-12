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
using VRage.GameServices;
using VRageMath;

namespace CrunchDistressSignals
{
    public class Commands : CommandModule
    {
        public static Dictionary<long, DateTime> distressCooldowns = new Dictionary<long, DateTime>();
        public static int distressCount = 0;
        [Command("admindistress", "admin distress signals")]
        [Permission(MyPromoteLevel.Admin)]
        public void admindistress(string name, string reason = "Admin Distress Signal", int red = 52, int green = 235, int blue = 225)
        {
            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

            var color = new Color(red, green, blue);
            var message = new DistressSignal
            {
                GPS = Context.Player.Character.PositionComp.GetPosition(),
                PlayerName = name,
                Color = color,
                SendToGlobal = true,
                Reason = reason,
                Name = name
            };

            Context.Respond($"Sending signal to global" );
            Core.SendToMQ(MQPatching.MQPluginPatch.GlobalDistressSignals, message);
        }

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

            var playerName = Context.Player.DisplayName;

            if (Core.DistressGroups.Any(x =>
                    string.Equals(x.Name, reason, StringComparison.CurrentCultureIgnoreCase) || x.Aliases.Any(z => string.Equals(z, reason, StringComparison.CurrentCultureIgnoreCase))))
            {
                var signal = Core.DistressGroups.First(x => string.Equals(x.Name, reason, StringComparison.CurrentCultureIgnoreCase) || x.Aliases.Any(z => string.Equals(z, reason, StringComparison.CurrentCultureIgnoreCase)));
                //send a distress signal, and a new object 
                var distress = new DistressSignal
                {
                    GPS = Context.Player.Character.PositionComp.GetPosition(),
                    PlayerName = signal.Name,
                    Color = signal.Color,
                    SteamIds = signal.SteamIdsToSendTo,
                    SendToGlobal = false,
                    Reason = reason
                };
                Core.SendToMQ(MQPatching.MQPluginPatch.DistressSignals, distress);
                Core.SendToDiscord(distress, signal);
                Context.Respond("Sending signal to distress group.");
                return;
            }

            var data = Core.PlayerDataProvier.GetPlayerData(Context.Player.SteamUserId);

            if (Core.AlliancePluginInstalled && data.SendToAlliance)
            {
                var methodInput = new object[] { playerFac.Tag };
                var members = (List<long>) Core.GetAllianceMembers?.Invoke(null, methodInput);

                var distress = new DistressSignal
                {
                    GPS = Context.Player.Character.PositionComp.GetPosition(),
                    PlayerName = playerName + " - Alliance Distress ",
                    Color = Color.Yellow,
                    FactionsToSendTo = members,
                    SendToGlobal = false,
                    Reason = reason
                };
                if (members.Any())
                {
                    Context.Respond("Sending signal to alliance.");
                    Core.SendToMQ(MQPatching.MQPluginPatch.DistressSignals, distress);
                    return;
                }
            }

            var message = new DistressSignal
            {
                GPS = Context.Player.Character.PositionComp.GetPosition(),
                PlayerName = playerName + " - Distress ",
                Color = Color.Purple,
                FactionsToSendTo = data.FactionsToSendTo,
                SendToGlobal = data.SendToGlobal,
                Reason = reason
            };
            if (message.FactionsToSendTo.Any())
            {
                message.SendToGlobal = false;
            }

            Context.Respond(data.SendToGlobal ? $"Sending signal to global" : $"Sending signal to listed factions");
            Core.SendToMQ(data.SendToGlobal ? MQPatching.MQPluginPatch.GlobalDistressSignals : MQPatching.MQPluginPatch.DistressSignals, message);
        }
    }

    [Category("dconfig")]
    public class PlayerConfigCommands : CommandModule
    {
        [Command("reload", "reload configs")]
        [Permission(MyPromoteLevel.Admin)]
        public void reload()
        {
            Core.LoadConfigs();
            Context.Respond("Done");
        }

        [Command("addfac", "add a faction tag, or list of tags seperated by , to whitelist")]
        [Permission(MyPromoteLevel.None)]
        public void addTag(string factionTags)
        {
            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

            var tags = factionTags.Replace(" ", "").Split(',').ToList();
            var data = Core.PlayerDataProvier.GetPlayerData(Context.Player.SteamUserId);
            foreach (var tag in tags)
            {
                
                IMyFaction target = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (target == null)
                {
                    Context.Respond($"Target faction not found. {tag}");
                    return;
                }

                if (!data.FactionsToSendTo.Contains(target.FactionId))
                {
                    data.FactionsToSendTo.Add(target.FactionId);
                }
            }
            Context.Respond("Added factions to whitelist");
            Core.PlayerDataProvier.SavePlayerData(Context.Player.SteamUserId, data);
        }

        [Command("alliance", "toggle alliance settings")]
        [Permission(MyPromoteLevel.None)]
        public void toggleAlliance()
        {
            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

            var data = Core.PlayerDataProvier.GetPlayerData(Context.Player.SteamUserId);
            data.SendToAlliance = !data.SendToAlliance;
            Context.Respond($"Toggled alliance signals to {data.SendToAlliance}");
            Core.PlayerDataProvier.SavePlayerData(Context.Player.SteamUserId, data);
        }

        [Command("removefac", "add a faction tag, or list of tags seperated by , to whitelist")]
        [Permission(MyPromoteLevel.None)]
        public void removeTag(string factionTags)
        {
            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

            var tags = factionTags.Replace(" ", "").Split(',').ToList();
            var data = Core.PlayerDataProvier.GetPlayerData(Context.Player.SteamUserId);
            foreach (var tag in tags)
            {

                IMyFaction target = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (target == null)
                {
                    Context.Respond($"Target faction not found. {tag}");
                    return;
                }

                if (data.FactionsToSendTo.Contains(target.FactionId))
                {
                    data.FactionsToSendTo.Remove(target.FactionId);
                }
            }
            Context.Respond("Removed factions from whitelist.");
            Core.PlayerDataProvier.SavePlayerData(Context.Player.SteamUserId, data);
        }
    }
}
