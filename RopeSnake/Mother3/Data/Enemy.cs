using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Data
{
    public sealed class Enemy
    {
        public static readonly int FieldSize = 144;

        [JsonProperty(Order = -1, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string NameHint { get; set; }

        public int Index { get; set; }
        public ushort BackgroundIndex { get; set; }
        public ushort SwirlMusic { get; set; }
        public ushort BattleMusic { get; set; }
        public ushort WinMusic { get; set; }
        public ushort Level { get; set; }
        public int Hp { get; set; }
        public int Pp { get; set; }
        public byte Offense { get; set; }
        public byte Defense { get; set; }
        public byte Iq { get; set; }
        public byte Speed { get; set; }
        public byte SurpriseOffense { get; set; }
        public byte SurpriseDefense { get; set; }
        public byte SurpriseIq { get; set; }
        public byte SurpriseSpeed { get; set; }
        public ushort AttackSound { get; set; }
        public byte EncounterText { get; set; }
        public byte DeathText { get; set; }
        public int Experience { get; set; }
        public int Money { get; set; }
        public Dictionary<WeaknessType, ushort> Weaknesses { get; set; }
        [JsonProperty]
        public ItemDrop[] ItemDrops { get; private set; } = new ItemDrop[3];
        [JsonProperty]
        public ushort[] Actions { get; private set; } = new ushort[8];

        [JsonProperty(Order = 99)]
        public byte[] Unknown { get; set; } = new byte[34];
    }

    public sealed class ItemDrop
    {
        public byte Item { get; set; }
        public byte Chance { get; set; }
    }

    public enum WeaknessType
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
        Numb = 10,
        DCMC = 11,
        WallStaple = 12,
        Apologize = 13,
        MakeLaugh = 14,
        Neutral = 15,
        Fire = 16,
        Freeze = 17,
        Thunder = 18,
        Bomb = 19
    }
}
