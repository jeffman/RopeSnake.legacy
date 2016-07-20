using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Color = RopeSnake.Graphics.Color;

namespace RopeSnake.Graphics
{
    public static unsafe class Render
    {
        private delegate void DrawPixelAction(uint* currentPixel, uint color);

        public static Bitmap Tilemap(ITilemap<TileInfo> tilemap, ITileset<Tile> tileset, Palette palette,
            TransparencyMode transparency)
        {
            CheckConsistentTileSizes(tilemap, tileset);
            var bitmap = new Bitmap(tilemap.Width * tilemap.TileWidth, tilemap.Height * tilemap.TileHeight,
                PixelFormat.Format32bppArgb);
            Tilemap(tilemap, tileset, palette, bitmap, 0, 0, new Rectangle(0, 0, bitmap.Width, bitmap.Height), transparency);
            return bitmap;
        }

        public static void Tilemap(ITilemap<TileInfo> tilemap, ITileset<Tile> tileset, Palette palette,
            Bitmap bitmap, int x, int y, Rectangle clippingRegion, TransparencyMode transparency)
        {
            var canvas = bitmap.LockBits();
            Tilemap(tilemap, tileset, palette, canvas, x, y, clippingRegion, transparency);
            bitmap.UnlockBits(canvas);
        }

        public static void Tilemap(ITilemap<TileInfo> tilemap, ITileset<Tile> tileset, Palette palette,
            BitmapData canvas, int x, int y, Rectangle clippingRegion, TransparencyMode transparency)
        {
            CheckConsistentTileSizes(tilemap, tileset);

            for (int tileY = 0; tileY < tilemap.Height; tileY++)
            {
                for (int tileX = 0; tileX < tilemap.Width; tileX++)
                {
                    int pixelX = x + (tileX * tilemap.TileWidth);
                    int pixelY = y + (tileY * tilemap.TileHeight);

                    var tileRect = new Rectangle(pixelX, pixelY, tilemap.TileWidth, tilemap.TileHeight);

                    if (clippingRegion.IntersectsWith(tileRect))
                    {
                        var tileInfo = tilemap[tileX, tileY];
                        var tile = tileset[tileInfo.TileIndex];

                        Tile(tile, palette, tileInfo.PaletteIndex, tileInfo.FlipX, tileInfo.FlipY,
                            pixelX, pixelY, canvas, transparency);
                    }
                }
            }
        }

        public static void Tile(Tile tile, Palette palette, int subPaletteIndex, bool flipX, bool flipY,
            int x, int y, BitmapData canvas, TransparencyMode transparency)
        {
            if (canvas.PixelFormat != PixelFormat.Format32bppArgb)
                throw new InvalidOperationException("PixelFormat");

            uint* startPixel = (uint*)(canvas.Scan0) + (y * canvas.Stride / sizeof(uint)) + x;
            uint* currentPixel = startPixel;

            var drawTransparentPixelAction = CreateDrawTransparentPixelAction(transparency);

            int startIndexX = flipX ? tile.Width - 1 : 0;
            int startIndexY = flipY ? tile.Height - 1 : 0;

            int indexIncrementX = flipX ? -1 : 1;
            int indexIncrementY = flipY ? -1 : 1;

            int currentIndexY = startIndexY;

            for (int pixelY = 0; (pixelY < tile.Height) && (y + pixelY < canvas.Height); pixelY++)
            {
                if (y + pixelY >= 0)
                {
                    currentPixel = startPixel;
                    int currentIndexX = startIndexX;

                    for (int pixelX = 0; (pixelX < tile.Width) && (x + pixelX < canvas.Width); pixelX++)
                    {
                        if (x + pixelX >= 0)
                        {
                            byte colorIndex = tile[currentIndexX, currentIndexY];
                            uint color = palette[subPaletteIndex, colorIndex].Argb;

                            if (colorIndex == 0)
                                drawTransparentPixelAction(currentPixel, color);
                            else
                                *currentPixel = color;
                        }
                        currentIndexX += indexIncrementX;
                        currentPixel++;
                    }
                }
                currentIndexY += indexIncrementY;
                startPixel += (canvas.Stride / sizeof(uint));
            }
        }

        private static void CheckConsistentTileSizes(ITilemap<TileInfo> tilemap, ITileset<Tile> tileset)
        {
            if (tileset.Any(t => t.Width != tilemap.TileWidth || t.Height != tilemap.TileHeight))
                throw new Exception($"All tiles in " +
                    $"{nameof(tileset)} must match " +
                    $"{nameof(tilemap)}.{nameof(tilemap.TileWidth)} and " +
                    $"{nameof(tilemap)}.{nameof(tilemap.TileWidth)} in size");
        }

        private static DrawPixelAction CreateDrawTransparentPixelAction(TransparencyMode transparency)
        {
            switch (transparency)
            {
                case TransparencyMode.DontDraw:
                    return (c, p) => { };

                case TransparencyMode.DrawSolid:
                    return (c, p) => *c = p;

                case TransparencyMode.DrawTransparent:
                    return (c, p) => *c = 0;

                default:
                    throw new ArgumentException(nameof(transparency));
            }
        }
    }

    public enum TransparencyMode
    {
        DontDraw,
        DrawTransparent,
        DrawSolid
    }
}
