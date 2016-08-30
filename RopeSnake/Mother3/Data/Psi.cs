using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RopeSnake.Mother3.Data
{
    [Validate]
    public sealed class Psi : INameHint
    {
        public static readonly int FieldSize = 56;

        [JsonProperty(Order = -1, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string NameHint { get; set; }

        public int Index { get; set; }
        [JsonConverter(typeof(StringEnumConverter)), IsValidEnum(typeof(PsiCategory), Warn = true)]
        public PsiCategory Category { get; set; }
        public ushort PpCost { get; set; }
        [JsonConverter(typeof(StringEnumConverter)), IsValidEnum(typeof(ElementalType), Warn = true)]
        public ElementalType Type { get; set; }
        [JsonConverter(typeof(StringEnumConverter)), IsValidEnum(typeof(PsiTarget), Warn = true)]
        public PsiTarget Target { get; set; }
        public ushort LowAmount { get; set; }
        public ushort HighAmount { get; set; }
        [JsonConverter(typeof(StringEnumConverter)), IsValidEnum(typeof(AilmentType), Warn = true)]
        public AilmentType? AfflictionType { get; set; }
        public byte AfflictionChance { get; set; }

        [NotNull, CountEquals(2)]
        public byte[] Animations { get; set; } = new byte[2];

        [JsonProperty(Order = 99), NotNull, CountEquals(32)]
        public byte[] Unknown { get; set; } = new byte[32];

        [InRange(0, 5)]
        public byte? Test { get; set; }
    }

    public enum PsiCategory : int
    {
        Offense = 0,
        Recover = 1,
        Assist = 2
    }

    public enum PsiTarget : ushort
    {
        None = 0,
        OneAlly = 1,
        HealingGamma = 2,
        AllAllies = 3,
        HealingOmega = 4,
        OneEnemy = 13,
        AllEnemies = 14,
        OneStrike = 15,
        TwoStrikes = 16,
        ThreeStrikes = 17,
        FourStrikes = 18,
        MultipleStrikes = 19
    }
}
