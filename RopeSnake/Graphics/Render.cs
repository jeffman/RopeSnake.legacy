using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = RopeSnake.Graphics.Color;

namespace RopeSnake.Graphics
{
    public static class Render
    {
        private delegate void RenderPixelAction(IRenderer renderer, int x, int y, uint color);

        public static void Tilemap(
            IRenderer renderer,
            int x,
            int y,
            Tilemap<TileInfo> tilemap,
            ITileset<Tile> tileset,
            Palette palette,
            TransparencyMode transparency = TransparencyMode.DontDraw)
        {
            var region = new Region();
            region.MakeInfinite();
            Tilemap(renderer, x, y, tilemap, tileset, palette, region, transparency);
        }

        public static void Tilemap(
            IRenderer renderer,
            int x,
            int y,
            Tilemap<TileInfo> tilemap,
            ITileset<Tile> tileset,
            Palette palette,
            Region clippingRegion,
            TransparencyMode transparency = TransparencyMode.DontDraw)
        {
            CheckConsistentTileSizes(tilemap, tileset);

            for (int tileY = 0; tileY < tilemap.Height; tileY++)
            {
                for (int tileX = 0; tileX < tilemap.Width; tileX++)
                {
                    int pixelX = x + (tileX * tilemap.TileWidth);
                    int pixelY = y + (tileY * tilemap.TileHeight);

                    var tileRect = new Rectangle(pixelX, pixelY, tilemap.TileWidth, tilemap.TileHeight);

                    if (clippingRegion.IsVisible(tileRect))
                    {
                        var tileInfo = tilemap[tileX, tileY];
                        var tile = tileset[tileInfo.TileIndex];

                        Tile(renderer, pixelX, pixelY, tile, palette, clippingRegion,
                            tileInfo.PaletteIndex, tileInfo.FlipX, tileInfo.FlipY,
                            transparency);
                    }
                }
            }
        }

        public static void Tile(
            IRenderer renderer,
            int x,
            int y,
            Tile tile,
            Palette palette,
            Region clippingRegion,
            int subPaletteIndex = 0,
            bool flipX = false,
            bool flipY = false,
            TransparencyMode transparency = TransparencyMode.DontDraw)
        {
            var drawTransparentPixelAction = CreateDrawTransparentPixelAction(transparency);

            int startIndexX = flipX ? tile.Width - 1 : 0;
            int startIndexY = flipY ? tile.Height - 1 : 0;

            int indexIncrementX = flipX ? -1 : 1;
            int indexIncrementY = flipY ? -1 : 1;

            int sourceIndexY = startIndexY;

            for (int destY = y; (destY < y + tile.Height) && (destY < renderer.Height); destY++)
            {
                int sourceIndexX = startIndexX;

                for (int destX = x; (destX < x + tile.Width) && (destX < renderer.Width); destX++)
                {
                    if (clippingRegion.IsVisible(destX, destY))
                    {
                        int colorIndex = tile[sourceIndexX, sourceIndexY];
                        uint color = palette[subPaletteIndex, colorIndex].Argb;

                        if (colorIndex == 0)
                        {
                            drawTransparentPixelAction(renderer, destX, destY, color);
                        }
                        else
                        {
                            renderer.SetPixel(destX, destY, color);
                        }
                    }

                    sourceIndexX += indexIncrementX;
                }

                sourceIndexY += indexIncrementX;
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

        private static RenderPixelAction CreateDrawTransparentPixelAction(TransparencyMode transparency)
        {
            switch (transparency)
            {
                case TransparencyMode.DontDraw:
                    return (r, x, y, p) => { };

                case TransparencyMode.DrawSolid:
                    return (r, x, y, p) => r.SetPixel(x, y, p);

                case TransparencyMode.DrawTransparent:
                    return (r, x, y, p) => r.SetPixel(x, y, r.Transparent);

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
