using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public static class Extensions
    {
        public static BitmapData LockBits(this Bitmap bitmap)
        {
            return bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }
    }
}
