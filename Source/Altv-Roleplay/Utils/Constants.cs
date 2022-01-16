using AltV.Net.Data;

namespace Altv_Roleplay.Utils
{
    public static class Constants
    {
        public static class DatabaseConfig
        {
            public static string Host = "127.0.0.1";
            public static string User = "valea";
            public static string Password = "valea";
            public static string Port = "3306";
            public static string Database = "valea";
        }

        public static class Positions
        {
            public static readonly Position Empty = new Position(0, 0, 0);

            //Staatlich
            public static readonly Position IdentityCardApply = new Position((float)-1291.1208, (float)-571.74066, (float)30.644653); //Personalausweis
            public static readonly Position TownhallHouseSelector = new Position((float)-1295.855, (float)-570.0923, (float)30.560425); //Einwohnermeldeamt
            public static readonly Position Jobcenter_Position = new Position((float)-1290.7781, (float)-576.76483, (float)30.560425); //Jobcenter
            public static readonly Position VehicleLicensing_Position = new Position((float)337.54243, (float)-1562.6241, (float)30); //Zulassungsstelle
            public static readonly Position Vehicleschluesseldienst_Position = new Position((float)331.56924, (float)-1557.3495, (float)30); //Zulassungsstelle
            public static readonly Position VehicleLicensing_VehPosition = new Position((float)345.15164, (float)-1562.6241, (float)28.4); //Zulassungsstele Fzg Pos
            public static readonly Position AutoClubLosSantos_StoreVehPosition = new Position((float)400.4967, (float)-1632.4088, (float)28); //Verwahrstelle Einparkpunkte
            public static readonly Position AutoClubLosSantos_TuneVehPosition = new Position((float)-334.43076, (float)-115.58241, (float)38.002197); //Reparaturpunkt +80f

            //
            //
            public static readonly Position SpawnPos_Airport = new Position((float)-1045.6615, (float)-2751.1912, (float)21.360474);
            public static readonly Rotation SpawnRot_Airport = new Rotation(0, 0, (float)0.44526514);

            //Minijob

            //
            // LIEFERANT
            //
            public static readonly Position Minijob_Elektrolieferent_StartPos = new Position((float)727.170654296875, (float)135.3732147216797, (float)80.75458526611328);
            public static readonly Position Minijob_Elektrolieferant_VehOutPos = new Position((float)694.11426, (float)51.375824, (float)83.5531);
            public static readonly Rotation Minijob_Elektrolieferant_VehOutRot = new Rotation((float)-0.015625, (float)0.0625, (float)-2.078125);

            //
            // PILOT
            //
            public static readonly Position Minijob_Pilot_StartPos = new Position((float)-992.7115478515625, (float)-2948.3564453125, (float)13.957913398742676);
            public static readonly Position Minijob_Pilot_VehOutPos = new Position((float)-981.54724, (float)-2994.8044, (float)14.208423);
            public static readonly Rotation Minijob_Pilot_VehOutRot = new Rotation(0, 0, (float)1.015625);

            //
            // MÜLLMANN
            //
            public static readonly Position Minijob_Müllmann_StartPos = new Position((float)-617.0723266601562, (float)-1622.7850341796875, (float)33.010528564453125);
            public static readonly Position Minijob_Müllmann_VehOutPos = new Position((float)-591.8637, (float)-1586.2814, (float)25.977295);
            public static readonly Rotation Minijob_Müllmann_VehOutRot = new Rotation(0, 0, (float)1.453125);

            // 
            // BUSFAHRER
            // 
            public static readonly Position Minijob_Busdriver_StartPos = new Position((float)454.12713623046875, (float)-600.075927734375, (float)28.578372955322266);
            public static readonly Position Minijob_Busdriver_VehOutPos = new Position((float)466.33847, (float)-579.0725, (float)27.729614);
            public static readonly Rotation Minijob_Busdriver_VehOutRot = new Rotation(0, 0, (float)3.046875);

