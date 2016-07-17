using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public class Palette : IEnumerable<Color>
    {
        private Color[,] _colors;
        private int _subPaletteCount;
        private int _colorsPerSubPalette;

        public virtual int SubPaletteCount => _subPaletteCount;
        public virtual int ColorsPerSubPalette => _colorsPerSubPalette;

        public virtual Color this[int subPaletteIndex, int colorIndex]
        {
            get { return _colors[subPaletteIndex, colorIndex]; }
            set { _colors[subPaletteIndex, colorIndex] = value; }
        }

        public Palette(int subPaletteCount, int colorsPerSubPalette)
        {
            if (subPaletteCount < 0)
                throw new ArgumentException(nameof(subPaletteCount));

            if (colorsPerSubPalette < 0)
                throw new ArgumentException(nameof(colorsPerSubPalette));

            _colors = new Color[subPaletteCount, colorsPerSubPalette];
            _subPaletteCount = subPaletteCount;
            _colorsPerSubPalette = colorsPerSubPalette;
        }

        public virtual Color GetColor(int subPaletteIndex, int colorIndex)
            => _colors[subPaletteIndex, colorIndex];

        public virtual void SetColor(int subPaletteIndex, int colorIndex, Color color)
            => _colors[subPaletteIndex, colorIndex] = color;

        public virtual IEnumerator<Color> GetEnumerator()
        {
            for (int subPalette = 0; subPalette < _subPaletteCount; subPalette++)
            {
                for (int color = 0; color < _colorsPerSubPalette; color++)
                {
                    yield return _colors[subPalette, color];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
