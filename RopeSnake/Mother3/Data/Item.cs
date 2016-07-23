using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Data
{
    public sealed class Item
    {
        public static readonly int FieldSize = 108;

        public int Index { get; set; }
        public ItemType Type { get; set; }
        public bool Key { get; set; }
        public ushort SellPrice { get; set; }
        public EquipFlags EquipFlags { get; set; }
        public int Hp { get; set; }
        public short Pp { get; set; }
        public sbyte Offense { get; set; }
        public sbyte Defense { get; set; }
        public sbyte Iq { get; set; }
        public sbyte Speed { get; set; }
        public ushort LowerHp { get; set; }
        public ushort UpperHp { get; set; }
        public ushort BattleTextIndex { get; set; }
        public Dictionary<AilmentType, short> AilmentProtection { get; set; }
        public Dictionary<ElementalType, sbyte> ElementalProtection { get; set; }
        [JsonProperty(PropertyName = "Unknown", Order = 99)]
        public byte[] Unknown { get; set; } = new byte[52];
    }

    public enum ItemType : int
    {
        Weapon = 0,
        Body = 1,
        Head = 2,
        Arms = 3,
        Food = 4,
        StatusHealer = 5,
        BattleA = 6,
        BattleB = 7,
        ImportantA = 8,
        ImportantB = 9
    }

    [Flags]
    public enum EquipFlags : ushort
    {
        None = 0x0000,
        EmptyA = 0x0001,
        Flint = 0x0002,
        Lucas = 0x0004,
        Duster = 0x0008,
        Kumatora = 0x0010,
        Boney = 0x0020,
        Salsa = 0x0040,
        Wess = 0x0080,
        Thomas = 0x0100,
        Ionia = 0x0200,
        Fuel = 0x0400,
        Alec = 0x0800,
        Fassad = 0x1000,
        Claus = 0x2000,
        EmptyB = 0x4000,
        EmptyC = 0x8000
    }

    public enum AilmentType : byte
    {
        Poison = 0,
        Paralysis = 1,
        Sleep = 2,
        Strange = 3,
        Cry = 4,
        Forgetful = 5,
        Nausea = 6,
        Fleas = 7,
        Burned = 8,
        Solidified = 9,
        Numb = 10
    }

    public enum ElementalType : int
    {
        Neutral = 0,
        Fire = 1,
        Freeze = 2,
        Thunder = 3,
        Bomb = 4
    }
}
