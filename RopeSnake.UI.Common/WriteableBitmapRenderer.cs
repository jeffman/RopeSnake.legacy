using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Graphics;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace RopeSnake.UI.Common
{
    public unsafe sealed class WriteableBitmapRenderer : IRenderer
    {
        private WriteableBitmap _bitmap;
        private uint* _scanStart;

        public int Width => _bitmap.PixelWidth;
        public int Height => _bitmap.PixelHeight;
        public uint Transparent => 0;

        public WriteableBitmapRenderer(WriteableBitmap bitmap)
        {
            if (bitmap.Format != PixelFormats.Pbgra32)
                throw new NotSupportedException("Must use PixelFormats.Pbgra32");

            _bitmap = bitmap;
            bitmap.Lock();
            _scanStart = (uint*)_bitmap.BackBuffer;
        }

        public void SetPixel(int x, int y, uint value)
        {
            uint* destination = _scanStart + (y * _bitmap.BackBufferStride / 4) + x;
            *destination = value;
        }

        public uint GetPixel(int x, int y)
        {
            uint* source = _scanStart + (y * _bitmap.BackBufferStride / 4) + x;
            return *source;
        }

        public void Dispose()
        {
            _bitmap.Unlock();
        }
    }
}
