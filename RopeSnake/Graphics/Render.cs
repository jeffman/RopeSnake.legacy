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
    public static class Render
    {
        public static void Tilemap(ITilemap<TileInfo> tilemap, ITileset<Tile> tileset, Palette palette,
            BitmapData canvas, TransparencyMode transparency)
        {
            
        }

        public static void Tile(Tile tile, Palette palette, int subPaletteIndex, bool flipX, bool flipY,
            int x, int y, BitmapData canvas, TransparencyMode transparency)
        {
            if (canvas.PixelFormat != PixelFormat.Format32bppArgb)
                throw new InvalidOperationException("PixelFormat");

            unsafe
            {
                uint* startPixel = (uint*)(canvas.Scan0) + (y * canvas.Stride / sizeof(uint)) + x;
                uint* currentPixel = startPixel;

                Action<uint> drawTransparentPixelAction;
                switch (transparency)
                {
                    case TransparencyMode.DontDraw:
                        drawTransparentPixelAction = c => { };
                        break;

                    case TransparencyMode.DrawSolid:
                        drawTransparentPixelAction = c => { *currentPixel = c; };
                        break;

                    case TransparencyMode.DrawTransparent:
                        drawTransparentPixelAction = c => { *currentPixel = 0; };
                        break;

                    default:
                        throw new ArgumentException(nameof(transparency));
                }

                int startIndexX = flipX ? tile.Width - 1 : 0;
                int startIndexY = flipY ? tile.Height - 1 : 0;

                int indexIncrementX = flipX ? -1 : 1;
                int indexIncrementY = flipY ? -1 : 1;

                int currentIndexY = startIndexY;

                for (int pixelY = 0; pixelY < tile.Height; pixelY++)
                {
                    currentPixel = startPixel;
                    int currentIndexX = startIndexX;

                    for (int pixelX = 0; pixelX < tile.Width; pixelX++)
                    {
                        byte colorIndex = tile[currentIndexX, currentIndexY];
                        uint color = palette[subPaletteIndex, colorIndex].Argb;

                        if (colorIndex == 0)
                            drawTransparentPixelAction(color);
                        else
                            *currentPixel = color;

                        currentIndexX += indexIncrementX;
                        currentIndexY += indexIncrementY;
                        currentPixel++;
                    }

                    startPixel += (canvas.Stride / sizeof(uint));
                }
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
