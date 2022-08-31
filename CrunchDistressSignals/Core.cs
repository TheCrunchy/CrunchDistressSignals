using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers;
using Torch.Managers.PatchManager;
using Torch.Session;

namespace CrunchDistressSignals
{
    public class Core : TorchPluginBase
    {
        public static Config Config;
        public static string PlayerStoragePath;
        public static Logger Log = LogManager.GetLogger("DistressSignals");
        public static ITorchPlugin MQ;
        public static MethodInfo SendMessage;
        public static bool AlliancePluginInstalled = false;
        public static ITorchPlugin Alliances;
        public static MethodInfo GetAllianceMembers;
        public static bool MQPluginInstalled = false;
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();

            if (sessionManager != null)
            {
                sessionManager.SessionStateChanged += SessionChanged;
            }

            SetupConfig();

        }

        private void SetupConfig()
        {
            FileUtils utils = new FileUtils();

            var path = $"{StoragePath}\\DistressConfig.xml";
            if (File.Exists(path))
            {
                Config = utils.ReadFromXmlFile<Config>(path);
                utils.WriteToXmlFile<Config>(path, Config, false);
            }
            else
            {
                Config = new Config();
                utils.WriteToXmlFile<Config>(path, Config, false);
            }
            if (Config.StoragePath.Equals("Default"))
            {
                PlayerStoragePath = Path.Combine($"{StoragePath}//DistressData");
                Directory.CreateDirectory(StoragePath + "//DistressData");
            }
            else
            {
                PlayerStoragePath = Config.StoragePath;
            }
        }


        public static bool SendToMQ(string Type, Object SendThis)
        {
            if (!MQPluginInstalled)
            {
                return false;
            }
            var input = JsonConvert.SerializeObject(SendThis);
            var methodInput = new object[] { Type, input };
            SendMessage?.Invoke(MQ, methodInput);
            return true;
        }

        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {

        }

        public void InitPluginDependencies(PluginManager Plugins, PatchManager Patches)
        {
            if (Torch.Managers.GetManager<PluginManager>().Plugins.TryGetValue(Guid.Parse("74796707-646f-4ebd-8700-d077a5f47af3"),
                    out var AlliancePlugin))
            {
                try
                {
                    var AllianceIntegration =
                        AlliancePlugin.GetType().Assembly.GetType("AlliancesPlugin.Integrations.AllianceIntegrationCore");

                    GetAllianceMembers = AllianceIntegration.GetMethod("GetAllianceMembers", BindingFlags.Public | BindingFlags.Static);
                    Alliances = AlliancePlugin;
                    AlliancePluginInstalled = true;
                }
                catch (Exception ex)
                {
                    Log.Error("Error loading the alliance integration");
                }
            }
            else
            {
                Log.Info("Alliances not installed");
            }
            if (Plugins.Plugins.TryGetValue(Guid.Parse("319afed6-6cf7-4865-81c3-cc207b70811d"), out var MQPlugin))
            {
                SendMessage = MQPlugin.GetType().GetMethod("SendMessage", BindingFlags.Public | BindingFlags.Instance, null, new Type[2] { typeof(string), typeof(string) }, null);
                MQ = MQPlugin;

                MQPatching.MQPluginPatch.Patch(Patches.AcquireContext());
                Patches.Commit();

                MQPluginInstalled = true;
            }
        }
        public bool InitPlugins = false;
        public override void Update()
        {
            if (!InitPlugins)
            {
                InitPluginDependencies(Torch.Managers.GetManager<PluginManager>(), Torch.Managers.GetManager<PatchManager>());
                InitPlugins = true;
            }
        }
    }
}

