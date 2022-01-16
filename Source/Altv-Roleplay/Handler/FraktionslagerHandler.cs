using AltV.Net;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class FraktionslagerHandler : IScript
    {
        public static async Task openFraktionslager(ClassicPlayer player)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || player.Dimension <= 0 || !ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) || !ServerFactions.ExistFaction(player.Dimension)) return;
            int factionId = player.Dimension;
            await LoginHandler.setCefStatus(player, true);
        }
    }
}
