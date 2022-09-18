using Torch.API;

namespace CrunchDistressSignals.PlayerData
{
    public interface IPlayerDataProvider
    {
        PlayerData GetPlayerData(ulong SteamId);
        PlayerData LoadPlayerData(ulong SteamId);
        void SavePlayerData(ulong SteamId, PlayerData data);
        void Login(IPlayer p);
        void Logout(IPlayer p);
    }
}