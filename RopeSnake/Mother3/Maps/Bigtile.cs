using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class Bigtile
    {
        [NotNull, ArrayLength(new[] { 2, 2 })]
        public Minitile[,] Minitiles { get; } = new Minitile[2, 2];

        public bool Collision { get; set; }
        public bool Door { get; set; }

        public uint UnknownFields { get; set; }

        public Bigtile()
        {
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    Minitiles[x, y] = new Minitile();
        }
    }
}
