using AltV.Net.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altv_Roleplay.models
{
    public partial class Server_Storages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int owner { get; set; }
        public int secondOwner { get; set; }
        public Position entryPos { get; set; }
        public List<Server_Storage_Item> items { get; set; }
        public float maxSize { get; set; }
        public int price { get; set; }
        public int isfaction { get; set; }
        public int factionid { get; set; }
        public int showBlip { get; set; }

        [NotMapped]
        public bool isLocked { get; set; } = true;
    }

    public partial class Server_Storage_Item
    {
        public string name { get; set; }
        public int amount { get; set; }
    }
}
