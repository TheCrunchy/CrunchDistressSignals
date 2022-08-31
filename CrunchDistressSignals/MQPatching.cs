using System;
using System.Collections.Generic;
using System.Reflection;
using CrunchDistressSignals.Helpers;
using Newtonsoft.Json;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.Managers.PatchManager;

namespace CrunchDistressSignals
{
    public class MQPatching
    {
        public static class MQPluginPatch
        {
            internal static readonly MethodInfo HandleMessagePatch = typeof(MQPluginPatch).GetMethod(nameof(HandleMessage), BindingFlags.Static | BindingFlags.Public) ??
                                                                     throw new Exception("Failed to find patch method");

            private static Dictionary<string, Action<string>> Handlers = new Dictionary<string, Action<string>>();

            public static string DistressSignals = "DistressSignal";
            public static string GlobalDistressSignals = "GlobalDistressSignal";

            public static void Patch(PatchContext ctx)
            {
                var HandleMessageMethod = Core.MQ.GetType().GetMethod("MessageHandler", BindingFlags.Instance | BindingFlags.Public);
                if (HandleMessageMethod == null) return;

                ctx.GetPattern(HandleMessageMethod).Suffixes.Add(HandleMessagePatch);
                Handlers.Add(DistressSignals, HandleDistress);
                Handlers.Add(GlobalDistressSignals, HandleDistress);
            }

            public static void HandleDistress(string MessageBody)
            {
                var DistressSignal = JsonConvert.DeserializeObject<CrunchDistressSignals.Models.DistressSignal>(MessageBody);
                var gps = GPSHelper.CreateGps(DistressSignal.GPS, DistressSignal.Color, DistressSignal.PlayerName, DistressSignal.Reason);
                var gpscol = (MyGpsCollection)MyAPIGateway.Session?.GPS;
                foreach (var player in MySession.Static.Players.GetOnlinePlayers())
                {
                    var fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
                    if (fac != null && DistressSignal.FactionsToSendTo.Contains(fac.FactionId))
                    {
                        gpscol.SendAddGpsRequest(player.Identity.IdentityId, ref gps);
                    }
                }
            }
            public static void HandleGlobalDistress(string MessageBody)
            {
                var DistressSignal = JsonConvert.DeserializeObject<CrunchDistressSignals.Models.DistressSignal>(MessageBody);
                var gps = DistressSignal.GPS;
                if (DistressSignal.SendToGlobal) return;
                foreach (var player in MySession.Static.Players.GetOnlinePlayers())
                {

                }
            }

            public static void HandleMessage(string MessageType, string MessageBody)
            {
                if (Handlers.TryGetValue(MessageType, out var action))
                {
                    action.Invoke(MessageBody);
                }
            }
        }
    }
}
