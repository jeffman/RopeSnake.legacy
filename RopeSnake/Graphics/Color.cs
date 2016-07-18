using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public struct Color : IEquatable<Color>
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly uint Argb;
        
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            Argb = (uint)(r | (g << 8) | (b << 16)) | 0xFF000000;
        }

        public bool Equals(Color other)
        {
            return
                R == other.R &&
                G == other.G &&
                B == other.B;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Color))
                return false;

            return Equals((Color)obj);
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        public static bool operator ==(Color first, Color second) => first.Equals(second);

        public static bool operator !=(Color first, Color second) => !(first == second);
    }
}
