using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.API;

namespace CrunchDistressSignals.PlayerData
{
    public class PlayerDataProvider : IPlayerDataProvider
    {
        public Dictionary<ulong, PlayerData> PlayerData = new Dictionary<ulong, PlayerData>();
        public string FolderLocation;
        private FileUtils utils = new FileUtils();
        public PlayerDataProvider(string FolderLocation)
        {
            this.FolderLocation = FolderLocation;
            Directory.CreateDirectory(FolderLocation);
        }

        public PlayerData GetPlayerData(ulong SteamId)
        {
            return PlayerData.TryGetValue(SteamId, out var data) ? data : LoadPlayerData(SteamId);
        }

        public PlayerData LoadPlayerData(ulong SteamId)
        {
            if (File.Exists($"{FolderLocation}{SteamId}.Json"))
            {
                return utils.ReadFromJsonFile<PlayerData>($"{FolderLocation}{SteamId}.Json");
            }

            var newData = new PlayerData();
            utils.WriteToJsonFile($"{FolderLocation}{SteamId}.Json", newData);
            return newData;
        }

        public void SavePlayerData(ulong SteamId, PlayerData data)
        {
            utils.WriteToJsonFile($"{FolderLocation}{SteamId}.Json", data);
        }

        public void Login(IPlayer p)
        {
            if (p == null)
            {
                return;
            }
            var data = LoadPlayerData(p.SteamId);
            SavePlayerData(p.SteamId, data);
        }

        public void Logout(IPlayer p)
        {
            if (p == null)
            {
                return;
            }
            var data = GetPlayerData(p.SteamId);
            SavePlayerData(p.SteamId, data);
        }

    }
}