            //Hotel
            public static readonly Position Hotel_Apartment_ExitPos = new Position((float)266.08685302734375, (float)-1007.5635986328125, (float)-101.00853729248047);
            public static readonly Position Hotel_Apartment_StoragePos = new Position((float)265.9728698730469, (float)-999.4517211914062, (float)-99.00858306884766);

            //Knast
            public static readonly Position Arrest_Position = new Position(1690.9055f, 2591.222f, 45.910645f);

            //Kleidung
            public static readonly Position Clothes_Police = new Position((float)462.93628, (float)-999.33624, (float)29.698345);
            public static readonly Position Clothes_Fib = new Position((float)143.81538, (float)-763.5165, (float)242.14355);
            public static readonly Position Clothes_Medic = new Position((float)298.65494, (float)-597.9429, (float)43.28);
            public static readonly Position Clothes_ACLS = new Position((float)-340.04834, (float)-162, (float)44.579468);
            public static readonly Position Clothes_VUC = new Position((float)104.874725, (float)-1303.556, (float)28.5);
            public static readonly Position Tune_Mode = new Position((float)-352.85275, (float)-93.77143, (float)39.002197);

            //FFA
            public static readonly Position FFA = new Position((float)758.3077, (float)-816.26373, (float)26.499634);
            //FFA
            //S


            //Farming
            public static readonly Position ProcessTest = new Position((float)-252.05, (float)-971.736, (float)31.21);
            public static readonly Position Schwarzwasch = new Position((float)-170.611, (float)1553.7362, (float)-12.372925);

            //Waschstraßen
            public static readonly Position Waschstrasse = new Position((float)24.487913, (float)-1391.9868, (float)29.330444);
            public static readonly Position Waschstrasse2 = new Position((float)-699.9033, (float)-935.9077, (float)19.001465);
            public static readonly Position Waschstrasse3 = new Position((float)0.00, (float)0.00, (float)0.0);
            public static readonly Position Waschstrasse4 = new Position((float)0.00, (float)0.00, (float)0.0);
            public static readonly Position Waschstrasse5 = new Position((float)0.00, (float)0.00, (float)0.0);
            public static readonly Position Waschstrasse6 = new Position((float)0.00, (float)0.00, (float)0.0);
            public static readonly Position Waschstrasse7 = new Position((float)0.00, (float)0.00, (float)0.0);

            // Labor Positions
            public static readonly Position weedLabor_ExitPosition = new Position(444.55383f, 1534.5099f, -12.339233f); //Ausgang
            public static readonly Position weedLabor_InvPosition = new Position(458.61f, 1533.8242f, -12.37292f);  // Verarbeitungspunkt
            public static readonly Position waffenLabor_ExitPosition = new Position(1165f, -3196.6f, -39.01306f); //Ausgang
            public static readonly Position waffenLabor_InvPosition = new Position(1158f, -3196.2065f, -39.01245f); // Verarbeitungspunkt
            public static readonly Position fraktionslager_ExitPosition = new Position(-298.1802f, 1509.1912f, -10.3509f);
            public static readonly Position fraktionslager_InvPosition = new Position(992.7165f, -3098.123f, -39.01245f);

            // Lagerhallen Positions
            public static readonly Position storage_ExitPosition = new Position(1088.6505f, -3187.912f, -38.995605f);
            public static readonly Position storage_InvPosition = new Position(1103.3407f, -3195.8374f, -38.995605f);
            public static readonly Position storage_LSPDInvPosition = new Position(472.8923f, -996.96265f, 26.263794f);

            // Dynasty 8 Positions
            public static readonly Position dynasty8_pedPositionStorage = new Position(-706.5231f, 264.567f, 82.131f);
            public static readonly float dynasty8_pedRotationStorage = 25.817f;
            public static readonly Position dynasty8_positionStorage = new Position(-707.894f, 266.3209f, 82.131f);
        }
    }
}
