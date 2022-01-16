using AltV.Net;
using AltV.Net.Async;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using System;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class StorageHandler : IScript
    {

        public static async Task openStorage(ClassicPlayer player)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || player.Dimension <= 0 || !ServerStorages.ExistStorage(player.Dimension)) return;
            int storageId = player.Dimension;
            player.Emit("Client:Storage:openStorage", 1, storageId, CharactersInventory.GetCharacterInventory(User.GetPlayerOnline(player)), ServerStorages.GetStorageItemJSON(storageId));
        }
        public static async Task openStorage2(ClassicPlayer player)
        {
            if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || player.Dimension <= 0 || !ServerStorages.ExistStorage(player.Dimension)) return;
            if (ServerStorages.GetFactionid(player.Dimension) != ServerFactions.GetCharacterFactionId((int)player.GetCharacterMetaId())) { HUDHandler.SendNotification(player, 4, 5000, "Dazu hast du kein Zugriff (LSPD)"); return; }
            int storageId = player.Dimension;
            player.Emit("Client:Storage:openStorage", 1, storageId, CharactersInventory.GetCharacterInventory(User.GetPlayerOnline(player)), ServerStorages.GetStorageItemJSON(storageId));
        }

        [AsyncClientEvent("Server:Storage:switchItemToStorage")]
        public async Task switchItemToStorage(ClassicPlayer player, int storageId, string itemName, int itemAmount)
        {
            try
            {
                // Inventory -> Storage
                if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || player.Dimension <= 0 || itemName.Length <= 0 || itemAmount <= 0 || storageId <= 0 || !ServerStorages.ExistStorage(storageId)) return;
                if (itemAmount > CharactersInventory.GetCharacterItemAmount(User.GetPlayerOnline(player), itemName, "inventory"))
                {
                    HUDHandler.SendNotification(player, 3, 1500, "Soviele Gegenstände hast du nicht dabei.");
                    return;
                }

                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                if (ServerStorages.GetWeight(storageId) + itemWeight > ServerStorages.GetMaxSize(storageId))
                {
                    HUDHandler.SendNotification(player, 4, 5000, $"Fehler: Soviel Platz hat deine Lagerhalle nicht (maximal: {ServerStorages.GetMaxSize(storageId)}kg).");
                    return;
                }

                ServerStorages.AddItem(storageId, itemName, itemAmount);
                CharactersInventory.RemoveCharacterItemAmount(User.GetPlayerOnline(player), itemName, itemAmount, "inventory");
                HUDHandler.SendNotification(player, 4, 1500, $"Du hast {itemAmount}x {itemName} in die Lagerhalle gelegt.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [AsyncClientEvent("Server:Storage:switchItemToInventory")]
        public async Task switchItemToInventory(ClassicPlayer player, int storageId, string itemName, int itemAmount)
        {
            try
            {
                // Storage -> Inventory
                if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0 || player.Dimension <= 0 || itemName.Length <= 0 || itemAmount <= 0 || storageId <= 0 || !ServerStorages.ExistStorage(storageId)) return;
                if (itemAmount > ServerStorages.GetItemAmount(storageId, itemName))
                {
                    HUDHandler.SendNotification(player, 3, 1500, "Soviele Gegenstände sind nicht in der Lagerhalle.");
                    return;
                }
                float itemWeight = ServerItems.GetItemWeight(itemName) * itemAmount;
                if (CharactersInventory.GetCharacterItemWeight(User.GetPlayerOnline(player), "inventory") + itemWeight > 15f)
                {
                    HUDHandler.SendNotification(player, 4, 5000, "Fehler: Du hast nicht genügend Platz für diese Gegenstände.");
                    return;
                }

                ServerStorages.RemoveItemAmount(storageId, itemName, itemAmount);
                CharactersInventory.AddCharacterItem(User.GetPlayerOnline(player), itemName, itemAmount, "inventory");
                HUDHandler.SendNotification(player, 2, 1500, $"Du hast {itemAmount}x {itemName} aus der Lagerhalle entnommen.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}