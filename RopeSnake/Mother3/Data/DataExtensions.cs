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

        public static ItemDrop ReadItemDrop(this BinaryStream stream)
        {
            var drop = new ItemDrop();
            drop.Item = stream.ReadByte();
            drop.Chance = stream.ReadByte();
            stream.Position += 2;
            return drop;
        }

        public static void WriteItemDrop(this BinaryStream stream, ItemDrop drop)
        {
            stream.WriteByte(drop.Item);
            stream.WriteByte(drop.Chance);
            stream.WriteUShort(0);
        }

        public static Enemy ReadEnemy(this BinaryStream stream)
        {
            var enemy = new Enemy();

            enemy.Index = stream.ReadInt();

            stream.ReadBytes(enemy.Unknown, 0, 6);

            enemy.BackgroundIndex = stream.ReadUShort();
            enemy.SwirlMusic = stream.ReadUShort();
            enemy.BattleMusic = stream.ReadUShort();
            enemy.WinMusic = stream.ReadUShort();
            enemy.Level = stream.ReadUShort();
            enemy.Hp = stream.ReadInt();
            enemy.Pp = stream.ReadInt();
            enemy.Offense = stream.ReadByte();
            enemy.Defense = stream.ReadByte();
            enemy.Iq = stream.ReadByte();
            enemy.Speed = stream.ReadByte();

            stream.ReadBytes(enemy.Unknown, 6, 4);

            enemy.SurpriseOffense = stream.ReadByte();
            enemy.SurpriseDefense = stream.ReadByte();
            enemy.SurpriseIq = stream.ReadByte();
            enemy.SurpriseSpeed = stream.ReadByte();

            stream.ReadBytes(enemy.Unknown, 6, 4);

            enemy.Weaknesses = new Dictionary<WeaknessType, ushort>();
            for (int i = 0; i < 20; i++)
                enemy.Weaknesses.Add((WeaknessType)i, stream.ReadUShort());

            for (int i = 0; i < 8; i++)
                enemy.Actions[i] = stream.ReadUShort();

            enemy.AttackSound = stream.ReadUShort();
            enemy.EncounterText = stream.ReadByte();
            enemy.DeathText = stream.ReadByte();

            stream.ReadBytes(enemy.Unknown, 14, 16);

            for (int i = 0; i < 3; i++)
                enemy.ItemDrops[i] = stream.ReadItemDrop();

            enemy.Experience = stream.ReadInt();
            enemy.Money = stream.ReadInt();

            stream.ReadBytes(enemy.Unknown, 30, 4);

            return enemy;
        }

        public static void WriteEnemy(this BinaryStream stream, Enemy enemy)
        {
            stream.WriteInt(enemy.Index);

            stream.WriteBytes(enemy.Unknown, 0, 6);

            stream.WriteUShort(enemy.BackgroundIndex);
            stream.WriteUShort(enemy.SwirlMusic);
            stream.WriteUShort(enemy.BattleMusic);
            stream.WriteUShort(enemy.WinMusic);
            stream.WriteUShort(enemy.Level);
            stream.WriteInt(enemy.Hp);
            stream.WriteInt(enemy.Pp);
            stream.WriteByte(enemy.Offense);
            stream.WriteByte(enemy.Defense);
            stream.WriteByte(enemy.Iq);
            stream.WriteByte(enemy.Speed);

            stream.WriteBytes(enemy.Unknown, 6, 4);

            stream.WriteByte(enemy.SurpriseOffense);
            stream.WriteByte(enemy.SurpriseDefense);
            stream.WriteByte(enemy.SurpriseIq);
            stream.WriteByte(enemy.SurpriseSpeed);

            stream.WriteBytes(enemy.Unknown, 10, 4);

            for (int i = 0; i < 20; i++)
                stream.WriteUShort(enemy.Weaknesses[(WeaknessType)i]);

            for (int i = 0; i < 8; i++)
                stream.WriteUShort(enemy.Actions[i]);

            stream.WriteUShort(enemy.AttackSound);
            stream.WriteByte(enemy.EncounterText);
            stream.WriteByte(enemy.DeathText);

            stream.WriteBytes(enemy.Unknown, 14, 16);

            for (int i = 0; i < 3; i++)
                stream.WriteItemDrop(enemy.ItemDrops[i]);

            stream.WriteInt(enemy.Experience);
            stream.WriteInt(enemy.Money);

            stream.WriteBytes(enemy.Unknown, 30, 4);
        }

        public static Psi ReadPsi(this BinaryStream stream)
        {
            var psi = new Psi();

            psi.Index = stream.ReadInt();
            psi.Category = (PsiCategory)stream.ReadInt();
            stream.ReadBytes(psi.Unknown, 0, 4);
            psi.PpCost = stream.ReadUShort();
            stream.ReadBytes(psi.Unknown, 4, 6);
            psi.Type = (ElementalType)stream.ReadInt();
            psi.Target = (PsiTarget)stream.ReadUShort();
            stream.ReadBytes(psi.Unknown, 10, 4);
            psi.LowAmount = stream.ReadUShort();
            psi.HighAmount = stream.ReadUShort();

            int afflictionType = stream.ReadByte();
            if (afflictionType == 0)
            {
                psi.AfflictionType = null;
            }
            else
            {
                psi.AfflictionType = (AilmentType)(afflictionType - 1);
            }

            psi.AfflictionChance = stream.ReadByte();
            stream.ReadBytes(psi.Unknown, 14, 11);
            stream.ReadBytes(psi.Animations, 0, 2);
            stream.ReadBytes(psi.Unknown, 25, 7);

            return psi;
        }

        public static void WritePsi(this BinaryStream stream, Psi psi)
        {
            stream.WriteInt(psi.Index);
            stream.WriteInt((int)psi.Category);
            stream.WriteBytes(psi.Unknown, 0, 4);
            stream.WriteUShort(psi.PpCost);
            stream.WriteBytes(psi.Unknown, 4, 6);
            stream.WriteInt((int)psi.Type);
            stream.WriteUShort((ushort)psi.Target);
            stream.WriteBytes(psi.Unknown, 10, 4);

            if (psi.AfflictionType == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte((byte)(psi.AfflictionType + 1));
            }

            stream.WriteByte(psi.AfflictionChance);
            stream.WriteBytes(psi.Unknown, 14, 11);
            stream.WriteBytes(psi.Animations, 0, 2);
            stream.WriteBytes(psi.Unknown, 25, 7);
        }
    }
}
