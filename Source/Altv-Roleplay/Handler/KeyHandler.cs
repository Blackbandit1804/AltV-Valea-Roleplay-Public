
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class KeyHandler : IScript

    {


        [AsyncClientEvent("Server:KeyHandler:PressE")]
        public async Task PressE(IPlayer player)
        {
            lock (player)
            {
                if (player == null || !player.Exists) return;
                int charId = User.GetPlayerOnline(player);
                if (charId == 0) return;


                ClassicColshape serverDoorLockCol = (ClassicColshape)ServerDoors.ServerDoorsLockColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                if (serverDoorLockCol != null)
                {
                    var doorColData = ServerDoors.ServerDoors_.FirstOrDefault(x => x.id == (int)serverDoorLockCol.GetColShapeId());
                    if (doorColData != null)
                    {
                        string doorKey = doorColData.doorKey;
                        string doorKey2 = doorColData.doorKey2;
                        if (doorKey == null || doorKey2 == null) return;
                        if (ServerFactions.GetCharacterFactionId(charId) != doorColData.FactionId && ServerFactions.GetCharacterFactionId(charId) != 8 && !(doorColData.FactionId == 1 && ServerFactions.GetCharacterFactionId(charId) == 3)) return;
                        if (!doorColData.state) { HUDHandler.SendNotification(player, 4, 1500, "Tür abgeschlossen."); }
                        else { HUDHandler.SendNotification(player, 2, 1500, "Tür aufgeschlossen."); }
                        doorColData.state = !doorColData.state;
                        Alt.EmitAllClients("Client:DoorManager:ManageDoor", doorColData.hash, new Position(doorColData.posX, doorColData.posY, doorColData.posZ), (bool)doorColData.state);
                        return;
                    }
                }

                ClassicColshape farmCol = (ClassicColshape)ServerFarmingSpots.ServerFarmingSpotsColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                if (farmCol != null && !player.IsInVehicle)
                {
                    UseFarmSpot(player as ClassicPlayer);
                }

                ClassicColshape farmProducerCol = (ClassicColshape)ServerFarmingSpots.ServerFarmingProducerColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                if (farmProducerCol != null && !player.IsInVehicle)
                {
                    if (player.GetPlayerFarmingActionMeta() != "None") { HUDHandler.SendNotification(player, 3, 5000, $"Warte einen Moment."); return; }
                    var farmColData = ServerFarmingSpots.ServerFarmingProducer_.FirstOrDefault(x => x.id == (int)farmProducerCol.GetColShapeId());
                    if (farmColData != null)
                    {
                        //FarmingHandler.ProduceItem(player, farmColData.neededItem, farmColData.producedItem, farmColData.neededItemAmount, farmColData.producedItemAmount, farmColData.duration);
                        FarmingHandler.openFarmingCEF(player, farmColData.neededItem, farmColData.producedItem, farmColData.neededItemAmount, farmColData.producedItemAmount, farmColData.duration, farmColData.neededItemTWO, farmColData.neededItemTHREE, farmColData.neededItemTWOAmount, farmColData.neededItemTHREEAmount);
                        return;
                    }
                }

                if (((ClassicColshape)Minijobs.Elektrolieferant.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.Elektrolieferant.Main.StartMinijob(player);
                    return;
                }

                if (((ClassicColshape)Minijobs.Pilot.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.Pilot.Main.TryStartMinijob(player);
                    return;
                }

                if (((ClassicColshape)Minijobs.Müllmann.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.Müllmann.Main.StartMinijob(player);
                    return;
                }

                if (((ClassicColshape)Minijobs.Busfahrer.Main.startJobShape).IsInRange((ClassicPlayer)player))
                {
                    Minijobs.Busfahrer.Main.TryStartMinijob(player);
                    return;
                }

                var houseEntrance = ServerHouses.ServerHouses_.FirstOrDefault(x => ((ClassicColshape)x.entranceShape).IsInRange((ClassicPlayer)player));
                if (houseEntrance != null && player.Dimension == 0)
                {
                    HouseHandler.openEntranceCEF(player, houseEntrance.id);
                    return;
                }

                var hotelPos = ServerHotels.ServerHotels_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (hotelPos != null && !player.IsInVehicle)
                {
                    HotelHandler.openCEF(player, hotelPos);
                    return;
                }

                if (player.Dimension >= 5000)
                {
                    int houseInteriorCount = ServerHouses.GetMaxInteriorsCount();
                    for (var i = 1; i <= houseInteriorCount; i++)
                    {
                        if (i > houseInteriorCount || i <= 0) continue;
                        if ((player.Dimension >= 5000 && player.Dimension < 10000) && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                        {
                            //Apartment Leave
                            HotelHandler.LeaveHotel(player);
                            return;
                        }
                        else if ((player.Dimension >= 5000 && player.Dimension < 10000) && player.Position.IsInRange(ServerHouses.GetInteriorStoragePosition(i), 2f))
                        {
                            //Apartment Storage
                            HotelHandler.openStorage(player);
                            return;
                        }
                        else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                        {
                            //House Leave
                            HouseHandler.LeaveHouse(player, i);
                            return;
                        }
                        else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorStoragePosition(i), 2f))
                        {
                            //House Storage
                            HouseHandler.openStorage(player);
                            return;
                        }
                        else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorManagePosition(i), 2f))
                        {
                            //Hausverwaltung
                            HouseHandler.openManageCEF(player);
                            return;
                        }
                    }
                }

                var teleportsPos = ServerItems.ServerTeleports_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1.5f));
                if (teleportsPos != null && !player.IsInVehicle)
                {
                    player.Position = new Position(teleportsPos.targetX, teleportsPos.targetY, teleportsPos.targetZ + 0.5f);
                    return;
                }

                var shopPos = ServerShops.ServerShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 3f));
                if (shopPos != null && !player.IsInVehicle)
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                    ShopHandler.openShop(player, shopPos);
                    return;
                }

                var garagePos = ServerGarages.ServerGarages_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (garagePos != null && !player.IsInVehicle)
                {
                    GarageHandler.OpenGarageCEF(player, garagePos.id);
                    return;
                }

                var schluesselPos = player.Position.IsInRange(Constants.Positions.Vehicleschluesseldienst_Position, 3f);
                if (schluesselPos != false && !player.IsInVehicle)
                {
                    /*                    HUDHandler.SendNotification(player, 2, 3500, "Schluesseldienst oeffnet...! Bitte Warten!");
                    */
                    ShopHandler.openschluesselShop((ClassicPlayer)player);
                    return;
                }

                var waschPos = player.Position.IsInRange(Constants.Positions.Waschstrasse, 7.5f);
                var waschPos2 = player.Position.IsInRange(Constants.Positions.Waschstrasse2, 7.5f);
                var waschPos3 = player.Position.IsInRange(Constants.Positions.Waschstrasse3, 7.5f);
                var waschPos4 = player.Position.IsInRange(Constants.Positions.Waschstrasse4, 7.5f);
                var waschPos5 = player.Position.IsInRange(Constants.Positions.Waschstrasse5, 7.5f);
                var waschPos6 = player.Position.IsInRange(Constants.Positions.Waschstrasse6, 7.5f);
                var waschPos7 = player.Position.IsInRange(Constants.Positions.Waschstrasse7, 7.5f);
                if (waschPos != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }
                if (waschPos2 != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }
                if (waschPos3 != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }
                if (waschPos4 != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }
                if (waschPos5 != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }
                if (waschPos6 != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }
                if (waschPos7 != false && player.IsInVehicle)
                {
                    /*                  HUDHandler.SendNotification(player, 2, 3500, "Fahrzeug wird gewaschen! Bitte Warten!");
                    */
                    ShopHandler.usewaschstrasse(player);
                    return;
                }

                var clothesShopPos = ServerClothesShops.ServerClothesShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (clothesShopPos != null && !player.IsInVehicle)
                {
                    /*                    HUDHandler.SendNotification(player, 2, 3500, "Kleidungsladen oeffnet...! Bitte Warten!");
                    */
                    ShopHandler.openClothesShop((ClassicPlayer)player, clothesShopPos.id);
                    return;
                }

                var vehicleShopPos = ServerVehicleShops.ServerVehicleShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.pedX, x.pedY, x.pedZ), 2f));
                if (vehicleShopPos != null && !player.IsInVehicle)
                {
                    if (vehicleShopPos.neededLicense != "None" && !Characters.HasCharacterPermission(charId, vehicleShopPos.neededLicense)) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast nicht die benötigte Lizenz."); return; }
                    //LSPD
                    if (vehicleShopPos.id == 6 && ServerFactions.GetCharacterFactionId(charId) != 1 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSPD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSPD]"); return; }
                    if (vehicleShopPos.id == 7 && ServerFactions.GetCharacterFactionId(charId) != 1 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSPD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSPD]"); return; }
                    //MD
                    if (vehicleShopPos.id == 8 && ServerFactions.GetCharacterFactionId(charId) != 4 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel MD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [MD]"); return; }
                    if (vehicleShopPos.id == 9 && ServerFactions.GetCharacterFactionId(charId) != 4 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel MD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [MD]"); return; }
                    //ACLS
                    if (vehicleShopPos.id == 10 && ServerFactions.GetCharacterFactionId(charId) != 5 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel ACLS", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [ACLS]"); return; }
                    //Fib
                    if (vehicleShopPos.id == 26 && ServerFactions.GetCharacterFactionId(charId) != 2 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel FIB", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [FIB]"); return; }

                    //LSF
                    if (vehicleShopPos.id == 23 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    if (vehicleShopPos.id == 24 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    if (vehicleShopPos.id == 99999 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    if (vehicleShopPos.id == 99998 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    //VUC
                    if (vehicleShopPos.id == 1000 && player.AdminLevel() <= 7) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [ADMINSHOP]"); return; }
                    //REST
                    ShopHandler.OpenVehicleShop(player, vehicleShopPos.name, vehicleShopPos.id);
                    return;
                }

                var vehicleSellPos = ServerVehicleShops.ServerVehicleShops_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.sellX, x.sellY, x.sellZ), 10f));
                if (vehicleSellPos != null && player.IsInVehicle)
                {
                    if (vehicleSellPos.neededLicense != "None" && !Characters.HasCharacterPermission(charId, vehicleSellPos.neededLicense)) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast nicht die benötigte Lizenz."); return; }
                    //LSPD
                    if (vehicleSellPos.id == 6 && ServerFactions.GetCharacterFactionId(charId) != 1 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSPD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSPD]"); return; }
                    if (vehicleSellPos.id == 7 && ServerFactions.GetCharacterFactionId(charId) != 1 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSPD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSPD]"); return; }
                    //MD
                    if (vehicleSellPos.id == 8 && ServerFactions.GetCharacterFactionId(charId) != 3 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel MD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [MD]"); return; }
                    if (vehicleSellPos.id == 9 && ServerFactions.GetCharacterFactionId(charId) != 3 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel MD", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [MD]"); return; }
                    //FIB
                    if (vehicleSellPos.id == 26 && ServerFactions.GetCharacterFactionId(charId) != 2 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel FIB", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [FIB]"); return; }
                    //ACLS
                    if (vehicleSellPos.id == 10 && ServerFactions.GetCharacterFactionId(charId) != 4 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel ACLS", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [ACLS]"); return; }
                    //LSF
                    if (vehicleSellPos.id == 23 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    if (vehicleSellPos.id == 24 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    if (vehicleSellPos.id == 99999 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    if (vehicleSellPos.id == 99998 && ServerFactions.GetCharacterFactionId(charId) != 6 && !CharactersInventory.ExistCharacterItem(charId, "Fahrzeugschluessel LSF", "schluessel")) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [LSF]"); return; }
                    //VUC
                    if (vehicleSellPos.id == 1000 && player.AdminLevel() <= 7) { HUDHandler.SendNotification(player, 3, 5000, $"Du hast hier keinen Zugriff drauf. [ADMINSHOP]"); return; }
                    //REST
                    ShopHandler.SellVehicle(player, vehicleSellPos.name, vehicleSellPos.id);
                    return;
                }

                var bankPos = ServerBanks.ServerBanks_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 1f));
                if (bankPos != null && !player.IsInVehicle)
                {
                    if (bankPos.zoneName == "Maze Bank Fraktion")
                    {
                        if (!ServerFactions.IsCharacterInAnyFaction(charId)) return;
                        if (ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) && ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) - 1) { return; }
                        player.EmitLocked("Client:FactionBank:createCEF", "faction", ServerFactions.GetCharacterFactionId(charId), ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)));
                        return;
                    }
                    if (bankPos.zoneName == "LSPD Bank Fraktion")
                    {
                        if (!ServerFactions.IsCharacterInAnyFaction(charId)) return;
                        if (ServerFactions.GetCharacterFactionId(charId) != 2) { HUDHandler.SendNotification(player, 4, 5000, "Fraktionsbank vom LSPD - Zugriff verweiget"); return; }
                        if (ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) && ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) - 1) { return; }
                        player.EmitLocked("Client:FactionBank:createCEF", "faction", ServerFactions.GetCharacterFactionId(charId), ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)));
                        return;
                    }
                    if (bankPos.zoneName == "LSMD Bank Fraktion")
                    {
                        if (!ServerFactions.IsCharacterInAnyFaction(charId)) return;
                        if (ServerFactions.GetCharacterFactionId(charId) != 4) { HUDHandler.SendNotification(player, 4, 5000, "Fraktionsbank vom LSMD - Zugriff verweiget"); return; }
                        if (ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) && ServerFactions.GetCharacterFactionRank(charId) != ServerFactions.GetFactionMaxRankCount(ServerFactions.GetCharacterFactionId(charId)) - 1) { return; }
                        player.EmitLocked("Client:FactionBank:createCEF", "faction", ServerFactions.GetCharacterFactionId(charId), ServerFactions.GetFactionBankMoney(ServerFactions.GetCharacterFactionId(charId)));
                        return;
                    }
                    else if (bankPos.zoneName == "Maze Bank Company")
                    {
                        if (!ServerCompanys.IsCharacterInAnyServerCompany(charId)) return;
                        if (ServerCompanys.GetCharacterServerCompanyRank(charId) != 1 && ServerCompanys.GetCharacterServerCompanyRank(charId) != 2) { HUDHandler.SendNotification(player, 3, 5000, "Du hast kein Unternehmen auf welches du zugreifen kannst."); return; }
                        player.EmitLocked("Client:FactionBank:createCEF", "company", ServerCompanys.GetCharacterServerCompanyId(charId), ServerCompanys.GetServerCompanyMoney(ServerCompanys.GetCharacterServerCompanyId(charId)));
                        return;
                    }
                    else
                    {
                        var bankArray = CharactersBank.GetCharacterBankAccounts(charId);
                        player.EmitLocked("Client:Bank:createBankAccountManageForm", bankArray, bankPos.zoneName);
                        return;
                    }
                }

                var barberPos = ServerBarbers.ServerBarbers_.FirstOrDefault(x => player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                if (barberPos != null && !player.IsInVehicle)
                {
                    player.EmitLocked("Client:Barber:barberCreateCEF", Characters.GetCharacterHeadOverlays(charId));
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.VehicleLicensing_Position, 3f))
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                    VehicleHandler.OpenLicensingCEF(player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.Schwarzwasch, 5f))
                {
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                    _ = RobberyHandler.washmoney(player);
                    return;
                }

                if (ServerFactions.IsCharacterInAnyFaction(charId))
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    var factionDutyPos = ServerFactions.ServerFactionPositions_.FirstOrDefault(x => x.factionId == factionId && x.posType == "duty" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 5f));
                    if (factionDutyPos != null && !player.IsInVehicle)
                    {
                        bool isDuty = ServerFactions.IsCharacterInFactionDuty(charId);
                        ServerFactions.SetCharacterInFactionDuty(charId, !isDuty);
                        if (isDuty)
                        {
                            HUDHandler.SendNotification(player, 4, 5000, "Du hast dich erfolgreich vom Dienst abgemeldet.");
                        }
                        else
                        {
                            HUDHandler.SendNotification(player, 2, 5000, "Du hast dich erfolgreich zum Dienst angemeldet.");
                        }
                        if (factionId == 1 || factionId == 12) SmartphoneHandler.RequestLSPDIntranet((ClassicPlayer)player);
                        return;
                    }

                    var factionStoragePos = ServerFactions.ServerFactionPositions_.FirstOrDefault(x => x.factionId == factionId && x.posType == "storage" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                    if (factionStoragePos != null && !player.IsInVehicle)
                    {
                        if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                        bool isDuty = ServerFactions.IsCharacterInFactionDuty(charId);
                        if (factionId <= 8)
                        {
                            if (isDuty)
                            {
                                var factionStorageContent = ServerFactions.GetServerFactionStorageItems(factionId, charId); //Fraktionsspind Items
                                var CharacterInvArray = CharactersInventory.GetCharacterInventory(charId); //Spieler Inventar
                                player.EmitLocked("Client:FactionStorage:openCEF", charId, factionId, "faction", CharacterInvArray, factionStorageContent);
                            }
                            else
                            {
                                HUDHandler.SendNotification(player, 2, 5000, "Du bist nicht zum Dienst angemeldet");
                            }
                            return;
                        }
                        else
                        {
                            var factionStorageContent = ServerFactions.GetServerFactionStorageItems(factionId, charId); //Fraktionsspind Items
                            var CharacterInvArray = CharactersInventory.GetCharacterInventory(charId); //Spieler Inventar
                            player.EmitLocked("Client:FactionStorage:openCEF", charId, factionId, "faction", CharacterInvArray, factionStorageContent);
                            return;
                        }
                    }

                    var factionServicePhonePos = ServerFactions.ServerFactionPositions_.ToList().FirstOrDefault(x => x.factionId == factionId && x.posType == "servicephone" && player.Position.IsInRange(new Position(x.posX, x.posY, x.posZ), 2f));
                    if (factionServicePhonePos != null && !player.IsInVehicle && ServerFactions.IsCharacterInFactionDuty(charId))
                    {
                        int activeLeitstelle = ServerFactions.GetCurrentServicePhoneOwner(factionId);

                        if (activeLeitstelle <= 0)
                        {
                            ServerFactions.UpdateCurrentServicePhoneOwner(factionId, charId);
                            ServerFactions.sendMsg(factionId, $"{Characters.GetCharacterName(charId)} hat das Leitstellentelefon deiner Fraktion übernommen.");
                            return;
                        }
                        if (activeLeitstelle != charId)
                        {
                            HUDHandler.SendNotification(player, 2, 5000, $"Die Leitstelle ist aktuell vom Mitarbeiter {Characters.GetCharacterName(activeLeitstelle)} besetzt.");
                            return;
                        }
                        if (activeLeitstelle == charId)
                        {
                            ServerFactions.UpdateCurrentServicePhoneOwner(factionId, 0);
                            ServerFactions.sendMsg(factionId, $"{Characters.GetCharacterName(charId)} hat das Leitstellentelefon deiner Fraktion abgelegt.");
                            return;
                        }
                    }
                }

                if (player.Position.IsInRange(Constants.Positions.Jobcenter_Position, 2.5f) && !Characters.IsCharacterCrimeFlagged(charId) && !player.IsInVehicle) //Arbeitsamt
                {
                    if (ServerFactions.GetCharacterFactionId(charId) > 0) { HUDHandler.SendNotification(player, 4, 7500, "Da du in einer Legalen Fraktion bist, kannst du vom Jobcenter kein Jobangebot bekommen!"); return; }
                    TownhallHandler.createJobcenterBrowser(player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.TownhallHouseSelector, 2.5f))
                {
                    TownhallHandler.openHouseSelector(player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.IdentityCardApply, 2.5f) && Characters.GetCharacterAccState(charId) == 0 && !player.IsInVehicle) //Rathaus IdentityCardApply
                {
                    TownhallHandler.tryCreateIdentityCardApplyForm(player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.Clothes_Police, 2.5f) && !player.IsInVehicle)
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    if (factionId == 1)
                    {
                        if (!player.HasData("HasPDClothesOn"))
                        {

                            //player.EmitLocked("Client:SpawnArea:setCharAccessory", 0, 46, 0);    //  Kopfbedeckung
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 0, 0);         //  Sonnenbrille
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 385, 0);       //  Oberbekleidung
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 30, 0);         //  Körper
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 186, 0);       //  Unterbekleidung
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 31, 0);       //  Hose 
                            player.EmitLocked("Client:SpawnArea:setCharAccessory", 7, 153, 0);       //  Gürtel
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 10, 8, 1);        //  Decals
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 25, 0);        //  Schuhe

                            //player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 57, 0);      // Schutzweste

                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten angezogen.");
                            player.SetData("HasPDClothesOn", true);
                            Characters.SetCharacterArmor(charId, 100);
                        }
                        else
                        {
                            _ = Characters.SetCharacterCorrectClothes(player);
                            HUDHandler.SendNotification(player, 4, 2500, "Du hast deine Arbeitsklamotten ausgezogen.");
                            player.DeleteData("HasPDClothesOn");
                        }
                    }
                    else
                    {
                        HUDHandler.SendNotification(player, 2, 2500, "Du darfst den Kleiderschrank nicht nutzen!");
                    }
                    return;
                }
          

                if (player.Position.IsInRange(Constants.Positions.FFA, 2.5f) && !player.IsInVehicle )
                {
                   if (player.HasData("FFA"))
                    {
                        HUDHandler.SendNotification(player, 4, 5000, "Du bist bereits in FFA"); return;
                    }
                     

                    HUDHandler.SendNotification(player, 4, 5000, "FFA Betreten");


                    player.RemoveAllWeaponsAsync();
                    Characters.SetCharacterWeapon(player, "PrimaryWeapon", "None");
                    Characters.SetCharacterWeapon(player, "PrimaryAmmo", 0);
                    Characters.SetCharacterWeapon(player, "SecondaryWeapon2", "None");
                    Characters.SetCharacterWeapon(player, "SecondaryWeapon", "None");
                    Characters.SetCharacterWeapon(player, "SecondaryAmmo2", 0);
                    Characters.SetCharacterWeapon(player, "SecondaryAmmo", 0);
                    Characters.SetCharacterWeapon(player, "FistWeapon", "None");
                    Characters.SetCharacterWeapon(player, "FistWeaponAmmo", 0);
                    Characters.SetCharacterPhoneEquipped(charId, false);
                  
                    player.Dimension = 3;
                    player.SetData("FFA", true);
                    int rnd = new Random().Next(1, 192);
                    
                    if (rnd >= 3 && rnd <= 24)
                    {
                        player.Spawn(new Position(258.52747f, -875.9077f, 29.212402f), 0);
                        player.Position = new Position(258.52747f, -875.9077f, 29.212402f);
                    }
                    else if (rnd >= 24 && rnd <= 48)
                    {
                        player.Spawn(new Position(218.42638f, -937.5165f, 24.140625f), 0);
                        player.Position = new Position(218.42638f, -937.5165f, 24.140625f);
                    }
                    else if (rnd >= 48 && rnd <= 72)
                    {
                        player.Spawn(new Position(204.03957f, -993.7187f, 30.088623f), 0);
                        player.Position = new Position(204.03957f, -993.7187f, 30.088623f);
                    }
                    else if (rnd >= 72 && rnd <= 96)
                    {
                        player.Spawn(new Position(207.53406f, -994.66815f, 29.279907f), 0);
                        player.Position = new Position(207.53406f, -994.66815f, 29.279907f);
                    }
                    else if (rnd >= 96 && rnd <= 120)
                    {
                        player.Spawn(new Position(160.68132f, -999.0857f, 29.330444f), 0);
                        player.Position = new Position(160.68132f, -999.0857f, 29.330444f);
                    }
                    else if (rnd >= 120 && rnd <= 144)
                    {
                        player.Spawn(new Position(143.92088f, -966.2769f, 29.549438f), 0);
                        player.Position = new Position(143.92088f, -966.2769f, 29.549438f);
                    }
                    else if (rnd >= 144 && rnd <= 168)
                    {
                        player.Spawn(new Position(158.01758f, -914.0967f, 30.156006f), 0);
                        player.Position = new Position(158.01758f, -914.0967f, 30.156006f);
                    }
                    else if (rnd >= 144 && rnd <= 168)
                    {
                        player.Spawn(new Position(158.01758f, -914.0967f, 30.156006f), 0);
                        player.Position = new Position(158.01758f, -914.0967f, 30.156006f);
                    }
                    else if (rnd >= 168 && rnd <= 192)
                    {
                        player.Spawn(new Position(184.8923f, -853.95166f, 31.150146f), 0);
                        player.Position = new Position(184.8923f, -853.95166f, 31.150146f);
                    }

                    Task.Delay(100);
                    player.GiveWeapon(AltV.Net.Enums.WeaponModel.HeavyPistol, 9999, true);
                    player.GiveWeapon(AltV.Net.Enums.WeaponModel.AssaultRifle, 9999, true);
                    player.GiveWeapon(AltV.Net.Enums.WeaponModel.SpecialCarbine, 9999, true);
                    player.GiveWeapon(AltV.Net.Enums.WeaponModel.BullpupRifle, 9999, true);
                    player.GiveWeapon(AltV.Net.Enums.WeaponModel.GusenbergSweeper, 9999, true);
                    player.GiveWeapon(AltV.Net.Enums.WeaponModel.AdvancedRifle, 9999, true);

                    player.Health = 200;
                    player.Armor = 100;
                 //   Pla(0x5a96ba4, 9999, true);
                    return;
                }

            

                if (player.Position.IsInRange(Constants.Positions.Clothes_Fib, 2.5f) && !player.IsInVehicle)
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    if (factionId == 2)
                    {
                        if (!player.HasData("HasFIBClothesOn"))
                        {

                            //player.EmitLocked("Client:SpawnArea:setCharAccessory", 0, 46, 0);    //  Kopfbedeckung
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 0, 0);         //  Sonnenbrille
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 402, 0);       //  Oberbekleidung
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 0, 0);         //  Körper
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 8, -1, 0);       //  Unterbekleidung
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 24, 0);       //  Hose 
                            player.EmitLocked("Client:SpawnArea:setCharAccessory", 7, 0, 0);       //  Gürtel
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 10, 0, 0);        //  Decals
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 7, 0);        //  Schuhe

                            //player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 57, 0);      // Schutzweste

                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten angezogen.");
                            player.SetData("HasFIBClothesOn", true);
                            Characters.SetCharacterArmor(charId, 100);
                          } 
                       else
                           {
                            _ = Characters.SetCharacterCorrectClothes(player);
                            HUDHandler.SendNotification(player, 4, 2500, "Du hast deine Arbeitsklamotten ausgezogen.");
                            player.DeleteData("HasFIBClothesOn");
                        }
                    }
                    else
                    {
                        HUDHandler.SendNotification(player, 2, 2500, "Du darfst den Kleiderschrank nicht nutzen!");
                    }
                    return;
                }


                if (player.Position.IsInRange(Constants.Positions.Clothes_Medic, 2.5f) && !player.IsInVehicle)
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    if (factionId == 4)
                    {
                        if (!player.HasData("HasMedicClothesOn"))
                        {
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 35, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 348, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 155, 0);
                            //player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 12, 1); // WESTE
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 10, 58, 1);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 97, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 85, 0);

                            player.EmitLocked("Client:SpawnArea:setCharAccessory", 7, 126, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 0, 0);
                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten angezogen.");
                            player.SetData("HasMedicClothesOn", true);
                            Characters.SetCharacterArmor(charId, 100);
                        }
                        else
                        {
                            _ = Characters.SetCharacterCorrectClothes(player);
                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten ausgezogen.");
                            player.DeleteData("HasMedicClothesOn");
                        }
                    }
                    else
                    {
                        HUDHandler.SendNotification(player, 2, 2500, "Du darfst den Kleiderschrank nicht nutzen!");
                    }
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.Clothes_ACLS, 2.5f) && !player.IsInVehicle)
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    if (factionId == 5)
                    {
                        if (!player.HasData("HasMechanicClothesOn"))
                        {
                            if (!Characters.GetCharacterGender((int)player.GetCharacterMetaId()))
                            {
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 144, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 405, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 44, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 25, 0);
                                player.EmitLocked("Client:SpawnArea:setCharAccessory", 2, 2, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 20, 0);

                                player.EmitLocked("Client:SpawnArea:setCharAccessory", 7, 0, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 0, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 0, 0);
                                HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten angezogen.");
                                player.SetData("HasMechanicClothesOn", true);
                                Characters.SetCharacterArmor(charId, 100);
                            }
                            else
                            {
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 150, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 252, 14);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 153, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 24, 0);
                                player.EmitLocked("Client:SpawnArea:setCharAccessory", 2, 2, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 40, 0);

                                player.EmitLocked("Client:SpawnArea:setCharAccessory", 7, 0, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 0, 0);
                                player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 0, 0);
                                HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten angezogen.");
                                player.SetData("HasMechanicClothesOn", true);
                                Characters.SetCharacterArmor(charId, 100);
                            }
                         
                        }
                        else
                        {
                            player.DeleteData("HasMechanicClothesOn");
                            _ = Characters.SetCharacterCorrectClothes(player);
                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten ausgezogen.");
                        }
                    }
                    else
                    {
                        HUDHandler.SendNotification(player, 2, 2500, "Du darfst den Kleiderschrank nicht nutzen!");
                    }
                    return;
                }
                if (player.Position.IsInRange(Constants.Positions.Clothes_VUC, 2.5f) && !player.IsInVehicle)
                {
                    int factionId = ServerFactions.GetCharacterFactionId(charId);
                    if (factionId == 51)
                    {
                        if (!player.HasData("HasVUCClothesOn"))
                        {
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 98, 6);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 248, 14);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 153, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 25, 0);
                            player.EmitLocked("Client:SpawnArea:setCharAccessory", 2, 2, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 43, 0);

                            player.EmitLocked("Client:SpawnArea:setCharAccessory", 7, 0, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 0, 0);
                            player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 0, 0);
                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten angezogen.");
                            player.SetData("HasVUCClothesOn", true);
                            Characters.SetCharacterArmor(charId, 100);
                        }
                        else
                        {
                            player.DeleteData("HasVUCClothesOn");
                            Characters.SetCharacterCorrectClothes(player);
                            HUDHandler.SendNotification(player, 2, 2500, "Du hast deine Arbeitsklamotten ausgezogen.");
                        }
                    }
                    else
                    {
                        HUDHandler.SendNotification(player, 2, 2500, "Du darfst den Kleiderschrank nicht nutzen!");
                    }
                    return;
                }

                var tattooShop = ServerTattooShops.ServerTattooShops_.ToList().FirstOrDefault(x => x.owner != 0 && player.Position.IsInRange(new Position(x.pedX, x.pedY, x.pedZ), 2.5f));
                if (tattooShop != null && !player.IsInVehicle)
                {
                    ShopHandler.openTattooShop((ClassicPlayer)player, tattooShop);
                    return;
                }

                if (player.Position.IsInRange(RobberyHandler.bankRobPosition, 2f) || player.Position.IsInRange(RobberyHandler.bankExitPosition, 2f))
                {
                    RobberyHandler.EnterExitBank((ClassicPlayer)player);
                    return;
                }

                var bankRobPosGold = RobberyHandler.bankPickUpPositions.ToList().FirstOrDefault(x => player.Position.IsInRange(x.position, 1f));
                if (bankRobPosGold != null)
                {
                    RobberyHandler.pickUpBankGold((ClassicPlayer)player, bankRobPosGold);
                    return;
                }

                if (player.Position.IsInRange(RobberyHandler.jeweleryRobPosition, 2f))
                {
                    RobberyHandler.robJewelery((ClassicPlayer)player);
                    return;
                }

                var laborEntry = ServerFactions.ServerFactions_.FirstOrDefault(x => player.Position.IsInRange(x.laborPos, 2.5f) && !x.isLaborLocked);
                if (laborEntry != null)
                {
                    player.Dimension = laborEntry.id;
                    player.Position = ServerFactions.GetLaborExitPosition(laborEntry.id);
                    return;
                }


                var waffenlaborEntry = ServerFactions.ServerFactions_.FirstOrDefault(x => player.Position.IsInRange(x.waffenlaborPos, 2.5f) && !x.isWaffenLaborLocked);
                if (waffenlaborEntry != null)
                {
                    player.Dimension = waffenlaborEntry.id;
                    player.Position = ServerFactions.GetWaffenLaborExitPosition(waffenlaborEntry.id);
                    return;
                }

                var fraktionslagerEntry = ServerFactions.ServerFactions_.FirstOrDefault(x => player.Position.IsInRange(x.fraktionslagerPos, 2.5f) && !x.isFraktionslagerLocked);
                if (fraktionslagerEntry != null)
                {
                    player.Dimension = fraktionslagerEntry.id;
                    player.Position = ServerFactions.GetFraktionslagerExitPosition(fraktionslagerEntry.id);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.weedLabor_InvPosition, 2.5f) && player.Dimension != 0 && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == player.Dimension)
                {
                    LaborHandler.openLabor((ClassicPlayer)player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.weedLabor_ExitPosition, 2.5f) && player.Dimension != 0)
                {
                    Server_Factions faction = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                    if (faction == null || faction.laborPos == new Position(0, 0, 0) || faction.isLaborLocked) return;
                    player.Position = faction.laborPos;
                    player.Dimension = 0;
                    return;
                }


                if (player.Position.IsInRange(Constants.Positions.waffenLabor_InvPosition, 2.5f) && player.Dimension != 0 && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == player.Dimension)
                {
                    WaffenlaborHandler.openLabor((ClassicPlayer)player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.waffenLabor_ExitPosition, 2.5f) && player.Dimension != 0)
                {
                    Server_Factions faction = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                    if (faction == null || faction.waffenlaborPos == new Position(0, 0, 0) || faction.isWaffenLaborLocked) return;
                    player.Position = faction.waffenlaborPos;
                    player.Dimension = 0;
                    return;
                }

                Server_Storages storageEntry = ServerStorages.ServerStorages_.ToList().FirstOrDefault(x => player.Position.IsInRange(x.entryPos, 2f) && !x.isLocked);
                if (storageEntry != null && !player.IsInVehicle)
                {
                    player.Dimension = storageEntry.id;
                    player.Position = Constants.Positions.storage_ExitPosition;
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.storage_ExitPosition, 2f) && player.Dimension != 0)
                {
                    Server_Storages storage = ServerStorages.ServerStorages_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                    if (storage == null || storage.entryPos == new Position(0, 0, 0) || storage.isLocked) return;
                    player.Position = storage.entryPos;
                    player.Dimension = 0;
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.fraktionslager_ExitPosition, 2.5f) && player.Dimension != 0)
                {
                    Server_Factions faction = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                    if (faction == null || faction.fraktionslagerPos == new Position(0, 0, 0) || faction.isFraktionslagerLocked) return;
                    player.Position = faction.fraktionslagerPos;
                    player.Dimension = 0;
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.storage_InvPosition, 2.5f) && player.Dimension != 0)
                {
                    StorageHandler.openStorage((ClassicPlayer)player);
                    return;
                }
                if (player.Position.IsInRange(Constants.Positions.storage_LSPDInvPosition, 2.5f) && ServerStorages.ExistStorage(player.Dimension))
                {
                    StorageHandler.openStorage2((ClassicPlayer)player);
                    return;
                }

                if (player.Position.IsInRange(Constants.Positions.dynasty8_positionStorage, 2f))
                {
                    player.Emit("Client:Dynasty8:create", "storages", ServerStorages.GetAccountStorages(User.GetPlayerOnline(player)), ServerStorages.GetFreeStorages());
                    return;
                }
            }
        }
        private async void UseFarmSpot(ClassicPlayer player)
        {
            int charId = User.GetPlayerOnline(player);
            ClassicColshape farmCol = (ClassicColshape)ServerFarmingSpots.ServerFarmingSpotsColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
            if (farmCol != null && !player.IsInVehicle)
            {
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                if (player.GetPlayerFarmingActionMeta() != "None") return;
                var farmColData = ServerFarmingSpots.ServerFarmingSpots_.FirstOrDefault(x => x.id == (int)farmCol.GetColShapeId());

                if (farmColData != null)
                {
                    if (farmColData.itemName.Contains("Eisenerz") && farmColData.neededItemToFarm != "None")
                    {
                        if (!CharactersInventory.ExistCharacterItem(charId, "Spitzhacke", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Spitzhacke", "backpack"))
                        {
                            HUDHandler.SendNotification(player, 3, 3500, $"Zum Farmen benötigst du: *Spitzhacke*.");
                            return;
                        }
                    }
                    if (farmColData.itemName.Contains("Kupfererz") && farmColData.neededItemToFarm != "None")
                    {
                        if (!CharactersInventory.ExistCharacterItem(charId, "Spitzhacke", "inventory") && !CharactersInventory.ExistCharacterItem(charId, "Spitzhacke", "backpack"))
                        {
                            HUDHandler.SendNotification(player, 3, 3500, $"Zum Farmen benötigst du: *Spitzhacke*.");
                            return;
                        }
                    }
                    player.SetPlayerFarmingActionMeta("farm");
                    FarmingHandler.FarmFieldAction(player, farmColData.itemName, farmColData.itemMinAmount, farmColData.itemMaxAmount, farmColData.animation, farmColData.duration);
                    return;
                }
            }
        }


        [AsyncClientEvent("Server:KeyHandler:PressU")]
        public async Task PressU(IPlayer player)
        {
            try
            {
                lock (player)
                {
                    if (player == null || !player.Exists) return;
                    int charId = User.GetPlayerOnline(player);
                    if (charId <= 0) return;
                    if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }

                    /*ClassicColshape serverDoorLockCol = (ClassicColshape)ServerDoors.ServerDoorsLockColshapes_.FirstOrDefault(x => ((ClassicColshape)x).IsInRange((ClassicPlayer)player));
                    if (serverDoorLockCol != null)
                    {
                        var doorColData = ServerDoors.ServerDoors_.FirstOrDefault(x => x.id == (int)serverDoorLockCol.GetColShapeId());
                        if (doorColData != null)
                        {
                            string doorKey = doorColData.doorKey;
                            string doorKey2 = doorColData.doorKey2;
                            if (doorKey == null || doorKey2 == null) return;
                            if (!CharactersInventory.ExistCharacterItem(charId, doorKey, "schluessel") && !CharactersInventory.ExistCharacterItem(charId, doorKey, "schluessel") && !CharactersInventory.ExistCharacterItem(charId, doorKey2, "schluessel") && !CharactersInventory.ExistCharacterItem(charId, doorKey2, "schluessel")) return;

                            if (!doorColData.state) { HUDHandler.SendNotification(player, 4, 1500, "Tür abgeschlossen."); }
                            else { HUDHandler.SendNotification(player, 2, 1500, "Tür aufgeschlossen."); }
                            doorColData.state = !doorColData.state;
                            Alt.EmitAllClients("Client:DoorManager:ManageDoor", doorColData.hash, new Position(doorColData.posX, doorColData.posY, doorColData.posZ), (bool)doorColData.state);
                            return;
                        }
                    }*/

                    if (player.Dimension >= 5000)
                    {
                        int houseInteriorCount = ServerHouses.GetMaxInteriorsCount();
                        for (var i = 1; i <= houseInteriorCount; i++)
                        {
                            if (player.Dimension >= 5000 && player.Dimension < 10000 && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                            {
                                //Hotel abschließen / aufschließen
                                if (player.Dimension - 5000 <= 0) continue;
                                int apartmentId = player.Dimension - 5000;
                                int hotelId = ServerHotels.GetHotelIdByApartmentId(apartmentId);
                                if (hotelId <= 0 || apartmentId <= 0) continue;
                                if (!ServerHotels.ExistHotelApartment(hotelId, apartmentId)) { HUDHandler.SendNotification(player, 3, 5000, "Ein unerwarteter Fehler ist aufgetreten [HOTEL-001]."); return; }
                                if (ServerHotels.GetApartmentOwner(hotelId, apartmentId) != charId) { HUDHandler.SendNotification(player, 3, 5000, "Du hast keinen Schlüssel."); return; }
                                HotelHandler.LockHotel(player, hotelId, apartmentId);
                                return;
                            }
                            else if (player.Dimension >= 10000 && player.Position.IsInRange(ServerHouses.GetInteriorExitPosition(i), 2f))
                            {
                                //Haus abschließen / aufschließen
                                if (player.Dimension - 10000 <= 0) continue;
                                int houseId = player.Dimension - 10000;
                                if (houseId <= 0) continue;
                                if (!ServerHouses.ExistHouse(houseId)) { HUDHandler.SendNotification(player, 3, 5000, "Ein unerwarteter Fehler ist aufgetreten [HOUSE-001]."); return; }
                                if (ServerHouses.GetHouseOwner(houseId) != charId && !ServerHouses.IsCharacterRentedInHouse(charId, houseId)) { HUDHandler.SendNotification(player, 3, 5000, "Dieses Haus gehört nicht dir und / oder du bist nicht eingemietet."); return; }
                                HouseHandler.LockHouse(player, houseId);
                                return;
                            }
                        }
                    }

                    var houseEntrance = ServerHouses.ServerHouses_.FirstOrDefault(x => ((ClassicColshape)x.entranceShape).IsInRange((ClassicPlayer)player));
                    if (houseEntrance != null)
                    {
                        HouseHandler.LockHouse(player, houseEntrance.id);
                    }


                    if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
                    var laborEntry = ServerFactions.ServerFactions_.FirstOrDefault(x => x.laborPos.IsInRange(player.Position, 2.5f));
                    if (laborEntry != null && !player.IsInVehicle && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == laborEntry.id)
                    {
                        if (laborEntry.isLaborLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Labor aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Labor abgeschlossen.");
                        ServerFactions.SetLaborLocked(laborEntry.id, !laborEntry.isLaborLocked);
                        return;
                    }

                    if (player.Position.IsInRange(Constants.Positions.weedLabor_ExitPosition, 2.5f) && player.Dimension != 0 && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == player.Dimension)
                    {
                        Server_Factions labor = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                        if (labor == null) return;
                        if (labor.isLaborLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Labor aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Labor abgeschlossen.");
                        ServerFactions.SetLaborLocked(labor.id, !labor.isLaborLocked);
                        return;
                    }

                    if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
                    var waffenlaborEntry = ServerFactions.ServerFactions_.FirstOrDefault(x => x.waffenlaborPos.IsInRange(player.Position, 2.5f));
                    if (waffenlaborEntry != null && !player.IsInVehicle && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == waffenlaborEntry.id)
                    {
                        if (waffenlaborEntry.isWaffenLaborLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Waffen Labor aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Waffen Labor abgeschlossen.");
                        ServerFactions.SetWaffenLaborLocked(waffenlaborEntry.id, !waffenlaborEntry.isWaffenLaborLocked);
                        return;
                    }

                    if (player.Position.IsInRange(Constants.Positions.waffenLabor_ExitPosition, 2.5f) && player.Dimension != 0 && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == player.Dimension)
                    {
                        Server_Factions waffenlabor = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                        if (waffenlabor == null) return;
                        if (waffenlabor.isWaffenLaborLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Waffen Labor aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Waffen Labor abgeschlossen.");
                        ServerFactions.SetWaffenLaborLocked(waffenlabor.id, !waffenlabor.isWaffenLaborLocked);
                        return;
                    }

                    if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
                    var fraktionslagerEntry = ServerFactions.ServerFactions_.FirstOrDefault(x => x.fraktionslagerPos.IsInRange(player.Position, 2.5f));
                    if (fraktionslagerEntry != null && !player.IsInVehicle && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == fraktionslagerEntry.id)
                    {
                        if (fraktionslagerEntry.isFraktionslagerLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Fraktionslager aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Fraktionslager abgeschlossen.");
                        ServerFactions.SetFraktionslagerLocked(fraktionslagerEntry.id, !fraktionslagerEntry.isFraktionslagerLocked);
                        return;
                    }

                    if (player.Position.IsInRange(Constants.Positions.fraktionslager_ExitPosition, 2.5f) && player.Dimension != 0 && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == player.Dimension)
                    {
                        Server_Factions fraktionslager = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                        if (fraktionslager == null) return;
                        if (fraktionslager.isFraktionslagerLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Fraktionslager aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Fraktionslager abgeschlossen.");
                        ServerFactions.SetFraktionslagerLocked(fraktionslager.id, !fraktionslager.isFraktionslagerLocked);
                        return;
                    }

                    if (player.Position.IsInRange(Constants.Positions.fraktionslager_ExitPosition, 2.5f) && player.Dimension != 0 && ServerFactions.IsCharacterInAnyFaction(User.GetPlayerOnline(player)) && ServerFactions.GetCharacterFactionId(User.GetPlayerOnline(player)) == player.Dimension)
                    {
                        Server_Factions fraktionslager = ServerFactions.ServerFactions_.ToList().FirstOrDefault(x => x.id == player.Dimension);
                        if (fraktionslager == null) return;
                        if (fraktionslager.isFraktionslagerLocked) HUDHandler.SendNotification(player, 2, 2500, "Du hast das Fraktionslager aufgeschlossen.");
                        else HUDHandler.SendNotification(player, 4, 2500, "Du hast das Fraktionslager abgeschlossen.");
                        ServerFactions.SetFraktionslagerLocked(fraktionslager.id, !fraktionslager.isFraktionslagerLocked);
                        return;
                    }
                    Server_Storages storage = ServerStorages.ServerStorages_.FirstOrDefault(x => player.Position.IsInRange(x.entryPos, 2f) && (x.owner == User.GetPlayerOnline(player) || x.secondOwner == User.GetPlayerOnline(player) || x.factionid == ServerFactions.GetCharacterFactionId(charId)));
                    if (storage != null && !player.IsInVehicle && player.Dimension == 0)
                    {
                        storage.isLocked = !storage.isLocked;
                        if (storage.isLocked) HUDHandler.SendNotification(player, 4, 2500, "Du hast die Lagerhalle abgeschlossen.");
                        else HUDHandler.SendNotification(player, 2, 2500, "Du hast die Lagerhalle aufgeschlossen.");
                        return;
                    }

                    if (player.Position.IsInRange(Constants.Positions.storage_ExitPosition, 2f) && player.Dimension != 0 && (ServerStorages.GetOwner(player.Dimension) == User.GetPlayerOnline(player) || ServerStorages.GetSecondOwner(player.Dimension) == User.GetPlayerOnline(player)))
                    {
                        Server_Storages storages = ServerStorages.ServerStorages_.FirstOrDefault(x => x.id == player.Dimension);
                        if (storages == null) return;
                        storages.isLocked = !storages.isLocked;
                        if (storages.isLocked) HUDHandler.SendNotification(player, 4, 2500, "Du hast die Lagerhalle abgeschlossen.");
                        else HUDHandler.SendNotification(player, 2, 2500, "Du hast die Lagerhalle aufgeschlossen.");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:KeyHandler:PressComma")]
        public async Task PressComma(IPlayer player)
        {
            try
            {
                if (player == null && !player.Exists && player.GetCharacterMetaId() == 0 && Characters.GetCharacterAccountId((int)player.GetCharacterMetaId()) == 0 && player.IsInVehicle == true) return;
                if (player.HasData("usingItem")) return;
                if (!CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Verbandskasten") && !CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Verband")) return;
                if (player.IsInVehicle)
                {
                    HUDHandler.SendNotification(player, 2, 5000, "Nicht im Auto"); return;
                }
                if (CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Verbandskasten"))
                {
                    HUDHandler.SendNotification(player, 2, 5000, "Du hast einen Verbandskasten benutzt. <br><br> -1 Verbandskasten");
                    InventoryHandler.InventoryAnimation(player, "verband", 5000);

                   
                    player.SetPlayerIsCuffed("handcuffs", true);
                    player.SetData("usingItem", true);
                    await Task.Delay(5000);
                    player.DeleteData("usingItem");
                    Characters.SetCharacterHealth((int)player.GetCharacterMetaId(), 200);
                    player.Health = 200;
                    player.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterArmor((int)player.GetCharacterMetaId()), Characters.GetCharacterHealth((int)player.GetCharacterMetaId()), Characters.GetCharacterHunger((int)player.GetCharacterMetaId()), Characters.GetCharacterThirst((int)player.GetCharacterMetaId())); //HUD updaten
                    CharactersInventory.RemoveCharacterItemAmount2((int)player.GetCharacterMetaId(), "Verbandskasten", 1);
                    await Task.Delay(5000);
                    player.SetPlayerIsCuffed("handcuffs", false);
                }
                else if (CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Verband"))
                {
                    HUDHandler.SendNotification(player, 2, 5000, "Du hast einen Verband benutzt. <br><br> -1 Verband");
                    InventoryHandler.InventoryAnimation(player, "verband", 5000);
                    player.SetData("usingItem", true);
                    await Task.Delay(5000);
                    player.DeleteData("usingItem");
                    if (player.Health >= 175) player.Health = 200;
                    else player.Health += 25;
                    Characters.SetCharacterHealth((int)player.GetCharacterMetaId(), player.Health);
                    player.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterArmor((int)player.GetCharacterMetaId()), Characters.GetCharacterHealth((int)player.GetCharacterMetaId()), Characters.GetCharacterHunger((int)player.GetCharacterMetaId()), Characters.GetCharacterThirst((int)player.GetCharacterMetaId())); //HUD updaten
                    CharactersInventory.RemoveCharacterItemAmount2((int)player.GetCharacterMetaId(), "Verband", 1);
                }

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [AsyncClientEvent("Server:KeyHandler:PressPeriod")]
        public async Task PressPeriod(IPlayer player)
        {
            try
            {
                if (player == null && !player.Exists && player.GetCharacterMetaId() == 0 && Characters.GetCharacterAccountId((int)player.GetCharacterMetaId()) == 0 && player.IsInVehicle == true) return;
                if (player.HasData("usingItem")) return;
                if (CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Schutzweste"))
                {
                    
                    player.SetPlayerIsCuffed("handcuffs", true);
                    HUDHandler.SendNotification(player, 2, 6000, "Du hast ein Schutzweste benutzt. <br><br> -1 Schutzweste");
                    InventoryHandler.InventoryAnimation(player, "weste", 6000);
                    player.SetData("usingItem", true);
                    await Task.Delay(6000);
                    player.DeleteData("usingItem");
                    Characters.SetCharacterArmor((int)player.GetCharacterMetaId(), 100);
                    player.Armor = 100;
                    player.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterArmor((int)player.GetCharacterMetaId()), Characters.GetCharacterHealth((int)player.GetCharacterMetaId()), Characters.GetCharacterHunger((int)player.GetCharacterMetaId()), Characters.GetCharacterThirst((int)player.GetCharacterMetaId())); //HUD updaten
                    CharactersInventory.RemoveCharacterItemAmount2((int)player.GetCharacterMetaId(), "Schutzweste", 1);
                    await Task.Delay(5000);
                    player.SetPlayerIsCuffed("handcuffs", false);
                }
                else if (CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Beamtenschutzweste"))
                {
                    HUDHandler.SendNotification(player, 2, 6000, "Du hast eine Beamtenschutzweste benutzt. <br><br> -1 Beamtenschutzweste");
                    InventoryHandler.InventoryAnimation(player, "weste", 6000);
                    player.SetData("usingItem", true);
                    await Task.Delay(6000);
                    player.DeleteData("usingItem");
                    Characters.SetCharacterArmor((int)player.GetCharacterMetaId(), 100);
                    player.Armor = 100;
                    if (Characters.GetCharacterGender((int)player.GetCharacterMetaId())) player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 17, 2);
                    else player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 57, 0); // Schutzweste
                    CharactersInventory.RemoveCharacterItemAmount2((int)player.GetCharacterMetaId(), "Beamtenschutzweste", 1);
                }
                else if (CharactersInventory.ExistCharacterItem2((int)player.GetCharacterMetaId(), "Agentschutzweste"))
                {
                    HUDHandler.SendNotification(player, 2, 6000, "Du hast eine Agentenschutzweste benutzt. <br><br> -1 Agentenschutzweste");
                    InventoryHandler.InventoryAnimation(player, "weste", 6000);
                    player.SetData("usingItem", true);
                    await Task.Delay(6000);
                    player.DeleteData("usingItem");
                    Characters.SetCharacterArmor((int)player.GetCharacterMetaId(), 100);
                    player.Armor = 100;
                    if (Characters.GetCharacterGender((int)player.GetCharacterMetaId())) player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 17, 2);
                    else player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 62, 0); // Schutzweste
                    CharactersInventory.RemoveCharacterItemAmount2((int)player.GetCharacterMetaId(), "Agentschutzweste", 1);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [ClientEvent("Server:Farm:AutoCollect")]
        public Task AutoCollect(ClassicPlayer player)
        {
            if (player == null) return Task.CompletedTask;
            UseFarmSpot(player);
            return Task.CompletedTask;
        }
    }
}

