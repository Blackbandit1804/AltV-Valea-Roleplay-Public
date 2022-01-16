using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Handler;
using Altv_Roleplay.Model;
using Altv_Roleplay.Utils;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Timers;

namespace Altv_Roleplay
{
    public class Main : AsyncResource
    {
        public override IEntityFactory<IPlayer> GetPlayerFactory()
        {
            return new AccountsFactory();
        }

        public override IBaseObjectFactory<IColShape> GetColShapeFactory()
        {
            return new ColshapeFactory();
        }

        public override IEntityFactory<IVehicle> GetVehicleFactory()
        {
            return new VehicleFactory();
        }

        public override void OnStart()
        {
            AltV.Net.EntitySync.AltEntitySync.Init(7, (threadId) => 200, (threadId) => false,
            (threadCount, repository) => new AltV.Net.EntitySync.ServerEvent.ServerEventNetworkLayer(threadCount, repository),
            (entity, threadCount) => entity.Type,
            (entityId, entityType, threadCount) => entityType,
            (threadId) =>
            {
                return threadId switch
                {
                    // Marker
                    0 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                    // Text
                    1 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 32),
                    // Props
                    2 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 1500),
                    // Help Text
                    3 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 1),
                    // Blips
                    4 => new EntityStreamer.GlobalEntity(),
                    // Dynamic Blip
                    5 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 200),
                    // Ped
                    6 => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 64),
                    _ => new AltV.Net.EntitySync.SpatialPartitions.LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 115),
                };
            },
            new AltV.Net.EntitySync.IdProvider());

            Environment.SetEnvironmentVariable("COMPlus_legacyCorruptedState­­ExceptionsPolicy", "1");

            //Datenbank laden
            Database.DatabaseHandler.ResetDatabaseOnlineState();
            Database.DatabaseHandler.LoadAllPlayers();
            Database.DatabaseHandler.LoadAllPlayerCharacters();
            Database.DatabaseHandler.LoadAllCharacterSkins();
            Database.DatabaseHandler.LoadAllCharacterBankAccounts();
            Database.DatabaseHandler.LoadAllCharacterLastPositions();
            Database.DatabaseHandler.LoadAllCharacterInventorys();
            Database.DatabaseHandler.LoadAllCharacterLicenses();
            Database.DatabaseHandler.LoadAllCharacterPermissions();
            Database.DatabaseHandler.LoadAllCharacterMinijobData();
            Database.DatabaseHandler.LoadAllCharacterPhoneChats();
            Database.DatabaseHandler.LoadAllServerStorages();
            Database.DatabaseHandler.LoadAllCharacterWanteds();
            Database.DatabaseHandler.LoadAllServerBlips();
            Database.DatabaseHandler.LoadAllServerMarkers();
            Database.DatabaseHandler.LoadAllServerVehiclesGlobal();
            Database.DatabaseHandler.LoadAllServerAnimations();
            Database.DatabaseHandler.LoadAllServerATMs();
            Database.DatabaseHandler.LoadAllServerBanks();
            Database.DatabaseHandler.LoadAllServerBankPapers();
            Database.DatabaseHandler.LoadAllServerItems();
            Database.DatabaseHandler.LoadAllServerPeds();
            Database.DatabaseHandler.LoadAllClothesShops();
            Database.DatabaseHandler.LoadAllServerShops();
            Database.DatabaseHandler.LoadAllServerShopItems();
            Database.DatabaseHandler.LoadAllServerBarbers();
            Database.DatabaseHandler.LoadAllServerTeleports();
            Database.DatabaseHandler.LoadAllGarages();
            Database.DatabaseHandler.LoadAllGarageSlots();
            Database.DatabaseHandler.LoadAllVehicleMods();
            Database.DatabaseHandler.LoadAllVehicles();
            Database.DatabaseHandler.LoadAllVehicleTrunkItems();
            Database.DatabaseHandler.LoadAllVehicleShops();
            Database.DatabaseHandler.LoadAllVehicleShopItems();
            Database.DatabaseHandler.LoadAllServerFarmingSpots();
            Database.DatabaseHandler.LoadAllServerFarmingProducers();
            Database.DatabaseHandler.LoadAllServerJobs();
            Database.DatabaseHandler.LoadAllServerLicenses();
            Database.DatabaseHandler.LoadAllServerFuelStations();
            Database.DatabaseHandler.LoadALlServerFuelStationSpots();
            Database.DatabaseHandler.LoadAllServerTabletAppData();
            Database.DatabaseHandler.LoadAllCharactersTabletApps();
            Database.DatabaseHandler.LoadAllCharactersTabletTutorialEntrys();
            Database.DatabaseHandler.LoadAllServerTabletEvents();
            Database.DatabaseHandler.LoadAllServerTabletNotes();
            Database.DatabaseHandler.LoadAllServerCompanys();
            Database.DatabaseHandler.LoadAllServerCompanyMember();
            Database.DatabaseHandler.LoadAllServerFactions();
            Database.DatabaseHandler.LoadAllServerFactionRanks();
            Database.DatabaseHandler.LoadAllServerFactionMembers();
            Database.DatabaseHandler.LoadAllServerFactionStorageItems();
            Database.DatabaseHandler.LoadAllServerDoors();
            Database.DatabaseHandler.LoadAllServerHotels();
            Database.DatabaseHandler.LoadAllServerHouses();
            Database.DatabaseHandler.LoadAllServerMinijobBusdriverRoutes();
            Database.DatabaseHandler.LoadAllServerMinijobBusdriverRouteSpots();
            Database.DatabaseHandler.LoadAllServerMinijobGarbageSpots();
            Database.DatabaseHandler.LoadAllServerLogsFaction();
            Database.DatabaseHandler.LoadAllServerLogsCompany();
            Database.DatabaseHandler.LoadAllTattooStuff();
            Database.DatabaseHandler.LoadAllServerUtilities();

            // Utils
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new Vector3(Constants.Positions.weedLabor_InvPosition.X, Constants.Positions.weedLabor_InvPosition.Y, Constants.Positions.weedLabor_InvPosition.Z - 1), new Vector3(1), color: new Rgba(150, 0, 0, 100), streamRange: 15, dimension: -2147483648);
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new Vector3(Constants.Positions.waffenLabor_InvPosition.X, Constants.Positions.waffenLabor_InvPosition.Y, Constants.Positions.waffenLabor_InvPosition.Z - 1), new Vector3(1), color: new Rgba(150, 0, 0, 100), streamRange: 15, dimension: -2147483648);
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new Vector3(Constants.Positions.storage_ExitPosition.X, Constants.Positions.storage_ExitPosition.Y, Constants.Positions.storage_ExitPosition.Z - 1), new Vector3(1), color: new Rgba(150, 0, 0, 100), streamRange: 15, dimension: -2147483648);
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new Vector3(Constants.Positions.storage_InvPosition.X, Constants.Positions.storage_InvPosition.Y, Constants.Positions.storage_InvPosition.Z - 1), new Vector3(1), color: new Rgba(150, 0, 0, 100), streamRange: 15, dimension: -2147483648);
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, new Vector3(Constants.Positions.fraktionslager_InvPosition.X, Constants.Positions.fraktionslager_InvPosition.Y, Constants.Positions.fraktionslager_InvPosition.Z - 1), new Vector3(1), color: new Rgba(150, 0, 0, 100), streamRange: 15, dimension: -2147483648);

            //Database.DatabaseHandler.RenewAll();
            Minijobs.Elektrolieferant.Main.Initialize();
            Minijobs.Pilot.Main.Initialize();
            Minijobs.Müllmann.Main.Initialize();
            Minijobs.Busfahrer.Main.Initialize();

            //Events registrieren
            Alt.OnColShape += ColAction;
            Alt.OnClient<IPlayer, string>("Server:Utilities:BanMe", banme);

            //Timer initialisieren
            System.Timers.Timer entityTimer = new System.Timers.Timer();
            /*            System.Timers.Timer kilometerTimer = new System.Timers.Timer();
                        System.Timers.Timer LaborTimer = new System.Timers.Timer();
            */
            System.Timers.Timer hudTimer = new System.Timers.Timer();
            System.Timers.Timer LaborTimer = new System.Timers.Timer();
            System.Timers.Timer WaffenLaborTimer = new System.Timers.Timer();
            System.Timers.Timer fuelTimer = new System.Timers.Timer();
            System.Timers.Timer desireTimer = new System.Timers.Timer();
            System.Timers.Timer VehicleAutomaticParkFetchTimer = new System.Timers.Timer();
            var EventSpamTimer = new System.Timers.Timer();
            //Elapsed?
            entityTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnEntityTimer);
            /*            kilometerTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnKilometerTimer);
            */
            hudTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnHUDTimer);
            LaborTimer.Elapsed += new ElapsedEventHandler(TimerHandler.LaborTimer);
            WaffenLaborTimer.Elapsed += new ElapsedEventHandler(TimerHandler.WaffenLaborTimer);
            fuelTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnFuelTimer);
            desireTimer.Elapsed += new ElapsedEventHandler(TimerHandler.OnDesireTimer);
            VehicleAutomaticParkFetchTimer.Elapsed += new ElapsedEventHandler(TimerHandler.VehicleAutomaticParkFetch);
            //EventSpamTimer.Elapsed += new ElapsedEventHandler(Handler.AntiCheatHandler.EventSpamHandler);
            //Interval
            entityTimer.Interval += 60000;
            /*           kilometerTimer.Interval += 10000;
            */
            hudTimer.Interval += 1500;
            fuelTimer.Interval += 30000;
            LaborTimer.Interval += 60000;
            WaffenLaborTimer.Interval += 60000;
            desireTimer.Interval += 200000;
            VehicleAutomaticParkFetchTimer.Interval += 60000 * 5;
            //EventSpamTimer.Interval += 60000;
            //enabled?
            entityTimer.Enabled = true;
            /*            kilometerTimer.Enabled = false;
            */
            hudTimer.Enabled = true;
            fuelTimer.Enabled = true;
            desireTimer.Enabled = true;
            VehicleAutomaticParkFetchTimer.Enabled = true;
            LaborTimer.Enabled = true;
            WaffenLaborTimer.Enabled = true;
            //EventSpamTimer.Enabled = true;

            // Dynasty8
            EntityStreamer.PedStreamer.Create("a_m_y_business_02", Constants.Positions.dynasty8_pedPositionStorage, new Vector3(0, 0, Constants.Positions.dynasty8_pedRotationStorage), 0);
            EntityStreamer.MarkerStreamer.Create(EntityStreamer.MarkerTypes.MarkerTypeVerticalCylinder, Constants.Positions.dynasty8_positionStorage, new Vector3(1), color: new Rgba(255, 51, 51, 100), dimension: 0, streamRange: 20);
            EntityStreamer.HelpTextStreamer.Create("Drücke E um eine Lagerhalle zu erwerben oder deine zu verkaufen.", Constants.Positions.dynasty8_positionStorage, streamRange: 1);

            Console.WriteLine($"Main-Thread = {Thread.CurrentThread.ManagedThreadId}");
        }

        private void banme(IPlayer player, string msg)
        {
            try
            {
                if (player == null || !player.Exists || player.AdminLevel() != 0) return;
                Alt.Log($"Ban Me: {player.Name} - {DateTime.Now.ToString()}");
                int charId = User.GetPlayerOnline(player);
                player.Kick($"Grund: {msg}");
                if (charId <= 0) return;
                User.SetPlayerBanned(Characters.GetCharacterAccountId(charId), true, $"Grund: {msg}");
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
        }

        private void ColAction(IColShape colShape, IEntity targetEntity, bool state)
        {
            if (colShape == null) return;
            if (!colShape.Exists) return;
            IPlayer client = targetEntity as IPlayer;
            if (client == null || !client.Exists) return;
            string colshapeName = colShape.GetColShapeName();
            ulong colshapeId = colShape.GetColShapeId();

            if (colshapeName == "Cardealer" && state == true)
            {
                ulong vehprice = colShape.GetColshapeCarDealerVehPrice();
                string vehname = colShape.GetColshapeCarDealerVehName();
                HUDHandler.SendNotification(client, 1, 2500, $"Name: {vehname}<br>Preis: {vehprice}$");
                return;
            }
            else if (colshapeName == "DoorShape" && state)
            {
                var doorData = ServerDoors.ServerDoors_.FirstOrDefault(x => x.id == (int)colshapeId);
                if (doorData == null) return;
                client.EmitLocked("Client:DoorManager:ManageDoor", doorData.hash, new Position(doorData.posX, doorData.posY, doorData.posZ), (bool)doorData.state);
            }
        }

        public override void OnStop()
        {
            foreach (var player in Alt.GetAllPlayers().Where(p => p != null && p.Exists))
            {
                player.Kick("Server wird heruntergefahren...");
            }
            DiscordLog.SendEmbed("serverstatus", "serverstatus", "Server wurde gestoppt.");
            Alt.Log("Server ist gestoppt.");
        }
    }
}
