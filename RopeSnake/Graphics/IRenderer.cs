using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public interface IRenderer : IDisposable
    {
        int Width { get; }
        int Height { get; }
        uint Transparent { get; }
        void SetPixel(int x, int y, uint value);
        uint GetPixel(int x, int y);
    }
}
