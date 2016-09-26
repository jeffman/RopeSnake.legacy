using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public unsafe sealed class BitmapRenderer : IRenderer
    {
        private Bitmap _bitmap;
        private BitmapData _bData;
        private uint* _scanStart;

        public int Width => _bData.Width;
        public int Height => _bData.Height;
        public uint Transparent => 0;

        public BitmapRenderer(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new NotSupportedException("Must use PixelFormat.Format32bppArgb");

            _bitmap = bitmap;
            _bData = bitmap.LockBits();
            _scanStart = (uint*)_bData.Scan0;
        }

        public void SetPixel(int x, int y, uint value)
        {
            uint* destination = _scanStart + (y * _bData.Stride / 4) + x;
            *destination = value;
        }

        public uint GetPixel(int x, int y)
        {
            uint* source = _scanStart + (y * _bData.Stride / 4) + x;
            return *source;
        }

        public void Dispose()
        {
            _bitmap.UnlockBits(_bData);
        }
    }
}
