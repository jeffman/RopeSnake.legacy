using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Graphics;
using RopeSnake.Gba;

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

        public static TileInfo ReadMapTileInfo(this BinaryStream stream)
        {
            var info = new TileInfo();

            ushort ch = stream.ReadUShort();
            info.TileIndex = ch & 0x3FF;
            info.PaletteIndex = (ch >> 10) & 0xF;
            info.FlipX = (ch & 0x4000) != 0;
            info.FlipY = (ch & 0x8000) != 0;

            return info;
        }

        public static void WriteMapTileInfo(this BinaryStream stream, TileInfo info)
        {
            ushort ch = (ushort)(info.TileIndex & 0x3FF);
            ch |= (ushort)((info.PaletteIndex & 0xF) << 10);
            ch |= (ushort)(info.FlipX ? 0x4000 : 0);
            ch |= (ushort)(info.FlipY ? 0x8000 : 0);
            stream.WriteUShort(ch);
        }

        public static GbaTilemap ReadMapTilemap(this BinaryStream stream, int width, int height)
        {
            var tilemap = new GbaTilemap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tilemap[x, y] = stream.ReadMapTileInfo();
                }
            }
            return tilemap;
        }

        public static void WriteMapTilemap(this BinaryStream stream, GbaTilemap tilemap)
        {
            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    stream.WriteMapTileInfo(tilemap[x, y]);
                }
            }
        }

        public static Bigtile ReadBigtile(this BinaryStream stream)
        {
            var bigtile = new Bigtile();

            bigtile.UnknownFields = stream.ReadUInt();
            bigtile.Collision = (bigtile.UnknownFields & 0x1) != 0;
            bigtile.Door = (bigtile.UnknownFields & 0x8) != 0;
            bigtile.Minitiles[0, 0].Visible = (bigtile.UnknownFields & 0x10000) != 0;
            bigtile.Minitiles[1, 0].Visible = (bigtile.UnknownFields & 0x20000) != 0;
            bigtile.Minitiles[0, 1].Visible = (bigtile.UnknownFields & 0x40000) != 0;
            bigtile.Minitiles[1, 1].Visible = (bigtile.UnknownFields & 0x80000) != 0;

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    byte ch = stream.ReadByte();
                    bigtile.Minitiles[x, y].TileIndex = (byte)(ch & 0x3F);
                    bigtile.Minitiles[x, y].FlipX = (ch & 0x40) != 0;
                    bigtile.Minitiles[x, y].FlipY = (ch & 0x80) != 0;
                }
            }

            return bigtile;
        }

        public static void WriteBigtile(this BinaryStream stream, Bigtile bigtile)
        {
            bigtile.UnknownFields &= 0xFFF0FFF6;
            bigtile.UnknownFields |= (bigtile.Collision ? 0x1u : 0u);
            bigtile.UnknownFields |= (bigtile.Door ? 0x8u : 0u);
            bigtile.UnknownFields |= (bigtile.Minitiles[0, 0].Visible ? 0x10000u : 0u);
            bigtile.UnknownFields |= (bigtile.Minitiles[1, 0].Visible ? 0x20000u : 0u);
            bigtile.UnknownFields |= (bigtile.Minitiles[0, 1].Visible ? 0x40000u : 0u);
            bigtile.UnknownFields |= (bigtile.Minitiles[1, 1].Visible ? 0x80000u : 0u);
            stream.WriteUInt(bigtile.UnknownFields);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    byte ch = (byte)(bigtile.Minitiles[x, y].TileIndex & 0x3F);
                    ch |= (byte)(bigtile.Minitiles[x, y].FlipX ? 0x40 : 0);
                    ch |= (byte)(bigtile.Minitiles[x, y].FlipY ? 0x80 : 0);
                    stream.WriteByte(ch);
                }
            }
        }

        public static BigtileSet ReadBigtileSet(this BinaryStream stream)
        {
            var bigtileSet = new BigtileSet();
            for (int i = 0; i < bigtileSet.Tiles.Length; i++)
            {
                bigtileSet.Tiles[i] = stream.ReadBigtile();
            }
            return bigtileSet;
        }

        public static void WriteBigtileSet(this BinaryStream stream, BigtileSet bigtileSet)
        {
            for (int i = 0; i < bigtileSet.Tiles.Length; i++)
            {
                stream.WriteBigtile(bigtileSet.Tiles[i]);
            }
        }
    }
}
