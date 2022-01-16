using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;

namespace Altv_Roleplay.AntiCheat.Models
{
    public class VehicleModel : Vehicle
    {
        private uint _GlobalSystemHealth { get; set; }
        public uint GlobaleSystemHealth
        {
            get => _GlobalSystemHealth;
            set
            {
                BodyHealth = _GlobalSystemHealth;
                _GlobalSystemHealth = value;
            }
        }

        public VehicleModel(IServer server, uint model, Position position, Rotation rotation) : base(server, model, position, rotation) { }
        public VehicleModel(IServer server, IntPtr nativePointer, ushort id) : base(server, nativePointer, id)
        {
            GlobaleSystemHealth = BodyHealth;
        }

        public class VehicleFactory : IEntityFactory<IVehicle>
        {
            public IVehicle Create(IServer server, IntPtr entityPointer, ushort id) => new VehicleModel(server, entityPointer, id) ?? null;
        }
    }
}
