using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.Services;
using Altv_Roleplay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
/*using System.Threading;
*/
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class LoginHandler : IScript
    {
        //INSTALL: WBB Verify Daten eingeben
        private string Key = "VEwSvcqG1N0SwcxayaBdSvnNbX4gbESh7lsnizVVTm5SLoHInQt54wS9lurd5Fr4EHKYhHfgJY6X3LVzQhaBr2ARXf2iedGRDOLVqi7eoRUWiV3dgH6R9cDe3siXtiBh";
        private string RequestUrl = "https://classicroleplay.de/WbbVerify.php";

        private static readonly HttpClient Client = new HttpClient();


       
        [AsyncScriptEvent(ScriptEventType.PlayerConnect)]
        public async Task OnPlayerConnect_Handler(ClassicPlayer player, string reason)
        {
        

            if (player == null || !player.Exists) return;
            await AltAsync.Do(() =>
            {
                player.SetSyncedMetaData("PLAYER_SPAWNED", false);
                player.SetSyncedMetaData("ADMINLEVEL", 0);
                player.SetPlayerIsCuffed("handcuffs", false);
                player.SetPlayerIsCuffed("ropecuffs", false);
              

                setCefStatus(player, false);
            });

            player.SetPlayerCurrentMinijob("None");
            player.SetPlayerCurrentMinijobRouteId(0);
            player.SetPlayerCurrentMinijobStep("None");
            player.SetPlayerCurrentMinijobActionCount(0);
            player.SetPlayerFarmingActionMeta("None");
            player.Dimension = -1;
            await User.SetPlayerOnline(player, 0);
            player.EmitLocked("Client:Pedcreator:spawnPed", ServerPeds.GetAllServerPeds());
            CreateLoginBrowser(player);
            
          
          
        } //DONE
   
        [AsyncScriptEvent(ScriptEventType.PlayerDisconnect)]
        public async Task OnPlayerDisconnected_Handler(ClassicPlayer player,string reason)
        {
            try
            {
                if (player == null) return;
                if (User.GetPlayerOnline(player) != 0) Characters.SetCharacterLastPosition(User.GetPlayerOnline(player), player.Position, player.Dimension);
                await User.SetPlayerOnline(player, 0);
                Characters.SetCharacterCurrentFunkFrequence(player.CharacterId, null);

     

                foreach (var client in Alt.GetAllPlayers())
                {
                    var name = Characters.GetCharacterName((int)player.GetCharacterMetaId());
                    if (client == null || !client.Exists) continue;
                    var range = 5; //Change OOC Range!!
                    if (client.Position.Distance(player.Position) <= range)
                    {
                        HUDHandler.SendNotification(client, 2, 5000, $"{player.Name} hat den server Verlassen");
                     
                    }
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }//DONE

        [AsyncClientEvent("Server:CEF:setCefStatus")]
        public static async Task setCefStatus(IPlayer player, bool status)
        {
            if (player == null || !player.Exists) return;
            await AltAsync.Do(() => player.SetSyncedMetaData("IsCefOpen", status));
        }//DONE

        public static void CreateLoginBrowser(IPlayer client)
        {
            if (client == null || !client.Exists) return;
            client.Model = 0x3D843282;
            client.Dimension = 10000;
            client.Position = new Position(3120, 5349, 10);
            client.EmitLocked("Client:Login:CreateCEF"); //Login triggern
        }//DONE

        public static async Task<LoginResponse> MakePostRequest(string requestUrl, string username, string password, string key)
        {
            var values = new Dictionary<string, string>
            {
                { "Username", username },
                { "Password", password },
                { "Key", key }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await Client.PostAsync(requestUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LoginResponse>(responseString);
        }

        [AsyncClientEvent("Server:Login:ValidateLoginCredentials")]
        public async Task ValidateLoginCredentials(ClassicPlayer client, string username, string password)
        {
            if (client == null || !client.Exists || client.IsBlocked || client.isSpawned) return;
            client.IsBlocked = true;
            //LoginResponse loginInfo;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                client.EmitLocked("Client:Login:showError", "Eines der Felder wurde nicht ordnungsgemäß ausgefüllt.");
                client.IsBlocked = false;
                return;
            }

            //loginInfo = MakePostRequest(RequestUrl, username, password, Key).Result;

            //switch (loginInfo.StatusCode)
            //{
            //    case LoginStatusCode.Success:
            //        if (loginInfo.UserData.Banned)
            //        {
            //            if (User.IsPlayerBanned(client))
            //            {
            //                client.EmitLocked("Client:Login:showError", $"Du wurdest gebannt. Grund: {loginInfo.UserData.BanReason}", loginInfo.UserData.BanReason);
            //            }
            //            else
            //            {
            //                User.SetPlayerBanned(client, true, loginInfo.UserData.BanReason);
            //                client.EmitLocked("Client:Login:showError", $"Du wurdest gebannt. Grund: {loginInfo.UserData.BanReason}", loginInfo.UserData.BanReason);
            //            }
            //            await Task.Delay(1000);
            //            client.IsBlocked = false;
            //            return;
            //        }
            //        else if (User.IsPlayerBanned(client))
            //        {
            //            client.EmitLocked("Client:Login:showError", $"Du wurdest gebannt. Grund: {User.GetPlayerBanReason(client)}", User.GetPlayerBanReason(client));
            //            await Task.Delay(1000);
            //            client.IsBlocked = false;
            //            return;
            //        }

            //        if (!User.ExistPlayerName(username))
            //        {
            //            User.CreatePlayerAccount(client, username, loginInfo.UserData.Email, password);
            //        }

            //        if (loginInfo.UserData.Whitelisted)
            //        {
            //            client.Dimension = (short)User.GetPlayerAccountId(client);
            //            client.EmitLocked("Client:Login:SaveLoginCredentialsToStorage", username, password);
            //            User.SetPlayerOnline((ClassicPlayer)client, 0);
            //            SendDataToCharselectorArea(client);
            //            LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, true, "Erfolgreich eingeloggt.");
            //            stopwatch.Stop();
            //            if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"ValidateLoginCredentials benötigte {stopwatch.Elapsed.Milliseconds}ms");
            //            await Task.Delay(1000);
            //            client.IsBlocked = false;
            //            return;
            //        }
            //        else
            //        {
            //            client.EmitLocked("Client:Login:showError", "Beantrage bitte eine Green_Card im Forum.");
            //        }
            //        break;

            //    case LoginStatusCode.WrongPasswordUsername:
            //        client.EmitLocked("Client:Login:showError", "Der Benutzername oder das Passwort stimmen nicht überein.");
            //        LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Der Benutzername oder das Passwort stimmen nicht überein.");
            //        break;

            //    case LoginStatusCode.DataMissing:
            //        client.EmitLocked("Client:Login:showError", "Trage deinen Benutzernamen oder Passwort ein.");
            //        LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Trage deinen Benutzernamen oder Passwort ein.");
            //        new LoginResponse(LoginStatusCode.DataMissing, null);
            //        break;

            //    case LoginStatusCode.KeyWrong:
            //        client.EmitLocked("Client:Login:showError", "Der Login Service ist nicht erreichbar.");
            //        LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Key-Error: WbbVerify");
            //        new LoginResponse(LoginStatusCode.KeyWrong, null);
            //        break;

            //    default:
            //        new LoginResponse(LoginStatusCode.Error, null);
            //        client.EmitLocked("Client:Login:showError", "der die das");
            //        break;
            //}



            if (!User.ExistPlayerName(username))
            {
                client.EmitLocked("Client:Login:showError", "Der eingegebene Benutzername wurde nicht gefunden.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Der eingegebene Benutzername wurde nicht gefunden ({username}).");
                client.IsBlocked = false;
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, User.GetPlayerPassword(username)))
            {
                client.EmitLocked("Client:Login:showError", "Das eingegebene Passwort ist falsch.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Das eingegebene Passwort ist falsch");
                return;
            }

            /*if(User.GetPlayerSocialclubId(username) != client.SocialClubId)*//**/
            if (User.GetPlayerSocialclubId(username) != 0) { if (User.GetPlayerSocialclubId(username) != client.SocialClubId) { client.EmitLocked("Client:Login:showError", "Fehler bei der Anmeldung (Fehlercode 508)."); client.IsBlocked = false; return; } }
            else { User.SetPlayerSocialID(client); }

            if (!User.IsPlayerWhitelisted(username))
            {
                client.EmitLocked("Client:Login:showError", "Dieser Benutzeraccount wurde noch nicht im Support aktiviert.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Dieser Benutzeraccount wurde noch nicht im Support aktiviert ({username}).");
                client.IsBlocked = false;
                return;
            }

            if (User.GetPlayerHardwareID(client) != 0) { if (User.GetPlayerHardwareID(client) != client.HardwareIdHash) { client.EmitLocked("Client:Login:showError", "Fehler bei der Anmeldung (Fehlercode 187)."); return; } }
            else { User.SetPlayerHardwareID(client); }

            if (User.IsPlayerBanned(client))
            {
                client.EmitLocked("Client:Login:showError", "Dieser Benutzeraccount wurde gebannt, im Support melden.");
                LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, false, $"Dieser Benutzeraccount wurde gebannt, im Support melden ({username}).");
                client.IsBlocked = false;
                return;
            }

            client.Dimension = (short)User.GetPlayerAccountId(client);
            client.EmitLocked("Client:Login:SaveLoginCredentialsToStorage", username, password);
            User.SetPlayerOnline((ClassicPlayer)client, 0);
            SendDataToCharselectorArea(client);
            LoggingService.NewLoginLog(username, client.SocialClubId, client.Ip, client.HardwareIdHash, true, "Erfolgreich eingeloggt.");
            stopwatch.Stop();
            if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"ValidateLoginCredentials benötigte {stopwatch.Elapsed.Milliseconds}ms");
            await Task.Delay(1000);
            client.IsBlocked = false;
            return;
        }

        [AsyncClientEvent("Server:Charselector:PreviewCharacter")]
        public async Task PreviewCharacter(IPlayer client, int charid)
        {
            if (client == null || !client.Exists) return;
            client.EmitLocked("Client:Charselector:ViewCharacter", Characters.GetCharacterGender(charid), Characters.GetCharacterSkin("facefeatures", charid), Characters.GetCharacterSkin("headblendsdata", charid), Characters.GetCharacterSkin("headoverlays", charid));
        }

        public static void SendDataToCharselectorArea(IPlayer client)
        {
            if (client == null || !client.Exists) return;
            var charArray = Characters.GetPlayerCharacters(client);
            client.Position = new Position((float)402.778, (float)-996.9758, (float)-98);
            if (client.AdminLevel() > 8)
            {
                client.EmitLocked("Client:Charselector:setMaxChars", 10);
            }

            client.EmitLocked("Client:Charselector:sendCharactersToCEF", charArray);

            client.EmitLocked("Client:Login:showArea", "charselect");
        }

        [AsyncClientEvent("Server:Charselector:spawnChar")]
        public async Task CharacterSelectedSpawnPlace(ClassicPlayer client, string spawnstr, string charcid)
        {
            if (client == null || !client.Exists || spawnstr == null || charcid == null || client.IsBlocked || client.isSpawned) return;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int charid = Convert.ToInt32(charcid);
            if (charid <= 0) return;
            var accEntry = User.Player.ToList().FirstOrDefault(x => x.socialClub == client.SocialClubId);
            if (accEntry == null) { return; }
            if (accEntry.playerid != Characters.GetCharacterAccountId(int.Parse(charcid))) { return; }
            client.IsBlocked = true;
            client.isSpawned = true;
            string charName = Characters.GetCharacterName(charid);
            await User.SetPlayerOnline(client, charid); //Online Feld = CharakterID
            client.CharacterId = charid;

            if (Characters.GetCharacterFirstJoin(charid) && Characters.GetCharacterFirstSpawnPlace(client, charid) == "unset")
            {
                Characters.SetCharacterFirstSpawnPlace(client, charid, spawnstr);
                CharactersInventory.AddCharacterItem(charid, "Bargeld", 10000, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Hamburger", 5, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Orangensaft", 5, "inventory");
                CharactersInventory.AddCharacterItem(charid, "Rucksack", 1, "inventory");
                Characters.SetCharacterBackpack(client, "66");
                CharactersInventory.AddCharacterItem(charid, "Tablet", 1, "inventory");

                // First-Spawn Kleider
                if (!Characters.GetCharacterGender((int)client.GetCharacterMetaId()))
                {
                    //Männlich
                    Characters.SwitchCharacterClothes(client, 5048, false);
                    Characters.SwitchCharacterClothes(client, 7761, false);
                    Characters.SwitchCharacterClothes(client, 4860, false);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 5048);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 7761);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 4860);
                }
                else
                {
                    //Weiblich
                    Characters.SwitchCharacterClothes(client, 17018, false);
                    Characters.SwitchCharacterClothes(client, 21383, false);
                    Characters.SwitchCharacterClothes(client, 27501, false);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 17018);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 21383);
                    CharactersClothes.CreateCharacterOwnedClothes(client.CharacterId, 27501);

                }


                switch (spawnstr)
                {
                    case "lsairport":
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Airport, 0);
                        break;
                    case null:
                        Characters.CreateCharacterLastPos(charid, Constants.Positions.SpawnPos_Airport, 0);
                        break;
                }
            }


            if (Characters.GetCharacterGender(charid)) client.Model = 0x9C9EFFD8;
            else client.Model = 0x705E61F2;

            client.EmitLocked("Client:ServerBlips:LoadAllBlips", ServerBlips.GetAllServerBlips());
            client.EmitLocked("Client:ServerMarkers:LoadAllMarkers", ServerBlips.GetAllServerMarkers());
            client.EmitLocked("Client:SpawnArea:setCharSkin", Characters.GetCharacterSkin("facefeatures", charid), Characters.GetCharacterSkin("headblendsdata", charid), Characters.GetCharacterSkin("headoverlays", charid));
            Position dbPos = Characters.GetCharacterLastPosition(charid);
            await Task.Delay(2000); // Fix char editor spawn
            client.Position = dbPos;
            client.Spawn(dbPos, 0);
            if (Characters.GetCharacterPedName(client.CharacterId) != "none" && !String.IsNullOrWhiteSpace(Characters.GetCharacterPedName(client.CharacterId))) client.Model = Alt.Hash(Characters.GetCharacterPedName(client.CharacterId));
            client.Dimension = 0;
            client.Dimension = Characters.GetCharacterLastDimension(charid);
            client.Health = (ushort)(Characters.GetCharacterHealth(charid) + 100);
            client.Armor = (ushort)Characters.GetCharacterArmor(charid);
            HUDHandler.CreateHUDBrowser(client); //HUD erstellen
            /*            WeatherHandler.SetRealTime(client); //Echtzeit setzen
            */
                     Characters.SetCharacterCorrectClothes(client);
            Characters.SetCharacterLastLogin(charid, DateTime.Now);
            Characters.SetCharacterCurrentFunkFrequence(charid, null);
            Alt.Log($"Eingeloggt {client.Name}");
            Alt.Emit("SaltyChat:EnablePlayer", client, charid);
            client.EmitLocked("SaltyChat_OnConnected");
            client.SetSyncedMetaData("NAME", Characters.GetCharacterAccountId((int)client.GetCharacterMetaId()) + " | " + User.GetPlayerUsername(Characters.GetCharacterAccountId((int)client.GetCharacterMetaId())) + " | " + Characters.GetCharacterName((int)client.GetCharacterMetaId())); //WICHTIG
            if (Characters.IsCharacterUnconscious(charid))
            {
                DeathHandler.openDeathscreen(client);
            }
            if (Characters.IsCharacterFastFarm(charid))
            {
                var fastFarmTime = Characters.GetCharacterFastFarmTime(charid) * 60000;
                client.EmitLocked("Client:Inventory:PlayEffect", "DrugsMichaelAliensFight", fastFarmTime);
                HUDHandler.SendNotification(client, 2, 2000, $"Du bist durch dein Koks noch {fastFarmTime} Minuten effektiver.");
            }
            ServerAnimations.RequestAnimationMenuContent(client);




            if (Characters.IsCharacterPhoneEquipped(charid) && CharactersInventory.ExistCharacterItem(charid, "Smartphone", "inventory") && CharactersInventory.GetCharacterItemAmount(charid, "Smartphone", "inventory") > 0)
            {
                client.EmitLocked("Client:Smartphone:equipPhone", true, Characters.GetCharacterPhonenumber(charid), Characters.IsCharacterPhoneFlyModeEnabled(charid));
                Characters.SetCharacterPhoneEquipped(charid, true);
            }
            else if (!Characters.IsCharacterPhoneEquipped(charid) || !CharactersInventory.ExistCharacterItem(charid, "Smartphone", "inventory") || CharactersInventory.GetCharacterItemAmount(charid, "Smartphone", "inventory") <= 0)
            {
                client.EmitLocked("Client:Smartphone:equipPhone", false, Characters.GetCharacterPhonenumber(charid), Characters.IsCharacterPhoneFlyModeEnabled(charid));
                Characters.SetCharacterPhoneEquipped(charid, false);
            }
            SmartphoneHandler.RequestLSPDIntranet(client);


            await setCefStatus(client, false);
            AltAsync.Do(() =>
            {
                client.SetStreamSyncedMetaData("sharedUsername", $"{charName} ({Characters.GetCharacterAccountId(charid)})");
                client.SetSyncedMetaData("ADMINLEVEL", client.AdminLevel());
                client.SetSyncedMetaData("PLAYER_SPAWNED", true);
            });

            if (Characters.IsCharacterInJail(charid))
            {
                HUDHandler.SendNotification(client, 1, 2500, $"Du befindest dich noch {Characters.GetCharacterJailTime(charid)} Minuten im Gefängnis.", 8000);
                client.Position = new Position(1691.4594f, 2565.7056f, 45.556763f);
                if (Characters.GetCharacterGender(charid) == false)
                {
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 11, 5, 0);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 3, 5, 0);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 4, 7, 15);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 6, 7, 0);
                    client.EmitLocked("Client:SpawnArea:setCharClothes", 8, 1, 88);
                }
                else
                {

                }
            }


            if (!CharactersLicenses.ExistCharacterLicenseEntry(charid))
            {
                CharactersLicenses.CreateCharacterLicensesEntry(charid, false, false, false, false, false, false, false, false);
            }


            if (!CharactersTablet.ExistCharacterTabletAppEntry(charid))
            {
                CharactersTablet.CreateCharacterTabletAppEntry(charid, false, false, false, false, false, false, false, false);
            }


            Characters.SetCharacterHealth(charid, client.Health);
            Characters.SetCharacterArmor(charid, client.Armor);
            client.EmitLocked("Client:HUD:UpdateDesire", Characters.GetCharacterArmor(client.Armor), Characters.GetCharacterHealth(charid), Characters.GetCharacterHunger(charid), Characters.GetCharacterThirst(charid)); //HUD updaten

            //SetBagOnLogin-IfBagIsExistent
            if (Characters.GetCharacterBackpack(Characters.GetCharacterIdFromCharName(charName)) == 45)
            {
                client.EmitLocked("Client:SpawnArea:setCharClothes", 5, 45, 0);
            }
            else if (Characters.GetCharacterBackpack(Characters.GetCharacterIdFromCharName(charName)) == 66)
            {
                client.EmitLocked("Client:SpawnArea:setCharClothes", 5, 66, 0);
            }
            else if (Characters.GetCharacterBackpack(Characters.GetCharacterIdFromCharName(charName)) == 86)
            {
                client.EmitLocked("Client:SpawnArea:setCharClothes", 5, 86, 0);
            }

            client.RemoveAllWeapons();
            string primaryWeapon = (string)Characters.GetCharacterWeapon(client, "PrimaryWeapon");
            int primaryAmmo = (int)Characters.GetCharacterWeapon(client, "PrimaryAmmo");
            string SecWeapon = (string)Characters.GetCharacterWeapon(client, "SecondaryWeapon");
            int SecAmmo = (int)Characters.GetCharacterWeapon(client, "SecondaryAmmo");
            string Sec2Weapon = (string)Characters.GetCharacterWeapon(client, "SecondaryWeapon2");
            int Sec2Ammo = (int)Characters.GetCharacterWeapon(client, "SecondaryAmmo2");
            string FistWeapon = (string)Characters.GetCharacterWeapon(client, "FistWeapon");
            if (primaryWeapon != "None") { client.GiveWeapon(WeaponHandler.GetWeaponModelByName(primaryWeapon), primaryAmmo, false); }
            if (SecWeapon != "None") { client.GiveWeapon(WeaponHandler.GetWeaponModelByName(SecWeapon), SecAmmo, false); }
            if (Sec2Weapon != "None") { client.GiveWeapon(WeaponHandler.GetWeaponModelByName(Sec2Weapon), Sec2Ammo, false); }
            if (FistWeapon != "None") { client.GiveWeapon(WeaponHandler.GetWeaponModelByName(FistWeapon), 1, false); }

            client.updateTattoos();
            stopwatch.Stop();
            if (stopwatch.Elapsed.Milliseconds > 30) Alt.Log($"{charid} - CharacterSelectedSpawnPlace benötigte {stopwatch.Elapsed.Milliseconds}ms");
            await Task.Delay(5000);
            client.IsBlocked = false;
            ServerTattoos.GetAllTattoos(client);
        }
    }
}
