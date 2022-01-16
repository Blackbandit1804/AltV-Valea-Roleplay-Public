﻿using AltV.Net.Elements.Entities;
using System;

namespace Altv_Roleplay.Handler
{
    class WeatherHandler
    {
        public static void SetRealTime(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            player.SetDateTime(DateTime.Now);
        }
    }
}
