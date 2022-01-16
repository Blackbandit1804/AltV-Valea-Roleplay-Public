using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Altv_Roleplay.Database;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Altv_Roleplay.Handler
{
    public class Commands : IScript
    {
        [Command("forceduty")]
        public void forceDuty(ClassicPlayer player)
        {
            if (player == null || !player.Exists || !player.isSpawned || player.AdminLevel() < 9 || !ServerFactions.IsCharacterInAnyFaction((int)player.GetCharacterMetaId())) return;
            ServerFactions.SetCharacterInFactionDuty((int)player.GetCharacterMetaId(), true);
        }

        [Command("char")]
        public static void CMD_char(IPlayer player)
        {
            if (player == null) { HUDHandler.SendNotification(player, 4, 3000, "Fehler 002 >> player == null // Spieler nicht gefunden"); HUDHandler.SendNotification(player, 4, 3000, "Fehler 002 >> Sollte der Fehler bleiben, bitte melde das einem Admin!"); return; }
            player.EmitLocked("Client:Charcreator:CreateCEF");
            player.Position = new Position((float)402.778, (float)-996.9758, (float)-98);
            player.Rotation = new Rotation(0, 0, (float)3.1168559);
        }



        [Command("setveh")]
        public void setVeh(ClassicPlayer player, string name, string plate)
        {
            try
            {

                if (player == null || !player.Exists || !player.isSpawned || player.AdminLevel() < 9) return;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                if (ServerVehicles.ExistServerVehiclePlate(plate)) { HUDHandler.SendNotification(player, 4, 5000, $"Dieses Kennzeichen existiert bereits."); return; }

                ulong fHash = Alt.Hash(name);
                int charId = User.GetPlayerOnline(player);
                if (charId == 0 || fHash == 0) return;
                ServerVehicles.CreateVehicle(fHash, charId, 0, 0, false, 0, player.Position, player.Rotation, $"{plate}", 0, 0, 0);
                CharactersInventory.AddCharacterItem(charId, $"Fahrzeugschluessel {plate}", 2, "schluessel");
                stopwatch.Stop();
                if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charId} - BuyVehicle benötigte {stopwatch.Elapsed.Milliseconds}ms");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }
        [CommandEvent(CommandEventType.CommandNotFound)]
        public void OnCommandNotFound(Player player, string cmd)
        {
            HUDHandler.SendNotification(player, 2, 5000, " command nicht gefunden ");

        }
        [Command("jailtime")]
        public void jailTimeCmd(ClassicPlayer player)
        {
            if (player == null || !player.Exists || !player.isSpawned || !Characters.IsCharacterInJail((int)player.GetCharacterMetaId())) return;
            int jailtime = Characters.GetCharacterJailTime((int)player.GetCharacterMetaId());
            if (jailtime <= 0) return;
            HUDHandler.SendNotification(player, 4, 5000, $"Du hast noch {jailtime} Hafteinheiten abzusitzen");
        }

        [Command("vehpos")]
        public void vehPos(IPlayer player)
        {
            if (player == null || !player.Exists || !player.IsInVehicle) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            HUDHandler.SendNotification(player, 4, 60000, $"{player.Vehicle.Position.ToString()}");
        }

        [Command("testveh")]
        public void CMD_testtest(IPlayer player)
        {
            if (player == null || !player.Exists || !player.IsInVehicle) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            VehicleHandler.testtesttest(player);
        }

        //Erstelle Lagerhalle
        [Command("createstorage")]
        public void CMD(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            Server_Storages st = new Server_Storages
            {
                entryPos = player.Position,
                items = new List<Server_Storage_Item>(),
                owner = 0,
                secondOwner = 0,
                maxSize = 200f,
                price = 25000
            };

            ServerStorages.ServerStorages_.Add(st);

            using (gtaContext gta = new gtaContext())
            {
                gta.Server_Storages.Add(st);
                gta.SaveChanges();
            }
            EntityStreamer.BlipStreamer.CreateStaticBlip("Lagerhalle", 0, 0.5f, true, 50, player.Position, 0);
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 1), new Vector3(1), color: new Rgba(180, 50, 50, 100), dimension: 0, streamRange: 100);
            HUDHandler.SendNotification(player, 4, 5000, "Erstellt");
        }

        [Command("Stabi")]
        public void CMD_Stabi(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 5) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            player.SetData("Stabi", true);
        }

        [Command("vscp")] // setzt vehicle price
        public void vehPrice(IPlayer player, string car, int price)
        {
            try
            {
                if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (car == null) { HUDHandler.SendNotification(player, 4, 5000, "Fahrzeugname nicht angegeben!"); return; }
                IVehicle testVehicle = Alt.CreateVehicle(car, player.Position, player.Rotation);

                if (testVehicle == null)
                {
                    HUDHandler.SendNotification(player, 2, 5000, $"Falsches Fahrzeug.");
                }
                else
                {
                    testVehicle.Remove();
                    uint hashedCar = Alt.Hash(car);
                    HUDHandler.SendNotification(player, 2, 20000, $"Fahrzeugname: {car} || Fahrzeughash: {hashedCar} || Neuer Preis: {price}$");
                    ServerVehicleShops.SetVehiclePrice(hashedCar, price);
                }

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        //OoC Messages
        [Command("ooc", true)]
        public void oocCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (msg == null) { HUDHandler.SendNotification(player, 4, 4000, "Wie wäre es mit einer Nachricht? (Fehlt)"); return; }
            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            DiscordLog.SendEmbed("ooc", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] Nachricht: {msg}");

            foreach (var client in Alt.GetAllPlayers())
            {
                var name = Characters.GetCharacterName((int)player.GetCharacterMetaId());
                if (client == null || !client.Exists) continue;
                var range = 5; //Change OOC Range!!
                if (client.Position.Distance(player.Position) <= range)
                {
                    HUDHandler.SendNotification((IPlayer)client, 2, 5000, $"[{(int)player.GetCharacterMetaId()}] {name}: \n " + msg);
                    /*                    HelperMethods.sendDiscordLog("OOC", $"{player.Name} Nachricht: {msg}", "red");
                    */
                }
            }
        }

        [Command("LSPD", true)]
        public void LSPDCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (!ServerFactions.IsCharacterInAnyFaction((int)player.GetCharacterMetaId())) { HUDHandler.SendNotification(player, 4, 4000, "Du bist in keiner Fraktion"); return; }
            if (ServerFactions.GetCharacterFactionId((int)player.GetCharacterMetaId()) != 1) { HUDHandler.SendNotification(player, 4, 4000, "Du gehörst nicht zum LSPD"); return; }

            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            foreach (var client in Alt.GetAllPlayers())
            {
                if (client == null || !client.Exists) continue;
                HUDHandler.SendNotification((IPlayer)client, 4, 10000, "Das L.S.P.D informiert: \n" + msg);
            }
            /*            HelperMethods.sendDiscordLog("LSPD USERMAIL", $"{player.accountName} hast eine LSPD Rundmail geschickt: {msg}", "red");
            */
        }

        [Command("FIB", true)]
        public void FIBCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (!ServerFactions.IsCharacterInAnyFaction((int)player.GetCharacterMetaId())) { HUDHandler.SendNotification(player, 4, 4000, "Du bist in keiner Fraktion"); return; }
            if (ServerFactions.GetCharacterFactionId((int)player.GetCharacterMetaId()) != 3) { HUDHandler.SendNotification(player, 4, 4000, "Du gehörst nicht zum FIB"); return; }

            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            foreach (var client in Alt.GetAllPlayers())
            {
                if (client == null || !client.Exists) continue;
                HUDHandler.SendNotification((IPlayer)client, 4, 10000, "Das FIB informiert: \n" + msg);
            }
            /*            HelperMethods.sendDiscordLog("LSPD USERMAIL", $"{player.accountName} hast eine LSPD Rundmail geschickt: {msg}", "red");
            */
        }

        [Command("GOV", true)]
        public void GOVCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (!ServerFactions.IsCharacterInAnyFaction((int)player.GetCharacterMetaId())) { HUDHandler.SendNotification(player, 4, 4000, "Du bist in keiner Fraktion"); return; }
            if (ServerFactions.GetCharacterFactionId((int)player.GetCharacterMetaId()) != 8) { HUDHandler.SendNotification(player, 4, 4000, "Du gehörst nicht zum FIB"); return; }

            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            foreach (var client in Alt.GetAllPlayers())
            {
                if (client == null || !client.Exists) continue;
                HUDHandler.SendNotification((IPlayer)client, 4, 10000, "Das GOV informiert: \n" + msg);
            }
            /*            HelperMethods.sendDiscordLog("LSPD USERMAIL", $"{player.accountName} hast eine LSPD Rundmail geschickt: {msg}", "red");
            */
        }

        [Command("report", true)]
        public void REPORTCMD(IPlayer player, string msg)
        {
            if (player == null || !player.Exists) return;
            if (msg.Contains('<') || msg.Contains('>'))
            {
                HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                return;
            }

            List<string> playerList = new List<string>();
            foreach (var client in Alt.GetAllPlayers().Cast<ClassicPlayer>())
            {
                var name = Characters.GetCharacterName((int)client.GetCharacterMetaId());
                float dist = client.Position.Distance(player.Position);
                if (dist <= 100.0)
                {
                    playerList.Add($"{name}({(int)client.GetCharacterMetaId()}) - {Math.Round((decimal)dist, 2)}m\n");
                }
            }

            string final = $"[REPORT] {Characters.GetCharacterName((int)player.GetCharacterMetaId())} (ID: {(int)player.GetCharacterMetaId()}) benötigt Support:\n{msg}\nSpieler in der Nähe: \n";

            foreach (var p in playerList)
            {
                final += p;
            }

            DiscordLog.SendEmbed("report", "Report", final);
            foreach (var client in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.AdminLevel() >= 1))
            {
                if (client == null || !client.Exists) continue;
                HUDHandler.SendNotification((IPlayer)client, 4, 10000, final);
            }
            /*            HelperMethods.sendDiscordLog("LSPD USERMAIL", $"{player.accountName} hast eine LSPD Rundmail geschickt: {msg}", "red");
            */
        }

        [Command("id")]
        public static void CMD_ID(IPlayer player)
        {
            if (player == null) { HUDHandler.SendNotification(player, 4, 3000, "Fehler 002 >> player == null // Spieler nicht gefunden"); HUDHandler.SendNotification(player, 4, 3000, "Fehler 002 >> Sollte der Fehler bleiben, bitte melde das einem Admin!"); return; }
            HUDHandler.SendNotification(player, 2, 10000, $"Deine ID = {player.Id}");
        }

        [Command("license")]
        public void licenseCMD(IPlayer player, int charId, string licenseshort)
        {
            if (licenseshort == null || !player.Exists) return;
            if (player.AdminLevel() < 8) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            if (charId <= 0) return;
            Characters.AddCharacterPermission(charId, licenseshort);
            HUDHandler.SendNotification(player, 2, 3500, $"Du hast die License {licenseshort} vergeben an: {charId}");
        }

        [Command("setAtm")]
        public void SetATM_CMD(IPlayer player, string name)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            ServerATM.CreateNewATM(player, 0, player.Position, name);
            HUDHandler.SendNotification(player, 2, 2500, $"ATM erfolgreich gesetzt: {name}");
        }


        [Command("money")]
        public void GiveItemCMD(IPlayer player, int itemAmount)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            CharactersInventory.AddCharacterItem((int)charId, "Bargeld", itemAmount, "inventory");
            HUDHandler.SendNotification(player, 2, 5000, $"{itemAmount}$ erhalten (Bargeld).");
        }

        [Command("players")]
        public void PlayerCMD(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (player.AdminLevel() < 2) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                string msg = "Liste aller Spieler:<br>";
                foreach (var p in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.GetCharacterMetaId() > 0))
                {
                    msg += $"{Characters.GetCharacterName((int)p.GetCharacterMetaId())} ({p.GetCharacterMetaId()})<br>";
                }
                HUDHandler.SendNotification(player, 1, 8000, msg);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("quitffa")]
        public static void CMD_QFFA(IPlayer player)
        {
            if (player.HasData("FFA"))
            {
                int charId = (int)player.GetCharacterMetaId();
                if (charId <= 0) return;
                HUDHandler.SendNotification(player, 4, 5000, "ffa verlassen");
                player.Dimension = 0;
                Characters.SetCharacterWeapon(player, "PrimaryWeapon", "None");
                Characters.SetCharacterWeapon(player, "PrimaryAmmo", 0);
                Characters.SetCharacterWeapon(player, "SecondaryWeapon2", "None");
                Characters.SetCharacterWeapon(player, "SecondaryWeapon", "None");
                Characters.SetCharacterWeapon(player, "SecondaryAmmo2", 0);
                Characters.SetCharacterWeapon(player, "SecondaryAmmo", 0);
                Characters.SetCharacterWeapon(player, "FistWeapon", "None");
                Characters.SetCharacterWeapon(player, "FistWeaponAmmo", 0);
                Characters.SetCharacterPhoneEquipped(charId, false);
                player.RemoveAllWeaponsAsync();
                player.Spawn(new Position(758.3077f, -816.26373f, 26.499634f), 0);
                player.Position = new Position(758.3077f, -816.26373f, 26.499634f);
                player.DeleteData("FFA");
                return;
            }
            else
            {
                HUDHandler.SendNotification(player, 4, 5000, "Du bist nicht in ffa");
                return;
            }
        }

        [Command("ReloadDB")]
        public static void CMD_RELOAD(IPlayer player, int ID)
        {
            if (player == null || !player.Exists) return;
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }

            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            if (ID <= 0 || ID >= 17) { HUDHandler.SendNotification(player, 4, 5000, "Benutze /ReloadDB <id> 1 - 16"); return; } //Diff -1 |{value}| +1

            switch (ID)
            {
                case 1: // Accounts - Funktioniert Nicht korrekt!
                    DatabaseHandler.LoadAllPlayers();
                    HUDHandler.SendNotification(player, 2, 5000, "Accounts neugeladen!");
                    Alt.Log("ReloadDB 1 Called");
                    break;
                case 2: // Characters - Funktioniert Nicht korrekt!
                    DatabaseHandler.LoadAllPlayerCharacters();
                    HUDHandler.SendNotification(player, 2, 5000, "Characters neugeladen!");
                    Alt.Log("ReloadDB 2 Called");
                    break;
                case 3: // Garagen
                    DatabaseHandler.LoadAllGarages();
                    DatabaseHandler.LoadAllGarageSlots();
                    HUDHandler.SendNotification(player, 2, 5000, "Garagen neugeladen");
                    Alt.Log("ReloadDB 3 Called");
                    break;
                case 4: // Vehicle Shop
                    DatabaseHandler.LoadAllVehicleShops();
                    DatabaseHandler.LoadAllVehicleShopItems();
                    HUDHandler.SendNotification(player, 2, 5000, "Fahrzeugshops neugeladen!");
                    Alt.Log("ReloadDB 4 Called");
                    break;
                case 5: // Shops - Dealer
                    DatabaseHandler.LoadAllServerShops();
                    DatabaseHandler.LoadAllServerShopItems();
                    HUDHandler.SendNotification(player, 2, 5000, "Dealer & Shops Neugeladen!");
                    Alt.Log("ReloadDB 5 Called");
                    break;
                case 6: // Klamotten Shops
                    DatabaseHandler.LoadAllClothesShops();
                    HUDHandler.SendNotification(player, 2, 5000, "Kleidungsshops neugeladen");
                    Alt.Log("ReloadDB 6 Called");
                    break;
                case 7: // Tankstellen
                    DatabaseHandler.LoadAllServerFuelStations();
                    DatabaseHandler.LoadALlServerFuelStationSpots();
                    HUDHandler.SendNotification(player, 2, 5000, "Tankstellen neugeladen!");
                    Alt.Log("ReloadDB 7 Called");
                    break;
                case 8: // Teleporter
                    DatabaseHandler.LoadAllServerTeleports();
                    HUDHandler.SendNotification(player, 2, 5000, "Teleporter Neugeladen!");
                    Alt.Log("ReloadDB 8 Called");
                    break;
                case 9: // Companys
                    DatabaseHandler.LoadAllServerCompanys();
                    DatabaseHandler.LoadAllServerCompanyMember();
                    HUDHandler.SendNotification(player, 2, 5000, "Unternehmen Neugeladen!");
                    Alt.Log("ReloadDB 9 Called");
                    break;
                case 10: // Faction
                    DatabaseHandler.LoadAllServerFactions();
                    DatabaseHandler.LoadAllServerFactionRanks();
                    DatabaseHandler.LoadAllServerFactionMembers();
                    HUDHandler.SendNotification(player, 2, 5000, "Factions Neugeladen!");
                    Alt.Log("ReloadDB 10 Called");
                    break;
                case 11: // Türen
                    DatabaseHandler.LoadAllServerDoors();
                    HUDHandler.SendNotification(player, 2, 5000, "Alle Türen wurden Neugeladen!");
                    Alt.Log("ReloadDB 11 Called");
                    break;
                case 12: // Lizenzen
                    DatabaseHandler.LoadAllCharacterLicenses();
                    HUDHandler.SendNotification(player, 2, 5000, "Lizenzen Neu Geladen!");
                    Alt.Log("ReloadDB 12 Called");
                    break;
                case 13: // Farming 
                    DatabaseHandler.LoadAllServerFarmingProducers();
                    DatabaseHandler.LoadAllServerFarmingSpots();
                    HUDHandler.SendNotification(player, 2, 5000, "Farming Neugeladen!");
                    Alt.Log("ReloadDB 13 Called");
                    break;
                case 14: // Bankautomaten
                    DatabaseHandler.LoadAllServerATMs();
                    HUDHandler.SendNotification(player, 2, 5000, "ATM's neu geladen!");
                    Alt.Log("ReloadDB 14 Called");
                    break;
                case 15: // Häuser Neuladen
                    DatabaseHandler.LoadAllServerHouses();
                    HUDHandler.SendNotification(player, 2, 5000, "Häuser neu geladen!");
                    Alt.Log("ReloadDB 15 Called");
                    break;
                case 16: // Lagerhallen
                    DatabaseHandler.LoadAllServerStorages();
                    HUDHandler.SendNotification(player, 2, 5000, "Lagerhallen neu geladen!");
                    break;
            }
        }

        [Command("setBank")]
        public void CMD_SETBANK(IPlayer player, string zoneName)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            ServerBank.CreateNewBank(player, 0, player.Position, zoneName);
            HUDHandler.SendNotification(player, 2, 3500, $"Du hast die Bank {zoneName} erfolgreich erstellt!");
        }

        [Command("getaccountidbymail")]
        public static void CMD_getAccountIdByMail(ClassicPlayer player, string mail)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || player.AdminLevel() < 2) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                var accEntry = User.Player.ToList().FirstOrDefault(x => x.Email == mail);
                if (accEntry == null) return;
                HUDHandler.SendNotification(player, 2, 5000, $"Spieler-ID der E-Mail {mail} lautet: {accEntry.playerid} - Spielername: {accEntry.playerName}");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("trace")]
        public static void trace_CMD(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() < 9) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                AltTrace.Start("FabiansDebugKasten");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("stoptrace")]
        public static void stopTrace_CMD(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() < 9) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                AltTrace.Stop();
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("kick", true)]
        public static void cmd_Kick(IPlayer player, int accId, string reason)
        {
            try
            {
                if (player == null || !player.Exists || accId <= 0 || player.AdminLevel() < 3) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerAccountId(x) == accId);
                if (targetP == null) { HUDHandler.SendNotification(player, 4, 5000, $"Dieser Spieler ist nicht online"); return; }
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client == null || !client.Exists) continue;
                    HUDHandler.SendNotification(client, 4, 10000, $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gekickt. (Grund: {reason})");
                }
                DiscordLog.SendEmbed("kick", "Kick-Log", $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gekickt. (Grund: {reason})");
                if (targetP != null) targetP.Kick($"Du wurdest gekickt: {reason} (Gekickt von {Characters.GetCharacterName(User.GetPlayerOnline(player))}))");
                HUDHandler.SendNotification(player, 4, 5000, $"Spieler mit ID {accId} Erfolgreich gekickt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("kick")]
        public static void cmd_Kick_NoReason(IPlayer player, int accId)
        {
            try
            {
                if (player == null || !player.Exists || accId <= 0 || player.AdminLevel() < 3) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerAccountId(x) == accId);
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client == null || !client.Exists) continue;
                    HUDHandler.SendNotification(client, 4, 10000, $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gekickt.");
                }
                DiscordLog.SendEmbed("kick", "Kick-Log", $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gekickt.");
                if (targetP != null) targetP.Kick($"Du wurdest gekickt. (Gekickt von {Characters.GetCharacterName(User.GetPlayerOnline(player))}))");
                HUDHandler.SendNotification(player, 4, 5000, $"Spieler mit ID {accId} Erfolgreich gekickt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("ban", true, new string[] { "xcm" })]
        public static void cmd_BAn(IPlayer player, int accId, string reason)
        {
            try
            {
                if (player == null || !player.Exists || accId <= 0 || player.AdminLevel() < 3) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                User.SetPlayerBanned(accId, true, $"{reason} (Gebannt von {Characters.GetCharacterName(User.GetPlayerOnline(player))})");
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerAccountId(x) == accId);
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client == null || !client.Exists) continue;
                    HUDHandler.SendNotification(client, 4, 10000, $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gebannt. (Grund: {reason})");
                }
                DiscordLog.SendEmbed("ban", "Ban-Log", $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gebannt. (Grund: {reason})");
                if (targetP != null) targetP.Kick($"Du wurdest gebannt: {reason} (Gebannt von {Characters.GetCharacterName(User.GetPlayerOnline(player))}))");
                HUDHandler.SendNotification(player, 4, 5000, $"Spieler mit ID {accId} Erfolgreich gebannt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("ban", false, new string[] { "xcm" })]
        public static void cmd_Ban_NoReason(IPlayer player, int accId)
        {
            try
            {
                if (player == null || !player.Exists || accId <= 0 || player.AdminLevel() < 3) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                User.SetPlayerBanned(accId, true, $"Gebannt von {Characters.GetCharacterName(User.GetPlayerOnline(player))}");
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerAccountId(x) == accId);
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client == null || !client.Exists) continue;
                    HUDHandler.SendNotification(client, 4, 10000, $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gebannt.");
                }
                DiscordLog.SendEmbed("ban", "Ban-Log", $"{Characters.GetCharacterName(User.GetPlayerOnline(targetP))}({accId}) wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} gebannt.");
                if (targetP != null) targetP.Kick($"Du wurdest gebannt. (Gebannt von {Characters.GetCharacterName(User.GetPlayerOnline(player))}))");
                HUDHandler.SendNotification(player, 4, 5000, $"Spieler mit ID {accId} Erfolgreich gebannt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("unban")]
        public static void CMD_Unban(ClassicPlayer player, int accId)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || accId <= 0 || player.AdminLevel() < 5) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                User.SetPlayerBanned(accId, false, "");
                HUDHandler.SendNotification(player, 4, 5000, $"Spieler mit ID {accId} Erfolgreich entbannt.");
                DiscordLog.SendEmbed("ban", "Ban-Log", $"Der Spieler mit der ID {accId} wurde von {Characters.GetCharacterName(User.GetPlayerOnline(player))} entbannt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("changeprice")]
        public static void cmd_ChangeP(IPlayer player, int shopId, int itemId, int newPrice)
        {
            try
            {
                if (player == null || !player.Exists || shopId <= 0 || itemId <= 0 || newPrice < 0 || player.AdminLevel() < 9) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                var shopItem = ServerShopsItems.ServerShopsItems_.FirstOrDefault(x => x != null && x.shopId == shopId && x.id == itemId);
                if (shopItem == null) return;
                shopItem.itemPrice = newPrice;
                using (gtaContext db = new gtaContext())
                {
                    db.Server_Shops_Items.Update(shopItem);
                    db.SaveChanges();
                }
                HUDHandler.SendNotification(player, 4, 5000, "Preis geändert.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("announce", true)]
        public void announceCMD(IPlayer player, string msg)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (player.AdminLevel() < 6) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                if (msg.Contains('<') || msg.Contains('>'))
                {
                    HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                    DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                    return;
                }
                foreach (var client in Alt.GetAllPlayers())
                {
                    if (client == null || !client.Exists) continue;
                    HUDHandler.SendNotification(client, 4, 10000, msg);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("support", true)]
        public void reportCMD(IPlayer player, string msg)
        {
            try
            {
                if (player == null || !player.Exists || User.GetPlayerOnline(player) <= 0) return;
                DiscordLog.SendEmbed("report", "Support", $" {Characters.GetCharacterName((int)player.GetCharacterMetaId())}[{(int)player.GetCharacterMetaId()}]  {msg}");
                if (msg.Contains('<') || msg.Contains('>'))
                {
                    HUDHandler.SendNotification(player, 4, 4000, "Ungültige Eingabe! (good try)");
                    DiscordLog.SendEmbed("sus", "OOC-Nachricht", $"{player.Name} [{(int)player.GetCharacterMetaId()}] hat rce probiert: {msg} (blocked)");
                    return;
                }
                foreach (var admin in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.AdminLevel() >= 1))
                {
                    //admin.SendChatMessage($"[SUPPORT] {Characters.GetCharacterName(User.GetPlayerOnline(player))} (ID: {User.GetPlayerOnline(player)}) benötigt Support: {msg}");
                    HUDHandler.SendNotification(admin, 4, 15000, $"[SUPPORT] {Characters.GetCharacterName(User.GetPlayerOnline(player))} (ID: {User.GetPlayerOnline(player)}) benötigt Support: {msg}");
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("changegender")]
        public void cMD(IPlayer player, int gender)
        {
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            switch (gender)
            {
                case 0: player.Model = 1885233650; break;
                case 1: player.Model = 2627665880; break;
            }
        }

        [Command("car")]
        public void heyCMD(IPlayer player, string model)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                if (player.Vehicle != null && player.Vehicle.Exists) player.Vehicle.Remove();
                IVehicle veh = Alt.CreateVehicle(model, new Position(player.Position.X + 2f, player.Position.Y, player.Position.Z), player.Rotation);
                if (veh != null)
                {
                    veh.EngineOn = true;
                    veh.LockState = VehicleLockState.Unlocked;
                    veh.SetNumberplateTextAsync("vRP-Admin");
                    veh.PrimaryColor = 1;
                    veh.SecondaryColor = 1;
                    veh.DashboardColor = 1;
                    veh.WindowTint = 3;
                    veh.WheelColor = 55;
                    veh.PearlColor = 3;
                    player.EmitLocked("Client:HUD:setIntoVehicle", veh); //Setze Spieler in Fahrzeug (First try :D)
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        [Command("carHash")]
        public static void CMD_GetCarHash(IPlayer player, string car)
        {
            try
            {
                if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                if (car == null) { HUDHandler.SendNotification(player, 4, 5000, "Fahrzeugname nicht angegeben!"); return; }
                IVehicle testVehicle = Alt.CreateVehicle(car, player.Position, player.Rotation);

                if (testVehicle == null)
                {
                    HUDHandler.SendNotification(player, 2, 5000, $"Falsches Fahrzeug.");
                }
                else
                {
                    testVehicle.Remove();
                    uint hashedCar = Alt.Hash(car);
                    HUDHandler.SendNotification(player, 2, 20000, $"Fahrzeugname: {car} || Fahrzeughash: {hashedCar}");
                    StreamWriter hashFile;
                    if (!File.Exists("SavedCoords.txt"))
                    {
                        hashFile = new StreamWriter("SavedHashed.txt");
                    }
                    else
                    {
                        hashFile = File.AppendText("SavedHashed.txt");
                    }
                    HUDHandler.SendNotification(player, 4, 8000, "Die SavedHashed.txt datei wurde überarbeitet!");
                    hashFile.WriteLine("| " + car + " | " + "Saved hash: " + hashedCar);
                    hashFile.Close();
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("makeAdmin")]
        public static void CMD_Giveadmin(IPlayer player, int accId, int adminLevel)
        {
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            try
            {
                if (player == null || !player.Exists) return;
                User.SetPlayerAdminLevel(accId, adminLevel);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("tppos")]
        public void TpPosCMD(IPlayer player, float X, float Y, float Z)
        {
            try
            {
                if (player == null || !player.Exists) return;
                if (player.AdminLevel() < 6) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                if (player.IsInVehicle && player.Seat.Equals(1))
                {
                    if (player.Vehicle == null) return;
                    player.Vehicle.Position = new Position(X, Y, Z);
                }
                else
                {
                    player.Position = new Position(X, Y, Z);
                }

            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("dv")]
        public void CMD_DeleteVehicle(IPlayer player, float range = 2)
        {
            if (player.AdminLevel() < 5) { return; };
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            foreach (IVehicle veh in Alt.GetAllVehicles())
            {
                if (veh.Position.Distance(player.Position) <= range)
                {
                    Alt.RemoveVehicle(veh);
                }
            }
        }

        [Command("pv")]
        public void CMD_ParkVehicle(IPlayer player, float range = 2)
        {
            if (player.AdminLevel() < 5) { return; };
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            foreach (IVehicle veh in Alt.GetAllVehicles())
            {
                if (veh.Position.Distance(player.Position) <= range)
                {
                    ServerVehicles.SetVehicleInGarage(veh, true, ServerVehicles.GetVehicleGarageId(veh));
                }
            }
        }

        [Command("fuel")]
        public void CMD_FuelVehicle(IPlayer player, float range = 2, float fuelValue = 70)
        {
            if (player.AdminLevel() < 5) { return; };
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            foreach (IVehicle veh in Alt.GetAllVehicles())
            {
                if (veh.Position.Distance(player.Position) <= range)
                {
                    ServerVehicles.SetVehicleFuel(veh, fuelValue);
                }
            }
        }

        [Command("parkvehicle")]
        public static void CMD_parkVehicleById(IPlayer player, int vehId)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() <= 8 || vehId <= 0) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                var vehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x != null && x.Exists && x.HasVehicleId() && (int)x.GetVehicleId() == vehId);
                if (vehicle == null) return;
                ServerVehicles.SetVehicleInGarage(vehicle, true, 25);
                HUDHandler.SendNotification(player, 4, 5000, $"Fahrzeug {vehId} in Garage 1(Pillbox) eingeparkt");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("parkallvehicles")]
        public static void CMD_ParkALlVehs(IPlayer player)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() < 6) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                int count = 0;
                foreach (var veh in Alt.GetAllVehicles().ToList().Where(x => x != null && x.Exists && x.HasVehicleId()))
                {
                    if (veh == null || !veh.Exists || !veh.HasVehicleId()) continue;
                    int currentGarageId = ServerVehicles.GetVehicleGarageId(veh);
                    if (currentGarageId <= 0) continue;
                    ServerVehicles.SetVehicleInGarage(veh, true, currentGarageId);
                    count++;
                }

                HUDHandler.SendNotification(player, 4, 5000, $"{count} Fahrzeuge eingeparkt.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("parkvehicle", true)]
        public static void CMD_parkVehicle(IPlayer player, string plate)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() < 2 || string.IsNullOrWhiteSpace(plate)) return;
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                var vehicle = Alt.GetAllVehicles().ToList().FirstOrDefault(x => x != null && x.Exists && x.HasVehicleId() && (int)x.GetVehicleId() > 0 && x.NumberplateText.ToLower() == plate.ToLower());
                if (vehicle == null) return;
                ServerVehicles.SetVehicleInGarage(vehicle, true, 20);
                HUDHandler.SendNotification(player, 4, 5000, $"Fahrzeug mit dem Kennzeichen {plate} in Garage 20 eingeparkt");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("whitelist")]
        public void WhitelistCMD(IPlayer player, int targetAccId)
        {
            try
            {
                if (player == null || !player.Exists || targetAccId <= 0 || player.GetCharacterMetaId() <= 0) return;
                if (player.AdminLevel() <= 0) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (!User.ExistPlayerById(targetAccId)) { HUDHandler.SendNotification(player, 4, 5000, $"Diese ID existiert nicht {targetAccId}"); return; }
                if (User.IsPlayerWhitelisted(targetAccId)) { HUDHandler.SendNotification(player, 4, 5000, "Der Spieler ist bereits gewhitelisted."); return; }
                User.SetPlayerWhitelistState(targetAccId, true);
                HUDHandler.SendNotification(player, 4, 5000, $"Du hast den Spieler {targetAccId} gewhitelistet.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("revive")]
        public void ReviveTargetCMD(IPlayer player, int targetId)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 5) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            string charName = Characters.GetCharacterName(targetId);
            if (!Characters.ExistCharacterName(charName)) return;
            var tp = Alt.GetAllPlayers().FirstOrDefault(x => x != null && x.Exists && x.GetCharacterMetaId() == (ulong)targetId);
            if (tp != null)
            {
                tp.Health = 200;
                DeathHandler.revive(tp);
                Alt.Emit("SaltyChat:SetPlayerAlive", tp, true);
                HUDHandler.SendNotification(player, 4, 5000, $"Du hast den Spieler {charName} wiederbelebt.");
                return;
            }
            HUDHandler.SendNotification(player, 4, 5000, $"Der Spieler {charName} ist nicht online.");
        }

        [Command("faction")]
        public void FactionCMD(IPlayer player, int charId, int id)
        {
            try
            {
                if (player == null || !player.Exists || player.GetCharacterMetaId() <= 0) return;
                if (player.AdminLevel() < 6) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
                if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
                if (ServerFactions.IsCharacterInAnyFaction(charId))
                {
                    ServerFactions.RemoveServerFactionMember(ServerFactions.GetCharacterFactionId(charId), charId);
                }

                ServerFactions.CreateServerFactionMember(id, charId, ServerFactions.GetFactionMaxRankCount(id), charId);
                HUDHandler.SendNotification(player, 4, 5000, $"Done.");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("giveitem")]
        public void GiveItemCMD(IPlayer player, string itemName, int itemAmount)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 9) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            itemName = itemName.Replace("_", " ");
            if (!ServerItems.ExistItem(ServerItems.ReturnNormalItemName(itemName))) { HUDHandler.SendNotification(player, 4, 5000, $"Itemname nicht gefunden: {itemName}"); return; }
            ulong charId = player.GetCharacterMetaId();
            if (charId <= 0) return;
            CharactersInventory.AddCharacterItem((int)charId, itemName, itemAmount, "inventory");
            HUDHandler.SendNotification(player, 2, 5000, $"Gegenstand '{itemName}' ({itemAmount}x) erhalten.");
        }

        [Command("tp", false)]
        public void GotoCMD(IPlayer player, int targetId)
        {
            if (player.AdminLevel() < 2) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            try
            {
                if (player == null || !player.Exists) return;
                if (targetId <= 0 || targetId.ToString().Length <= 0)
                {
                    HUDHandler.SendNotification(player, 4, 5000, "Benutzung: /tp charId");
                    return;
                }
                string targetCharName = Characters.GetCharacterName(targetId);
                if (targetCharName.Length <= 0)
                {
                    HUDHandler.SendNotification(player, 3, 5000, $"Warnung: Die angegebene Character-ID wurde nicht gefunden ({targetId}).");
                    return;
                }
                if (!Characters.ExistCharacterName(targetCharName))
                {
                    HUDHandler.SendNotification(player, 3, 5000, $"Warnung: Der angegebene Charaktername wurde nicht gefunden ({targetCharName} - ID: {targetId}).");
                    return;
                }
                var targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x != null && x.Exists && x.GetCharacterMetaId() == (ulong)targetId);
                if (targetPlayer == null || !targetPlayer.Exists) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Spieler ist nicht online."); return; }
                HUDHandler.SendNotification(targetPlayer, 1, 5000, $"{Characters.GetCharacterName((int)player.GetCharacterMetaId())} hat sich zu dir teleportiert.");
                HUDHandler.SendNotification(player, 2, 5000, $"Du hast dich zu dem Spieler {Characters.GetCharacterName((int)targetPlayer.GetCharacterMetaId())} teleportiert.");
                if (player.IsInVehicle && player.Seat.Equals(1))
                {
                    if (player.Vehicle == null) return;
                    player.Vehicle.Position = targetPlayer.Position + new Position(0, 0, 1);
                }
                else
                {
                    player.Position = targetPlayer.Position + new Position(0, 0, 1);
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("bring", false)]
        public void GetHereCMD(IPlayer player, int targetId)
        {
            if (player.AdminLevel() < 2) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            try
            {
                if (player == null || !player.Exists) return;
                if (targetId <= 0 || targetId.ToString().Length <= 0)
                {
                    HUDHandler.SendNotification(player, 4, 5000, "Benutzung: /bring charId");
                    return;
                }
                string targetCharName = Characters.GetCharacterName(targetId);
                if (targetCharName.Length <= 0)
                {
                    HUDHandler.SendNotification(player, 3, 5000, $"Warnung: Die angegebene Character-ID wurde nicht gefunden ({targetId}).");
                    return;
                }
                if (!Characters.ExistCharacterName(targetCharName))
                {
                    HUDHandler.SendNotification(player, 3, 5000, $"Warnung: Der angegebene Charaktername wurde nicht gefunden ({targetCharName} - ID: {targetId}).");
                    return;
                }
                var targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x != null && x.Exists && x.GetCharacterMetaId() == (ulong)targetId);
                if (targetPlayer == null || !targetPlayer.Exists) { HUDHandler.SendNotification(player, 4, 5000, "Fehler: Spieler ist nicht online."); return; }
                HUDHandler.SendNotification(targetPlayer, 1, 5000, $"{Characters.GetCharacterName((int)player.GetCharacterMetaId())} hat dich zu Ihm teleportiert.");
                HUDHandler.SendNotification(player, 2, 5000, $"Du hast den Spieler {Characters.GetCharacterName((int)targetPlayer.GetCharacterMetaId())} zu dir teleportiert.");
                targetPlayer.Position = player.Position + new Position(0, 0, 1);
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        [Command("pos")]
        public static void PosCMD(IPlayer player, string coordName)
        {
            if (player.AdminLevel() < 2)
            {
                HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte.");
                return;
            }
            if (!player.HasData("isAduty")) { HUDHandler.SendNotification(player, 4, 5000, "Nicht im (/am) Admindienst."); return; }
            if (coordName == null)
            {
                HUDHandler.SendNotification(player, 4, 5000, "Kein Namen angegeben!");
                return;
            }

            Position playerPosGet = player.Position;
            Rotation playerRotGet = player.Rotation;

            HUDHandler.SendNotification(player, 4, 8000, $"{coordName}: {playerPosGet.ToString()} - {playerRotGet.ToString()}");
            StreamWriter coordsFile;
            if (!File.Exists("SavedCoords.txt"))
            {
                coordsFile = new StreamWriter("SavedCoords.txt");
            }
            else
            {
                coordsFile = File.AppendText("SavedCoords.txt");
            }
            HUDHandler.SendNotification(player, 4, 8000, "Die SavedCoords.txt datei wurde überarbeitet!");
            coordsFile.WriteLine("| " + coordName + " | " + "Saved Coordenates: " + playerPosGet.ToString() + " Saved Rotation: " + playerRotGet.ToString());
            coordsFile.Close();
        }

        [Command("am", true)]
        public void AdutyCMD(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 2) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }

            if (player.HasData("isAduty"))
            {
                player.DeleteData("isAduty");
                _ = Characters.SetCharacterCorrectClothes(player);
                HUDHandler.SendNotification(player, 4, 5000, $"Du bist nun nicht mehr als Admin unterwegs. (F9 Deaktiviert)");
            }
            else
            {
                player.SetData("isAduty", true);

                int componentColor = 0;

                /*
                0 grau
                1 dunkelgrau-blau
                2 rot
                3 gelb
                4 dunkelgrau-gelb
                5 gold
                6 dunkelgrau-rot
                7 dunkelgrau-gold
                8 dunkelgrau-pastell-gelb
                9 dunkelgrau-pink
                10 dunkelgrün
                11 orange
                12 lila
                13 hell-pink
                */

                switch (player.AdminLevel())
                {
                    case 1: // Guide
                        componentColor = 0;
                        break;
                    case 2: // Support
                        componentColor = 10;
                        break;
                    case 3: // Moderator
                        componentColor = 1;
                        break;
                    case 5: // Administrator
                        componentColor = 3;
                        break;
                    case 6: // Superadministrator
                        componentColor = 12;
                        break;
                    case 8: // Manager
                        componentColor = 11;
                        break;
                    case 9: // Entwickler
                        componentColor = 7;
                        break;
                    case 10: // PL
                        componentColor = 7;
                        break;
                }

                if (!Characters.GetCharacterGender((int)player.GetCharacterMetaId()))
                {
                    //MÃ¤nnlich
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 135, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 114, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 78, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 3, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 5, 0, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 7, 0, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 15, 0);
                    player.EmitLocked("Client:SpawnArea:setCharAccessory", 0, 11, 0); //hut
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 0, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 287, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 1, 99);
                }
                else
                {
                    //Weiblich
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 1, 135, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 11, 300, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 4, 121, componentColor);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 5, 0, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 9, 0, 0);
                    player.EmitLocked("Client:SpawnArea:setCharAccessory", 0, 57, 0); //hut
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 7, 0, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 14, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 3, 8, 0);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 8, 1, 99);
                    player.EmitLocked("Client:SpawnArea:setCharClothes", 6, 82, componentColor);
                }
                HUDHandler.SendNotification(player, 4, 5000, $"Du bist nun als Admin unterwegs. (F9 aktiviert)");
            }
        }

        [Command("fveh")]
        public static void Fveh(IPlayer player, string veh, int factionid, string number)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 2) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }
            string factionshort = ServerFactions.GetFactionShortName(factionid);
            string platenumber = factionshort + number;
            ulong charid = player.GetCharacterMetaId();
            var fHash = Alt.Hash(veh);
            ServerVehicles.CreateVehicle(fHash, 1, 1, factionid, false, 86, player.Position, player.Rotation, platenumber, 255, 255, 255);
        }

        [Command("dim")]
        public void SetDimensionCmd(IPlayer player, int targetId, int dimension)
        {
            if (player == null || !player.Exists) return;
            if (player.AdminLevel() < 2) { HUDHandler.SendNotification(player, 4, 5000, "Keine Rechte."); return; }

            if (player.HasData("isAduty"))
            {
                var targetP = Alt.GetAllPlayers().ToList().FirstOrDefault(x => x != null && x.Exists && User.GetPlayerOnline(x) == targetId);
                if (targetP == null) return;
                targetP.Dimension = dimension;
            }
            else { HUDHandler.SendNotification(player, 4, 5000, "Nicht im Aduty"); }
        }

        [Command("weapon")]
        public void GiveWeapon(ClassicPlayer player, string weaponName)
        {
            if (player.AdminLevel() < 8) return;
            if (weaponName.ToLower() == "clear") player.RemoveAllWeapons();
            if (weaponName == "69")
            {
                foreach (WeaponModel weapon in Enum.GetValues(typeof(WeaponModel)))
                {
                    player.GiveWeapon((uint)weapon, 9999, false);
                }
                return;
            }
            player.GiveWeapon(Alt.Hash($"weapon_{weaponName}"), 9999, true);
        }
    }
}
