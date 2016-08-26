using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Data
{
    public static class DataExtensions
    {
        public static Item ReadItem(this BinaryStream stream)
        {
            var item = new Item();

            item.Index = stream.ReadInt();
            item.Type = (ItemType)stream.ReadInt();
            item.Key = !stream.ReadBool();

            item.Unknown[0] = stream.ReadByte();

            item.SellPrice = stream.ReadUShort();
            item.EquipFlags = (EquipFlags)stream.ReadUShort();

            stream.ReadBytes(item.Unknown, 1, 2);

            item.Hp = stream.ReadInt();
            item.Pp = stream.ReadShort();

            stream.ReadBytes(item.Unknown, 3, 2);

            item.Offense = stream.ReadSByte();
            item.Defense = stream.ReadSByte();
            item.Iq = stream.ReadSByte();
            item.Speed = stream.ReadSByte();

            stream.ReadBytes(item.Unknown, 5, 4);

            item.AilmentProtection = new Dictionary<AilmentType, short>();
            for (int i = 0; i < 11; i++)
                item.AilmentProtection.Add((AilmentType)i, stream.ReadShort());

            item.ElementalProtection = new Dictionary<ElementalType, sbyte>();
            for (int i = 0; i < 5; i++)
                item.ElementalProtection.Add((ElementalType)i, stream.ReadSByte());

            stream.ReadBytes(item.Unknown, 9, 19);

            item.LowerHp = stream.ReadUShort();
            item.UpperHp = stream.ReadUShort();

            stream.ReadBytes(item.Unknown, 28, 10);

            item.BattleTextIndex = stream.ReadUShort();

            stream.ReadBytes(item.Unknown, 38, 14);

            return item;
        }

        public static void WriteItem(this BinaryStream stream, Item item)
        {
            stream.WriteInt(item.Index);
            stream.WriteInt((int)item.Type);
            stream.WriteBool(!item.Key);

            stream.WriteByte(item.Unknown[0]);

            stream.WriteUShort(item.SellPrice);
            stream.WriteUShort((ushort)item.EquipFlags);

            stream.WriteBytes(item.Unknown, 1, 2);

            stream.WriteInt(item.Hp);
            stream.WriteShort(item.Pp);

            stream.WriteBytes(item.Unknown, 3, 2);

            stream.WriteSByte(item.Offense);
            stream.WriteSByte(item.Defense);
            stream.WriteSByte(item.Iq);
            stream.WriteSByte(item.Speed);

            stream.WriteBytes(item.Unknown, 5, 4);

            for (int i = 0; i < 11; i++)
                stream.WriteShort(item.AilmentProtection[(AilmentType)i]);

            for (int i = 0; i < 5; i++)
                stream.WriteSByte(item.ElementalProtection[(ElementalType)i]);

            stream.WriteBytes(item.Unknown, 9, 19);

            stream.WriteUShort(item.LowerHp);
            stream.WriteUShort(item.UpperHp);

            stream.WriteBytes(item.Unknown, 28, 10);

            stream.WriteUShort(item.BattleTextIndex);

            stream.WriteBytes(item.Unknown, 38, 14);
        }
    }
}
