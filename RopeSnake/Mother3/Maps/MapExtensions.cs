using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Maps
{
    public static class MapExtensions
    {
        public static LayerInfo ReadLayerInfo(this BinaryStream stream)
        {
            var info = new LayerInfo();
            info.UnknownFields = stream.ReadByte();
            info.Width = (byte)((info.UnknownFields & 7) + 1);
            info.Height = (byte)(((info.UnknownFields >> 3) & 7) + 1);
            stream.ReadBytes(info.Unknown, 0, 3);
            return info;
        }

        public static void WriteLayerInfo(this BinaryStream stream, LayerInfo info)
        {
            info.UnknownFields &= 0xC0;
            info.UnknownFields |= (byte)((info.Width - 1) & 7);
            info.UnknownFields |= (byte)(((info.Height - 1) & 7) << 3);
            stream.WriteByte(info.UnknownFields);
            stream.WriteBytes(info.Unknown, 0, 3);
        }

        public static MapInfo ReadMapInfo(this BinaryStream stream)
        {
            var info = new MapInfo();
            info.UnknownFields = stream.ReadUInt();
            info.Alpha[0] = (byte)((info.UnknownFields >> 2) & 0xF);
            info.Alpha[1] = (byte)((info.UnknownFields >> 6) & 0xF);
            stream.ReadBytes(info.Unknown, 0, 12);

            for (int i = 0; i < 3; i++)
                info.Layers[i] = stream.ReadLayerInfo();

            return info;
        }

        public static void WriteMapInfo(this BinaryStream stream, MapInfo info)
        {
            info.UnknownFields &= 0xFFFFFC03;
            info.UnknownFields |= (uint)((info.Alpha[0] & 0xF) << 2);
            info.UnknownFields |= (uint)((info.Alpha[1] & 0xF) << 6);
            stream.WriteUInt(info.UnknownFields);
            stream.WriteBytes(info.Unknown, 0, 12);

            for (int i = 0; i < 3; i++)
                stream.WriteLayerInfo(info.Layers[i]);
        }

        public static GraphicsInfo ReadGraphicsInfo(this BinaryStream stream)
        {
            var info = new GraphicsInfo();
            for (int i = 0; i < 12; i++)
            {
                info.TileSets[i] = stream.ReadShort();
            }
            info.Palette = stream.ReadShort();
            return info;
        }

        public static void WriteGraphicsInfo(this BinaryStream stream, GraphicsInfo info)
        {
            for (int i = 0; i < 12; i++)
            {
                stream.WriteShort(info.TileSets[i]);
            }
            stream.WriteShort(info.Palette);
        }


    }
}
