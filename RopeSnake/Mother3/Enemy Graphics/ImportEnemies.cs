using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using RopeSnake.Core;
using SharpFileSystem;

namespace FreeSpace
{
    class Pointers
    {
        public static int Base = 0x1C90960;
        public static int BaseSOB = 0x1C91E88;//Start of enemy SOB blocks pointers
        public static int BaseCCG = 0x1C909A8;//Start of enemy CCG blocks pointers
        public static int BasePAL = 0x1C91530;//Start of enemy palettes pointers
        public static void Removal(ref byte[] memblock)
        {
            int t = 0, g = 0, character = 0;
            int[] PointerSOB = new int[257], SOBLength = new int[257], PointerCCG = new int[257], CCGLength = new int[257];
            for (int i = 0; i <= 256; i++)
            {
                int Enemynum = i;
                int PoiSOB = BaseSOB + (Enemynum * 8);
                int PoiCCG = BaseCCG + (Enemynum * 8);
                int PoiPAL = BasePAL + (Enemynum * 8);
                PointerSOB[i] = memblock[PoiSOB] + (memblock[PoiSOB + 1] << 8) + (memblock[PoiSOB + 2] << 16) + (memblock[PoiSOB + 3] << 24) + Base;//SOB
                PointerCCG[i] = memblock[PoiCCG] + (memblock[PoiCCG + 1] << 8) + (memblock[PoiCCG + 2] << 16) + (memblock[PoiCCG + 3] << 24) + Base;//CCG
                SOBLength[i] = memblock[PoiSOB + 4] + (memblock[PoiSOB + 5] << 8) + (memblock[PoiSOB + 6] << 16) + (memblock[PoiSOB + 7] << 24);//SOBLength
                CCGLength[i] = memblock[PoiCCG + 4] + (memblock[PoiCCG + 5] << 8) + (memblock[PoiCCG + 6] << 16) + (memblock[PoiCCG + 7] << 24);//CCGLength
            }
            g = 0;
            while (t <= 256)
            {
                g = PointerSOB[t];
                character = g;
                for (g = 0; g <= SOBLength[t] / 4; g++)
                {
                    memblock[(int)character] = 255;
                    memblock[(int)character + 1] = 255;
                    memblock[(int)character + 2] = 255;
                    memblock[(int)character + 3] = 255;
                    character = character + 4;
                }
                t = t + 1;
            }
            character = 0;
            while (t <= 256)
            {
                g = PointerCCG[t];
                character = g;
                for (g = 0; g <= CCGLength[t] / 4; g++)
                {
                    memblock[(int)character] = 255;
                    memblock[(int)character + 1] = 255;
                    memblock[(int)character + 2] = 255;
                    memblock[(int)character + 3] = 255;
                    character = character + 4;
                }
                t = t + 1;
            }
        }
    }
}//Import of C++ code. It's commented, but somewhat messy.
namespace GBA
{
    class LZ77
    {
        public static int Decompress(byte[] data, int address, out byte[] output)
        {
            output = null;
            int start = address;

            if (data[address++] != 0x10) return -1; // Check for LZ77 signature

            // Read the block length
            int length = data[address++];
            length += (data[address++] << 8);
            length += (data[address++] << 16);
            output = new byte[length];

            int bPos = 0;
            while (bPos < length)
            {
                byte ch = data[address++];
                for (int i = 0; i < 8; i++)
                {
                    switch ((ch >> (7 - i)) & 1)
                    {
                        case 0:

                            // Direct copy
                            if (bPos >= length) break;
                            output[bPos++] = data[address++];
                            break;

                        case 1:

                            // Compression magic
                            int t = (data[address++] << 8);
                            t += data[address++];
                            int n = ((t >> 12) & 0xF) + 3;    // Number of bytes to copy
                            int o = (t & 0xFFF);

                            // Copy n bytes from bPos-o to the output
                            for (int j = 0; j < n; j++)
                            {
                                if (bPos >= length) break;
                                output[bPos] = output[bPos - o - 1];
                                bPos++;
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            return address - start;
        }

        public static byte[] Compress(byte[] data)
        {
            return Compress(data, 0, data.Length);
        }

        public static byte[] Compress(byte[] data, int address, int length)
        {
            int start = address;

            List<byte> obuf = new List<byte>();
            List<byte> tbuf = new List<byte>();
            int control = 0;

            // Let's start by encoding the signature and the length
            obuf.Add(0x10);
            obuf.Add((byte)(length & 0xFF));
            obuf.Add((byte)((length >> 8) & 0xFF));
            obuf.Add((byte)((length >> 16) & 0xFF));

            while ((address - start) < length)
            {
                tbuf.Clear();
                control = 0;
                for (int i = 0; i < 8; i++)
                {
                    bool found = false;

                    // First byte should be raw
                    if (address == start)
                    {
                        tbuf.Add(data[address++]);
                        found = true;
                    }
                    else if ((address - start) >= length)
                    {
                        break;
                    }
                    else
                    {
                        // We're looking for the longest possible string
                        // The farthest possible distance from the current address is 0x1000
                        int max_length = -1;
                        int max_distance = -1;

                        for (int k = 1; k <= 0x1000; k++)
                        {
                            if ((address - k) < start) break;

                            int l = 0;
                            for (; l < 18; l++)
                            {
                                if (((address - start + l) >= length) ||
                                    (data[address - k + l] != data[address + l]))
                                {
                                    if (l > max_length)
                                    {
                                        max_length = l;
                                        max_distance = k;
                                    }
                                    break;
                                }
                            }

                            // Corner case: we matched all 18 bytes. This is
                            // the maximum length, so don't bother continuing
                            if (l == 18)
                            {
                                max_length = 18;
                                max_distance = k;
                                break;
                            }
                        }

                        if (max_length >= 3)
                        {
                            address += max_length;

                            // We hit a match, so add it to the output
                            int t = (max_distance - 1) & 0xFFF;
                            t |= (((max_length - 3) & 0xF) << 12);
                            tbuf.Add((byte)((t >> 8) & 0xFF));
                            tbuf.Add((byte)(t & 0xFF));

                            // Set the control bit
                            control |= (1 << (7 - i));

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        // If we didn't find any strings, copy the byte to the output
                        tbuf.Add(data[address++]);
                    }
                }

                // Flush the temp buffer
                obuf.Add((byte)(control & 0xFF));
                obuf.AddRange(tbuf.ToArray());
            }
            while ((obuf.Count() % 4) != 0)
                obuf.Add(0);
            return obuf.ToArray();
        }
    }
}
namespace RopeSnake.Mother3.Enemy_Graphics
{
    class FinalProducts
    {
        public byte[] CCG, SOB, Palette;
        public int PointerCCG, PointerSOB, PointerPAL, ToCCG, ToSOB, AddressCCG, AddressSOB;
    }
    class Importing
    {
        public static int HasColourInside(Color[] a, Color b)
        {
            for (int o = 0; o < a.Length; o++)
            {
                if ((a[o].R == b.R) && (a[o].B == b.B) && (a[o].G == b.G) && (a[o].A == b.A))
                    return o;
            }
            return -1;
        }
        public static BitmapData LockBits(Bitmap bmp, ImageLockMode imageLockMode)
        {
            return bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), imageLockMode, bmp.PixelFormat);
        }
        unsafe static Bitmap MergeTwo8bppFrontBack(Bitmap firstImage, Bitmap secondImage)
        {
            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }
            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }
            Bitmap outputImage = new Bitmap(Math.Max(firstImage.Width, secondImage.Width), firstImage.Height + secondImage.Height, PixelFormat.Format8bppIndexed);
            outputImage.Palette = firstImage.Palette;
            BitmapData N = LockBits(outputImage, ImageLockMode.WriteOnly);
            BitmapData N3 = LockBits(firstImage, ImageLockMode.ReadOnly);
            byte* ptr = (byte*)N.Scan0;
            BitmapData N2 = LockBits(secondImage, ImageLockMode.ReadOnly);
            byte* ptr2 = (byte*)N2.Scan0;
            byte* ptr3 = (byte*)N3.Scan0;
            for (int i = 0; i < N3.Height; i++)
            {
                for (int y = 0; y < N3.Width; y++)
                {
                    ptr[y + (i * N.Stride)] = ptr3[y + (i * N3.Stride)];
                }
            }
            for (int i = 0; i < N2.Height; i++)
            {
                for (int y = 0; y < N2.Width; y++)
                {
                    ptr[y + ((i + N3.Height) * N.Stride)] = ptr3[y + (i * N2.Stride)];
                }
            }
            outputImage.UnlockBits(N);
            secondImage.UnlockBits(N2);
            firstImage.UnlockBits(N3);
            return outputImage;
        }
        public static Bitmap MergeTwo32ArgbFrontBack(Bitmap firstImage, Bitmap secondImage)
        {
            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }
            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }
            int outputImageWidth = Math.Max(firstImage.Width, secondImage.Width);
            int outputImageHeight = firstImage.Height + secondImage.Height;
            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(firstImage, new Rectangle(new Point(), firstImage.Size),
                    new Rectangle(new Point(), firstImage.Size), GraphicsUnit.Pixel);
                graphics.DrawImage(secondImage, new Rectangle(new Point(0, firstImage.Height), secondImage.Size),
                    new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
            }
            return outputImage;
        }
        static List<Byte> UniteHex(List<Byte> a, List<Byte> b, int Width1, int Width2, int Height1, int Height2)
        {
            List<Byte> c = new List<byte>();
            for (int j = 0; j < Height1; j++)
                for (int i = 0; i < Math.Max(Width1, Width2); i++)
                {
                    for (int u = 0; u < 32; u++)
                    {
                        if (i >= Width1)
                            c.Add(0);
                        else
                            c.Add(a[((j * Width1) * 32) + (i * 32) + u]);
                    }
                }
            for (int j = 0; j < Height2; j++)
                for (int i = 0; i < Math.Max(Width1, Width2); i++)
                {
                    for (int u = 0; u < 32; u++)
                    {
                        if (i >= Width2)
                            c.Add(0);
                        else
                            c.Add(b[((j * Width2) * 32) + (i * 32) + u]);
                    }
                }
            return c;
        }
        static int InsertPointer(byte[] memblock, byte[] a, int Pointer, int LastUsed)
        {
            memblock[Pointer] = (byte)(((LastUsed - FreeSpace.Pointers.Base)) & 0xFF);
            memblock[Pointer + 1] = (byte)(((LastUsed - FreeSpace.Pointers.Base) >> 8) & 0xFF);
            memblock[Pointer + 2] = (byte)(((LastUsed - FreeSpace.Pointers.Base) >> 16) & 0xFF);
            memblock[Pointer + 3] = (byte)(((LastUsed - FreeSpace.Pointers.Base) >> 24) & 0xFF);
            memblock[Pointer + 4] = (byte)(((a.Length)) & 0xFF);
            memblock[Pointer + 5] = (byte)(((a.Length) >> 8) & 0xFF);
            memblock[Pointer + 6] = (byte)(((a.Length) >> 16) & 0xFF);
            memblock[Pointer + 7] = (byte)(((a.Length) >> 24) & 0xFF);
            for (int i = 0; i < a.Length; i++)
                memblock[LastUsed + i] = a[i];
            return a.Length + LastUsed;
        }
        static void InsertOldPointer(byte[] memblock, int Pointer, int OldPointsTo, byte[] a)
        {
            memblock[Pointer] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base)) & 0xFF);
            memblock[Pointer + 1] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base) >> 8) & 0xFF);
            memblock[Pointer + 2] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base) >> 16) & 0xFF);
            memblock[Pointer + 3] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base) >> 24) & 0xFF);
            memblock[Pointer + 4] = (byte)(((a.Length)) & 0xFF);
            memblock[Pointer + 5] = (byte)(((a.Length) >> 8) & 0xFF);
            memblock[Pointer + 6] = (byte)(((a.Length) >> 16) & 0xFF);
            memblock[Pointer + 7] = (byte)(((a.Length) >> 24) & 0xFF);
        }
        static bool AreArraySame(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }
        static List<Byte> Function1(List<Color> FinalColors, Bitmap img, int tilewidth)
        {
            int tileheight = img.Height;
            while (tileheight % 8 != 0)
                tileheight += 1;
            tileheight /= 8;
            List<Byte> Realhex = new List<byte>();
            byte rest;
            for (int u = 0; u < tileheight; u++)
            {
                for (int k = 0; k < tilewidth; k++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            rest = 0;
                            if (((u * 8) + j) < img.Height)
                            {
                                if ((8 * k) + (i * 2) < img.Width)
                                {
                                    Color pixel = img.GetPixel((8 * k) + (i * 2), (u * 8) + j);
                                    if (FinalColors.Exists(x => x == pixel))
                                    {
                                        rest = (Byte)(FinalColors.IndexOf(pixel));
                                    }
                                    else
                                    {
                                        int[] Similar = new int[FinalColors.Count];
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            Similar[d] = 0;
                                            if (FinalColors[d].A >= pixel.A)
                                                Similar[d] += 1000 * (FinalColors[d].A - pixel.A);
                                            else
                                                Similar[d] += 1000 * (pixel.A - FinalColors[d].A);
                                            if (FinalColors[d].B >= pixel.B)
                                                Similar[d] += FinalColors[d].B - pixel.B;
                                            else
                                                Similar[d] += pixel.B - FinalColors[d].B;
                                            if (FinalColors[d].G >= pixel.G)
                                                Similar[d] += FinalColors[d].G - pixel.G;
                                            else
                                                Similar[d] += pixel.G - FinalColors[d].G;
                                            if (FinalColors[d].R >= pixel.R)
                                                Similar[d] += FinalColors[d].R - pixel.R;
                                            else
                                                Similar[d] += pixel.R - FinalColors[d].R;
                                        }
                                        int e = 1000000000;
                                        int MostSimilar = 0;
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            if (Similar[d] <= e)
                                            {
                                                e = Similar[d];
                                                MostSimilar = d;
                                            }
                                        }
                                        rest = (Byte)(MostSimilar);
                                    }
                                }
                            }
                            if (((u * 8) + j) < img.Height)
                            {
                                if (((8 * k) + (i * 2) + 1) < img.Width)
                                {
                                    Color pixel = img.GetPixel((8 * k) + (i * 2) + 1, (u * 8) + j);
                                    if (FinalColors.Exists(x => x == pixel))
                                    {
                                        rest += (Byte)((FinalColors.IndexOf(pixel)) * 16);
                                    }
                                    else
                                    {
                                        int[] Similar = new int[FinalColors.Count];
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            Similar[d] = 0;
                                            if (FinalColors[d].A >= pixel.A)
                                                Similar[d] += 1000 * (FinalColors[d].A - pixel.A);
                                            else
                                                Similar[d] += 1000 * (pixel.A - FinalColors[d].A);
                                            if (FinalColors[d].B >= pixel.B)
                                                Similar[d] += FinalColors[d].B - pixel.B;
                                            else
                                                Similar[d] += pixel.B - FinalColors[d].B;
                                            if (FinalColors[d].G >= pixel.G)
                                                Similar[d] += FinalColors[d].G - pixel.G;
                                            else
                                                Similar[d] += pixel.G - FinalColors[d].G;
                                            if (FinalColors[d].R >= pixel.R)
                                                Similar[d] += FinalColors[d].R - pixel.R;
                                            else
                                                Similar[d] += pixel.R - FinalColors[d].R;
                                        }
                                        int e = 1000000000;
                                        int MostSimilar = 0;
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            if (Similar[d] <= e)
                                            {
                                                e = Similar[d];
                                                MostSimilar = d;
                                            }
                                        }
                                        rest += (Byte)(MostSimilar * 16);
                                    }
                                }
                            }
                            Realhex.Add(rest);
                        }
                    }

                }
            }
            return Realhex;
        }
        static List<Color> Function2(Bitmap img, Bitmap img2, int Back)
        {
            int length = 0, u = 0, k = 0;
            List<int> Times = new List<int>();
            List<int> Connections = new List<int>();
            List<Color> Maxcolors = new List<Color>();
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i);
                    for (k = 0; k < length; k++)
                    {
                        u = 0;
                        if ((((pixel.R / 8) & 31) == ((Maxcolors[k].R / 8) & 31)) && (((pixel.G / 8) & 31) == ((Maxcolors[k].G / 8) & 31)) && (((pixel.B / 8) & 31) == ((Maxcolors[k].B / 8) & 31)) && (pixel.A == Maxcolors[k].A) && (k != 0))//Avoid colors that would be identical, but do not consider the first colour.
                        {
                            u = 1;
                            Times[k] += 1;
                            k = length;
                        }
                    }
                    if (u == 0)
                    {
                        length += 1;
                        Times.Add(0);
                        Maxcolors.Add(pixel);
                    }
                }
            }
            if (Back == 1)
                for (int i = 0; i < img2.Height; i++)
                {
                    for (int j = 0; j < img2.Width; j++)
                    {
                        Color pixel = img2.GetPixel(j, i);
                        for (k = 0; k < length; k++)
                        {
                            u = 0;
                            if ((((pixel.R / 8) & 31) == ((Maxcolors[k].R / 8) & 31)) && (((pixel.G / 8) & 31) == ((Maxcolors[k].G / 8) & 31)) && (((pixel.B / 8) & 31) == ((Maxcolors[k].B / 8) & 31)) && (pixel.A == Maxcolors[k].A) && (k != 0))//Avoid colors that would be identical, but do not consider the first colour.
                            {
                                u = 1;
                                Times[k] += 1;
                                k = length;
                            }
                        }
                        if (u == 0)
                        {
                            length += 1;
                            Times.Add(0);
                            Maxcolors.Add(pixel);
                        }
                    }
                }
            List<int> SortedTimes = Times.OrderByDescending(o => o).ToList();
            for (k = 0; k < length; k++)
            {
                for (u = 0; u < length; u++)
                {
                    if (SortedTimes[k] == Times[u])
                    {
                        Connections.Add(u);
                        u = length;
                    }
                }
            }
            if (Connections[0] != 0)
            {
                if (Connections.IndexOf(0) <= 15)
                    Connections[Connections.IndexOf(0)] = Connections[0];
                else
                    Connections[15] = Connections[0];
                Connections[0] = 0;
            }
            u = 0;
            List<Color> FinalColors = new List<Color>();
            for (k = 0; k < length; k++)
            {
                FinalColors.Add(Maxcolors[Connections[k]]);
            }
            Maxcolors.Clear();
            Connections.Clear();
            Times.Clear();
            SortedTimes.Clear();
            u = 0;
            if (FinalColors.Exists(x => x.A <= 128))
            {
                while (u == 0)
                {
                    if ((FinalColors.FindIndex(x => x.A <= 128)) != (FinalColors.FindLastIndex(x => x.A <= 128)))
                    {
                        FinalColors.RemoveAt(FinalColors.FindLastIndex(x => x.A <= 128));
                    }
                    else u = 1;
                }
            }
            return FinalColors;
        }
        static char ConvertHexToChar(int a)
        {
            char e = '0';
            if (a == 0x0)
                e = '0';
            else if (a == 0x1)
                e = '1';
            else if (a == 0x2)
                e = '2';
            else if (a == 0x3)
                e = '3';
            else if (a == 0x4)
                e = '4';
            else if (a == 0x5)
                e = '5';
            else if (a == 0x6)
                e = '6';
            else if (a == 0x7)
                e = '7';
            else if (a == 0x8)
                e = '8';
            else if (a == 0x9)
                e = '9';
            else if (a == 0xA)
                e = 'A';
            else if (a == 0xB)
                e = 'B';
            else if (a == 0xC)
                e = 'C';
            else if (a == 0xD)
                e = 'D';
            else if (a == 0xE)
                e = 'E';
            else if (a == 0xF)
                e = 'F';
            return e;
        }
        static Byte[,] Finalize(Byte[,] Tile, ref Byte[] SOB, int Tilewidth)
        {
            int a, Shape, Size, Tilestart, XSize, YSize, Tilecount = 0;
            List<List<Byte>> Finalized = new List<List<byte>>();
            int OAMNum = SOB[(SOB[8] + (SOB[9] << 8)) + 2] + (SOB[(SOB[8] + (SOB[9] << 8)) + 3] << 8);
            a = (SOB[8] + (SOB[9] << 8)) + 5;
            while (OAMNum > 0)
            {
                Shape = SOB[a++];
                a++;
                Size = SOB[a++];
                if (Size % 2 != 0)
                    Size -= 1;
                Tilestart = SOB[a] + (SOB[a + 1] << 8);
                SOB[a] = (Byte)(Tilecount & 0xFF);
                SOB[a + 1] = (Byte)((Tilecount >> 8) & 0x3);
                if (Shape == 0)
                {
                    if (Size == 0)
                        XSize = 1;
                    else if (Size == 0x40)
                        XSize = 2;
                    else if (Size == 0x80)
                        XSize = 4;
                    else
                        XSize = 8;
                    YSize = XSize;
                }
                else if (Shape == 0x40)
                {
                    if (Size == 0)
                    {
                        XSize = 2;
                        YSize = 1;
                    }
                    else if (Size == 0x40)
                    {
                        XSize = 4;
                        YSize = 1;
                    }
                    else if (Size == 0x80)
                    {
                        XSize = 4;
                        YSize = 2;
                    }
                    else
                    {
                        XSize = 8;
                        YSize = 4;
                    }
                }
                else
                {
                    if (Size == 0)
                    {
                        XSize = 1;
                        YSize = 2;
                    }
                    else if (Size == 0x40)
                    {
                        XSize = 1;
                        YSize = 4;
                    }
                    else if (Size == 0x80)
                    {
                        XSize = 2;
                        YSize = 4;
                    }
                    else
                    {
                        XSize = 4;
                        YSize = 8;
                    }
                }
                for (int i = 0; i < YSize; i++)
                {
                    for (int j = 0; j < XSize; j++)
                    {
                        Finalized.Add(new List<byte>());
                        for (int k = 0; k < 32; k++)
                        {
                            if (Tilestart + (i * Tilewidth) + j < Tile.Length / 32)
                                Finalized[Tilecount].Add(Tile[Tilestart + (i * Tilewidth) + j, k]);
                            else
                                Finalized[Tilecount].Add(0);
                        }
                        Tilecount++;
                    }
                }
                a += 5;
                OAMNum -= 1;
            }
            if ((SOB[8] + (SOB[9] << 8)) != (SOB[10] + (SOB[11] << 8)))
            {
                OAMNum = SOB[(SOB[10] + (SOB[11] << 8)) + 2] + (SOB[(SOB[10] + (SOB[11] << 8)) + 3] << 8);
                a = (SOB[10] + (SOB[11] << 8)) + 5;
                while (OAMNum > 0)
                {
                    Shape = SOB[a++];
                    a++;
                    Size = SOB[a++];
                    if (Size % 2 != 0)
                        Size -= 1;
                    Tilestart = SOB[a] + (SOB[a + 1] << 8);
                    SOB[a] = (Byte)(Tilecount & 0xFF);
                    SOB[a + 1] = (Byte)((Tilecount >> 8) & 0x3);
                    if (Shape == 0)
                    {
                        if (Size == 0)
                            XSize = 1;
                        else if (Size == 0x40)
                            XSize = 2;
                        else if (Size == 0x80)
                            XSize = 4;
                        else
                            XSize = 8;
                        YSize = XSize;
                    }
                    else if (Shape == 0x40)
                    {
                        if (Size == 0)
                        {
                            XSize = 2;
                            YSize = 1;
                        }
                        else if (Size == 0x40)
                        {
                            XSize = 4;
                            YSize = 1;
                        }
                        else if (Size == 0x80)
                        {
                            XSize = 4;
                            YSize = 2;
                        }
                        else
                        {
                            XSize = 8;
                            YSize = 4;
                        }
                    }
                    else
                    {
                        if (Size == 0)
                        {
                            XSize = 1;
                            YSize = 2;
                        }
                        else if (Size == 0x40)
                        {
                            XSize = 1;
                            YSize = 4;
                        }
                        else if (Size == 0x80)
                        {
                            XSize = 2;
                            YSize = 4;
                        }
                        else
                        {
                            XSize = 4;
                            YSize = 8;
                        }
                    }
                    for (int i = 0; i < YSize; i++)
                    {
                        for (int j = 0; j < XSize; j++)
                        {
                            Finalized.Add(new List<byte>());
                            for (int k = 0; k < 32; k++)
                            {
                                if (Tilestart + (i * Tilewidth) + j < Tile.Length / 32)
                                    Finalized[Tilecount].Add(Tile[Tilestart + (i * Tilewidth) + j, k]);
                                else
                                    Finalized[Tilecount].Add(0);
                            }
                            Tilecount++;
                        }
                    }
                    a += 5;
                    OAMNum -= 1;
                }
            }
            Byte[,] Final = new Byte[Finalized.Count, 32];
            for (int i = 0; i < Finalized.Count; i++)
                for (int u = 0; u < Finalized[i].Count; u++)
                    Final[i, u] = Finalized[i][u];
            return Final;
        }
        static Byte[] SOBGen(Byte[] OAMFront, Byte[] OAMBack, int TileheightFront, int TilewidthFront, int TilewidthBack, int Back)
        {
            List<Byte> SOB = new List<byte>();
            SOB.Add(0x73);
            SOB.Add(0x6F);
            SOB.Add(0x62);
            SOB.Add(0x20);
            if (Back >= 1)
            {
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(0x10);
                SOB.Add(0);
                SOB.Add((Byte)(0x14 + OAMFront.Count()));
                SOB.Add(0);
                SOB.Add((Byte)(0x18 + OAMFront.Count() + OAMBack.Count()));
                SOB.Add(0);
                SOB.Add((Byte)(0x18 + OAMFront.Count() + OAMBack.Count()));
                SOB.Add(0);
            }
            else
            {
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(0x10);
                SOB.Add(0);
                SOB.Add(0x10);
                SOB.Add(0);
                SOB.Add((Byte)(0x14 + OAMFront.Count()));
                SOB.Add(0);
                SOB.Add((Byte)(0x14 + OAMFront.Count()));
                SOB.Add(0);
            }
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add((Byte)((OAMFront.Count() / 8) & 0xFF));
            SOB.Add((Byte)(((OAMFront.Count() / 8) >> 8) & 0xFF));
            SOB.AddRange(OAMFront);
            if (Back >= 1)
            {
                SOB.Add(0);
                SOB.Add(0);
                SOB.Add((Byte)((OAMBack.Count() / 8) & 0xFF));
                SOB.Add((Byte)(((OAMBack.Count() / 8) >> 8) & 0xFF));
                for (int k = 0; k < OAMBack.Count(); k++)
                {
                    if (k % 8 != 4)
                        SOB.Add(OAMBack[k]);
                    else
                    {
                        SOB.Add((Byte)((OAMBack[k] + (Math.Max(TilewidthFront, TilewidthBack) * TileheightFront)) & 0xFF));
                        SOB.Add(((Byte)(OAMBack[k + 1] + (((OAMBack[k] + (Math.Max(TilewidthFront, TilewidthBack) * TileheightFront)) >> 8) & 0x3))));
                        k++;
                        if ((SOB[SOB.Count() - 1]) > 3)
                        {
                            Console.WriteLine("Too many tiles in the end!");
                            Environment.Exit(01);
                        }
                    }
                }
            }
            SOB.Add(4);
            SOB.Add(0);
            SOB.Add(1);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0x7E);
            SOB.Add(0x73);
            SOB.Add(0x6F);
            SOB.Add(0x62);
            return SOB.ToArray();
        }
        static Byte[,] TileOAM(Byte[,] Tile, int Tileheight, int Tilewidth, out int Modiheight, out int Modiwidth, out Byte[] OAM)
        {
            Modiwidth = Tilewidth;
            Modiheight = 0;
            int Limit = Tileheight, times = 0, more = 0, mors = 0;
            List<List<Byte>> TempTiles = new List<List<Byte>>();
            List<List<Byte>> TumpTiles = new List<List<Byte>>();
            int o = Limit;
            if (Tilewidth > 4)
                Modiwidth = 8;
            else if (Tilewidth > 2)
                Modiwidth = 4;
            for (int i = 0; i < Limit; i++)
            {
                Modiheight += 1;
                for (int k = 0; k < Modiwidth; k++)
                {
                    TumpTiles.Add(new List<Byte>());
                    if (k >= Tilewidth)
                    {
                        for (int j = 0; j < 32; j++)
                            TumpTiles[(i * Modiwidth) + k].Add(0);
                    }
                    else
                    {
                        for (int j = 0; j < 32; j++)
                            TumpTiles[(i * Modiwidth) + k].Add(Tile[(i * Tilewidth) + k, j]);
                    }
                }
            }
            int Singularwidth, Singularheight, XAdd = 0;
            List<int> X = new List<int>(), Y = new List<int>(), XSize = new List<int>(), YSize = new List<int>(), Tilestart = new List<int>();
            X.Add(0);
            Y.Add(0);
            XSize.Add(Modiwidth);
            YSize.Add(Tileheight);
            Tilestart.Add(0);
            if (Tileheight > 1)
                TumpTiles = RemoveExceedTiles(TumpTiles, Modiwidth, Tileheight, out Singularwidth, out Singularheight, ref Modiheight, ref XSize, ref YSize, ref X, ref Y, ref XAdd);
            else
            {
                Singularheight = 1; Singularwidth = 1;
            }
            if (TumpTiles.Count > 0)
                TumpTiles = FixTile(TumpTiles, Singularwidth, Tilewidth, ref Singularheight, ref Modiheight, ref XSize, ref YSize);
            else
            {
                List<Byte> s = new List<Byte>();
                for (int c = 0; c < 32; c++)
                    s.Add(0);
                TumpTiles.Add(s);
                Singularheight = 1;
                Tilewidth = 1;
                Tileheight = 1;
            }
            int XTemp = X[X.Count() - 1] + XSize[XSize.Count() - 1];
            MoveTile(ref TumpTiles, Singularwidth, ref Singularheight, ref Modiheight, 0, Tilewidth, XAdd, ref XSize, ref YSize, ref X, ref Y, ref Tilestart);
            TempTiles.AddRange(TumpTiles);
            TumpTiles = new List<List<Byte>>();
            times += 1;
            if (Tilewidth > 8)
            {
                for (int g = 0; g < ((Tilewidth - 1) / 8); g++)
                {
                    Tilestart.Add(Modiheight * 8);
                    mors = more;
                    for (int i = 0; i < Limit; i++)
                    {
                        Modiheight += 1;
                        for (int k = 0; k < 8; k++)
                        {
                            TumpTiles.Add(new List<Byte>());
                            if ((8 * times + k) >= Tilewidth)
                            {
                                for (int j = 0; j < 32; j++)
                                    TumpTiles[(i * 8) + k].Add(0);
                            }
                            else
                            {
                                for (int j = 0; j < 32; j++)
                                    TumpTiles[(i * 8) + k].Add(Tile[(i * Tilewidth) + 8 * times + k, j]);
                            }
                        }
                    }
                    X.Add(XTemp + XAdd);
                    Y.Add(0);
                    XSize.Add(8);
                    YSize.Add(Tileheight);
                    TumpTiles = RemoveExceedTiles(TumpTiles, 8, Tileheight, out Singularwidth, out Singularheight, ref Modiheight, ref XSize, ref YSize, ref X, ref Y, ref XAdd);
                    TumpTiles = FixTile(TumpTiles, Singularwidth, Tilewidth, ref Singularheight, ref Modiheight, ref XSize, ref YSize);
                    XTemp = X[X.Count() - 1] + XSize[XSize.Count() - 1];
                    MoveTile(ref TumpTiles, Singularwidth, ref Singularheight, ref Modiheight, 0, Tilewidth, XAdd, ref XSize, ref YSize, ref X, ref Y, ref Tilestart);
                    TempTiles.AddRange(TumpTiles);
                    TumpTiles = new List<List<Byte>>();
                    times += 1;
                }
            }
            OAM = OAMGen(TempTiles, XSize, YSize, X, Y, Tilestart, Tilewidth, Tileheight).ToArray();
            Limit = TempTiles.Count();
            Byte[,] Newtiles = new Byte[Limit, 32];
            for (int i = 0; i < Limit; i++)
            {
                for (int k = 0; k < 32; k++)
                    Newtiles[i, k] = TempTiles[i][k];
            }
            return Newtiles;
        }
        static void MoveTile(ref List<List<Byte>> TempTiles, int Tilewidth, ref int TileheightMod, ref int Modiheight, int Limitimes, int RealTileWidth, int XAdd, ref List<int> XSize, ref List<int> YSize, ref List<int> X, ref List<int> Y, ref List<int> Tilestart)
        {
            List<byte> Compare0 = new List<byte>();
            for (int j = 0; j < 32; j++)
                Compare0.Add(0);
            int l = RealTileWidth;
            if (RealTileWidth >= 5)
                l = 8;
            else if (RealTileWidth >= 3)
                l = 4;
            int mors, OriginalStart = Tilestart[Tilestart.Count() - 1];
            for (mors = TileheightMod; (mors % 2) != 0; mors++) ;
            int morn = (mors / l) * 2;
            int Tileheight = TileheightMod;
            int XTemp = X[X.Count() - 1];
            if (((Tilewidth % l) == 5) || ((Tilewidth % l) == 6))
            {
                if (morn > 0)
                    XSize[XSize.Count() - 1] -= 2;
                YSize[YSize.Count() - 1] -= morn;
                int YOriginal = Y[Y.Count() - 1] + YSize[YSize.Count() - 1];
                for (int g = 0; g < 3; g++)
                {
                    YSize.Add(morn);
                    Y.Add(YOriginal);
                    Tilestart.Add(6 + ((g * morn) * l) + OriginalStart);
                    XSize.Add(2);
                    X.Add(XTemp + (g * 2));
                    for (int i = 0; i < morn; i++)
                    {
                        for (int k = 0; k < 2; k++)
                            TempTiles[((i + (g * morn)) * l) + k + 6] = TempTiles[((i + (Tileheight - morn)) * l) + (g * 2) + k];
                    }
                    int temp = 0;
                    for (int i = 0; i < morn; i++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            if (TempTiles[((i + (g * morn)) * l) + k + 6] == Compare0)
                                temp += 1;
                        }
                    }
                    if (temp == (2 * morn))
                    {
                        Y.RemoveAt(Y.Count() - 1);
                        X.RemoveAt(X.Count() - 1);
                        YSize.RemoveAt(YSize.Count() - 1);
                        XSize.RemoveAt(XSize.Count() - 1);
                        Tilestart.RemoveAt(Tilestart.Count() - 1);
                    }
                }
                Modiheight -= morn;
                TileheightMod -= morn;
            }
            else if (((Tilewidth % l) == 3) || ((Tilewidth % l) == 4))
            {
                if (RealTileWidth > 4)
                    if (mors / 2 > 0)
                    {
                        XSize[XSize.Count() - 1] = 4;
                        YSize[YSize.Count() - 1] -= mors / 2;
                        int YOriginal = Y[Y.Count() - 1] + YSize[YSize.Count() - 1];
                        X.Add(X[X.Count() - 1]);
                        Y.Add(YOriginal);
                        XSize.Add(4);
                        YSize.Add(mors / 2);
                        Tilestart.Add(4 + OriginalStart);
                        for (int i = 0; i < mors / 2; i++)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                TempTiles[(i * l) + k + 4] = TempTiles[((i + (mors / 2)) * l) + k];
                            }
                        }
                        Modiheight -= mors / 2;
                        TileheightMod -= mors / 2;
                    }
            }
            else if (((Tilewidth % l) == 2) || ((Tilewidth % l) == 1))
            {
                if (RealTileWidth > 2)
                {
                    morn = mors / 4;
                    int Countemp = X.Count() - 1;
                    if ((mors % 4) != 0)
                        morn++;
                    XSize[Countemp] = 2;
                    int YOriginal = Y[Countemp] + morn;
                    for (int g = 0; g < 3; g++)
                    {
                        Tilestart.Add(OriginalStart + (2 * (g + 1)));
                        XSize.Add(2);
                        YSize.Add(0);
                        X.Add(XTemp);
                        Y.Add(YOriginal + (morn * g));
                        int temp = 0;
                        for (int i = 0; i < morn; i++)
                        {
                            if ((i + (morn * (g + 1))) < Tileheight)
                            {
                                Modiheight -= 1;
                                TileheightMod -= 1;
                                YSize[Countemp] -= 1;
                                YSize[YSize.Count() - 1] += 1;
                            }
                            for (int k = 0; k < 2; k++)
                            {
                                if ((i + (morn * (g + 1))) < Tileheight)
                                    TempTiles[(i * l) + (g * 2) + k + 2] = TempTiles[((i + (morn * (g + 1))) * l) + k];
                                else
                                {
                                    for (int j = 0; j < 32; j++)
                                        TempTiles[(i * l) + (g * 2) + k + 2][j] = 0;
                                }
                            }
                        }
                        for (int i = 0; i < morn; i++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                if (TempTiles[(i * l) + (g * 2) + k + 2] == Compare0)
                                    temp += 1;
                            }
                        }
                        if (temp == (2 * morn))
                        {
                            Y.RemoveAt(Y.Count() - 1);
                            X.RemoveAt(X.Count() - 1);
                            YSize.RemoveAt(YSize.Count() - 1);
                            XSize.RemoveAt(XSize.Count() - 1);
                            Tilestart.RemoveAt(Tilestart.Count() - 1);
                        }
                    }
                }
            }
        }
        static List<List<Byte>> FixTile(List<List<Byte>> TempTiles, int Tilewidth, int RealTileWidth, ref int Tileheight, ref int Modiheight, ref List<int> XSize, ref List<int> YSize)
        {
            int l = Tilewidth;
            List<List<Byte>> NewTile = new List<List<Byte>>();
            if (RealTileWidth > 4)
            {
                l = 8;
                XSize[XSize.Count() - 1] = 8;
            }
            else if (RealTileWidth > 2)
            {
                l = 4;
                XSize[XSize.Count() - 1] = 4;
            }
            for (int i = 0; i < Tileheight; i++)
            {
                for (int k = 0; k < l; k++)
                {
                    NewTile.Add(new List<Byte>());
                    if (k >= Tilewidth)
                    {
                        for (int j = 0; j < 32; j++)
                            NewTile[(i * l) + k].Add(0);
                    }
                    else
                        NewTile[(i * l) + k] = TempTiles[(i * Tilewidth) + k];
                }
            }
            while (Tileheight % 2 != 0)
            {
                for (int k = 0; k < l; k++)
                {
                    NewTile.Add(new List<Byte>());
                    for (int j = 0; j < 32; j++)
                        NewTile[(Tileheight * l) + k].Add(0);
                }
                Tileheight += 1;
                Modiheight += 1;
                YSize[YSize.Count() - 1] += 1;
            }
            return NewTile;
        }
        static List<Byte> OAMGen(List<List<Byte>> Tile, List<int> XSize, List<int> YSize, List<int> X, List<int> Y, List<int> Tilestart, int OriginalWidth, int OriginalHeight)
        {
            List<Byte> OAMList = new List<byte>();
            Byte Shape = 0, size = 0;
            List<Byte> Compare0 = new List<Byte>();
            for (int i = 0; i < 32; i++)
                Compare0.Add(0);
            int l = OriginalWidth;
            if (OriginalWidth > 4)
                l = 8;
            else if (OriginalWidth > 2)
                l = 4;
            for (int k = 0; k < (Tilestart.Count()); k++)
            {
                if (Tilestart[k] >= 1024)
                {
                    Console.WriteLine("Too many tiles in the end!");
                    Environment.Exit(01);
                }
                int Height, Tempheight = 0, Width = 0, TempWidth = 0;
                int y = 0;
                int XSizeTemp = 0;
                int YSizeTemp = 0;
                for (int i = YSize[k]; i > 0;)
                {
                    int x = 0;
                    Height = -28 - (((OriginalHeight << 3) / 2) - ((Y[k]) << 3) - ((YSize[k] - i) << 3));
                    Tempheight = 0;
                    if (i >= 8)
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            int temp = 0;
                            Width = -((OriginalWidth << 3) / 2) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            if (Width < 0)
                                Width += 0x400;
                            if (Width > 0x3FF)
                                Width -= 0x400;
                            if (j >= 7)
                            {
                                size = 0xC0;
                                Shape = 0;
                                j = 0;
                                i -= 8;
                                XSizeTemp = 8;
                                YSizeTemp = 8;
                            }
                            else if (j >= 5)
                            {
                                size = 0xC0;
                                Shape = 0x80;
                                XSizeTemp = 4;
                                YSizeTemp = 8;
                                for (int u = 0; u < YSizeTemp; u++)
                                    for (int c = 0; c < XSizeTemp; c++)
                                        if (Tile[Tilestart[k] + ((u + y) * l) + c + x] == Compare0)
                                            temp += 1;
                                if (temp != YSizeTemp * XSizeTemp)
                                {
                                    OAMList.Add((Byte)Height);
                                    OAMList.Add(Shape);
                                    OAMList.Add((Byte)(((Width)) & 0xFF));
                                    OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                                    OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                                    OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                                    OAMList.Add(0);
                                    OAMList.Add(0);
                                }
                                size = 0x80;
                                Shape = 0x80;
                                XSizeTemp = 2;
                                YSizeTemp = 4;
                                temp = 0;
                                for (int u = 0; u < YSizeTemp; u++)
                                    for (int c = 0; c < XSizeTemp; c++)
                                        if (Tile[Tilestart[k] + ((u + y) * l) + c + 4 + x] == Compare0)
                                            temp += 1;
                                if (temp != YSizeTemp * XSizeTemp)
                                {
                                    OAMList.Add((Byte)(Height));
                                    OAMList.Add(Shape);
                                    OAMList.Add((Byte)(((Width) + 32) & 0xFF));
                                    OAMList.Add((Byte)((size) + ((((Width) + 32) >> 8) & 0x1)));
                                    OAMList.Add((Byte)((Tilestart[k] + (y * l) + 4 + x) & 0xFF));
                                    OAMList.Add((Byte)(((Tilestart[k] + (y * l) + 4 + x) >> 8) & 0x3));
                                    OAMList.Add(0);
                                    OAMList.Add(0);
                                }
                                size = 0x80;
                                Shape = 0x80;
                                Tempheight += 32;
                                TempWidth += 32;
                                x += 4;
                                y += 4;
                                j -= 6;
                                i -= 8;
                            }
                            else if (j >= 3)
                            {
                                size = 0xC0;
                                Shape = 0x80;
                                j -= 4;
                                i -= 8;
                                XSizeTemp = 4;
                                YSizeTemp = 8;
                            }
                            else if (j == 2)
                            {
                                size = 0x80;
                                Shape = 0x80;
                                j -= 2;
                                i -= 4;
                                XSizeTemp = 2;
                                YSizeTemp = 4;
                            }
                            else
                            {
                                size = 0x40;
                                Shape = 0x80;
                                j -= 1;
                                i -= 4;
                                XSizeTemp = 1;
                                YSizeTemp = 4;
                            }
                            temp = 0;
                            for (int u = 0; u < YSizeTemp; u++)
                                for (int c = 0; c < XSizeTemp; c++)
                                    if (Tile[Tilestart[k] + ((u + y) * l) + c + x] == Compare0)
                                        temp += 1;
                            if (temp != YSizeTemp * XSizeTemp)
                            {
                                OAMList.Add((Byte)(Height + Tempheight));
                                OAMList.Add(Shape);
                                OAMList.Add((Byte)(((Width) + TempWidth) & 0xFF));
                                OAMList.Add((Byte)((size) + ((((Width) + TempWidth) >> 8) & 0x1)));
                                OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                                OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                                OAMList.Add(0);
                                OAMList.Add(0);
                            }
                            if ((XSize[k] >= 5 && XSize[k] <= 6) || (XSize[k] == 2) || (XSize[k] == 1))
                                y += 4;
                            else
                                y += 8;
                        }
                    }
                    else if (i >= 4)
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            int temp = 0;
                            Width = -((OriginalWidth << 3) / 2) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            if (Width < 0)
                                Width += 0x400;
                            if (Width > 0x3FF)
                                Width -= 0x400;
                            int xtemp = 0;
                            if ((j == 1))
                            {
                                size = 0x40;
                                Shape = 0x80;
                                j -= 1;
                                XSizeTemp = 1;
                                YSizeTemp = 4;
                            }
                            else if (j == 2)
                            {
                                size = 0x80;
                                Shape = 0x80;
                                j = 0;
                                XSizeTemp = 2;
                                YSizeTemp = 4;
                            }
                            else if (j >= 7)
                            {
                                size = 0xC0;
                                Shape = 0x40;
                                j -= 8;
                                XSizeTemp = 8;
                                YSizeTemp = 4;
                            }
                            else
                            {
                                size = 0x80;
                                Shape = 0;
                                j -= 4;
                                xtemp += 4;
                                XSizeTemp = 4;
                                YSizeTemp = 4;
                            }
                            i -= 4;
                            temp = 0;
                            for (int u = 0; u < YSizeTemp; u++)
                                for (int c = 0; c < XSizeTemp; c++)
                                    if (Tile[Tilestart[k] + ((u + y) * l) + c + x] == Compare0)
                                        temp += 1;
                            if (temp != YSizeTemp * XSizeTemp)
                            {
                                OAMList.Add((Byte)Height);
                                OAMList.Add(Shape);
                                OAMList.Add((Byte)(((Width)) & 0xFF));
                                OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                                OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                                OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                                OAMList.Add(0);
                                OAMList.Add(0);
                            }
                            x += xtemp;
                            if ((size == 0x80) && (Shape == 0) && (x == xtemp) && (j > 0))
                                i += 4;
                        }
                        y += 4;
                    }
                    else if ((i == 1) || (i == 3))
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            int temp = 0;
                            Width = -((OriginalWidth << 3) / 2) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            if (Width < 0)
                                Width += 0x400;
                            if (Width > 0x3FF)
                                Width -= 0x400;
                            int xtemp = 0;
                            if (j == 1)
                            {
                                size = 0;
                                Shape = 0;
                                j -= 1;
                                XSizeTemp = 1;
                                YSizeTemp = 1;
                            }
                            else if (j == 2)
                            {
                                size = 0;
                                Shape = 0x40;
                                j = 0;
                                XSizeTemp = 2;
                                YSizeTemp = 1;
                            }
                            else
                            {
                                size = 0x40;
                                Shape = 0x40;
                                j -= 4;
                                xtemp += 4;
                                XSizeTemp = 4;
                                YSizeTemp = 1;
                            }
                            i -= 1;
                            temp = 0;
                            for (int u = 0; u < YSizeTemp; u++)
                                for (int c = 0; c < XSizeTemp; c++)
                                    if (Tile[Tilestart[k] + ((u + y) * l) + c + x] == Compare0)
                                        temp += 1;
                            if (temp != YSizeTemp * XSizeTemp)
                            {
                                OAMList.Add((Byte)Height);
                                OAMList.Add(Shape);
                                OAMList.Add((Byte)(((Width)) & 0xFF));
                                OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                                OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                                OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                                OAMList.Add(0);
                                OAMList.Add(0);
                            }
                            x += xtemp;
                            if ((size == 0x40) && (Shape == 0x40) && (x == xtemp) && (j > 0))
                                i += 1;
                        }
                        y += 1;
                    }
                    else if (i == 2)
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            int temp = 0;
                            Width = -((OriginalWidth << 3) / 2) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            if (Width < 0)
                                Width += 0x400;
                            if (Width > 0x3FF)
                                Width -= 0x400;
                            int xtemp = 0;
                            if ((j == 1))
                            {
                                size = 0;
                                Shape = 0x80;
                                j -= 1;
                                XSizeTemp = 1;
                                YSizeTemp = 2;
                            }
                            else if (j == 2)
                            {
                                size = 0x40;
                                Shape = 0;
                                j = 0;
                                XSizeTemp = 2;
                                YSizeTemp = 2;
                            }
                            else
                            {
                                size = 0x80;
                                Shape = 0x40;
                                j -= 4;
                                xtemp += 4;
                                XSizeTemp = 4;
                                YSizeTemp = 2;
                            }
                            temp = 0;
                            i -= 2;
                            for (int u = 0; u < YSizeTemp; u++)
                                for (int c = 0; c < XSizeTemp; c++)
                                    if (Tile[Tilestart[k] + ((u + y) * l) + c + x] == Compare0)
                                        temp += 1;
                            if (temp != YSizeTemp * XSizeTemp)
                            {
                                OAMList.Add((Byte)Height);
                                OAMList.Add(Shape);
                                OAMList.Add((Byte)(((Width)) & 0xFF));
                                OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                                OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                                OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                                OAMList.Add(0);
                                OAMList.Add(0);
                            }
                            x += xtemp;
                            if ((size == 0x80) && (Shape == 0x40) && (x == xtemp) && (j > 0))
                                i += 2;
                        }
                        y += 2;
                    }
                }
            }
            return OAMList;
        }
        static Byte[] VFlip(Byte[,] Tile, int Tilenumber)
        {
            Byte[] TileVFlip = new Byte[32];
            for (int u = 0; u <= 31; u++)
            {
                if (u <= 3)
                    TileVFlip[u] = Tile[Tilenumber, 28 + u];
                else if (u <= 7)
                    TileVFlip[u] = Tile[Tilenumber, 24 + (u % 4)];
                else if (u <= 11)
                    TileVFlip[u] = Tile[Tilenumber, 20 + (u % 4)];
                else if (u <= 15)
                    TileVFlip[u] = Tile[Tilenumber, 16 + (u % 4)];
                else if (u <= 19)
                    TileVFlip[u] = Tile[Tilenumber, 12 + (u % 4)];
                else if (u <= 23)
                    TileVFlip[u] = Tile[Tilenumber, 8 + (u % 4)];
                else if (u <= 27)
                    TileVFlip[u] = Tile[Tilenumber, 4 + (u % 4)];
                else
                    TileVFlip[u] = Tile[Tilenumber, (u % 4)];
            }
            return TileVFlip;
        }
        static Byte[] HFlip(Byte[,] Tile, int Tilenumber)
        {
            Byte[] TileHFlip = new Byte[32];
            for (int u = 0; u <= 31; u++)
            {
                if ((u % 4) == 0)
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u + 3] % 16) * 16) + (Tile[Tilenumber, u + 3] / 16));
                else if ((u % 4) == 1)
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u + 1] % 16) * 16) + (Tile[Tilenumber, u + 1] / 16));
                else if ((u % 4) == 2)
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u - 1] % 16) * 16) + (Tile[Tilenumber, u - 1] / 16));
                else
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u - 3] % 16) * 16) + (Tile[Tilenumber, u - 3] / 16));
            }
            return TileHFlip;
        }
        static Byte[] Reverse(Byte[,] Tile, int Tilenumber)
        {
            Byte[,] TileReversing = new Byte[1, 32];
            Byte[] TileReverse = new Byte[32];
            TileReverse = HFlip(Tile, Tilenumber);
            for (int k = 0; k < 32; k++)
                TileReversing[0, k] = TileReverse[k];
            TileReverse = VFlip(TileReversing, 0);
            return TileReverse;
        }
        static Byte[] Same(Byte[,] Tile, int Tilenumber)
        {
            Byte[] TileSame = new Byte[32];
            for (int u = 0; u <= 31; u++)
                TileSame[u] = Tile[Tilenumber, u];
            return TileSame;
        }
        static List<List<Byte>> RemoveExceedTiles(List<List<Byte>> Tile, int Tilewidth, int Tileheight, out int Tilewdth, out int Tilehight, ref int Modiheight, ref List<int> XSize, ref List<int> YSize, ref List<int> X, ref List<int> Y, ref int XAdd)
        {
            int temp = 0, hu = 0, hd = 0, hl = 0, hr = 0, g = 0;
            while (g == 0)
            {
                for (int u = (hu * Tilewidth); u < ((hu + 1) * Tilewidth); u++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hu += 1;
                    Modiheight -= 1;
                    Y[Y.Count() - 1] += 1;
                    YSize[YSize.Count() - 1] -= 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = ((Tileheight - hd - 1) * Tilewidth); u < ((Tileheight - hd) * Tilewidth); u++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hd += 1;
                    Modiheight -= 1;
                    YSize[YSize.Count() - 1] -= 1;
                }
                else
                    g = 1;

                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = hl; u <= (((Tileheight - 1) * (Tilewidth)) + hl); u = u + Tilewidth)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tileheight))
                {
                    hl += 1;
                    XAdd -= 1;
                    XSize[XSize.Count() - 1] -= 1;
                    X[X.Count() - 1] += 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = (Tilewidth - hr - 1); u < (((Tileheight) * (Tilewidth)) + Tilewidth - hr - 1); u = u + Tilewidth)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * (Tileheight)))
                {
                    hr += 1;
                    XAdd += 1;
                    XSize[XSize.Count() - 1] -= 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            Tilehight = Tileheight - hu - hd;
            Tilewdth = Tilewidth - hl - hr;
            List<List<Byte>> NewTile = new List<List<Byte>>();
            for (int f = 0; f < (Tilehight); f++)
            {
                for (int d = 0; d < Tilewdth; d++)
                {
                    NewTile.Add(new List<Byte>());
                    NewTile[(d + (Tilewdth * f))] = Tile[hl + d + (Tilewidth * (f + hu))];
                }
            }
            return NewTile;
        }
        static int Comparison(Byte[,] Tile, int Tilenumber, int Tilenumber2)
        {
            int Compared = 0, Use = 0;
            Byte[] TileComp = Same(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be the same.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 1;
            Use = 0;
            TileComp = HFlip(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be HFlipped.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 10;
            Use = 0;
            TileComp = VFlip(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be VFlipped.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 100;
            Use = 0;
            TileComp = Reverse(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be Reversed.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 1000;
            Use = 0;
            return Compared;
        }
        static Byte[,] RemoveExceedingTiles(byte[,] Tile, int Tilewidth, int Tileheight, ref int Backheight, out int Tilewdth, out int Tilehight)
        {
            int temp = 0, hu = 0, hd = 0, hl = 0, hr = 0, g = 0;
            while (g == 0)
            {
                for (int u = (hu * Tilewidth); u < ((hu + 1) * Tilewidth); u++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hu += 1;
                    Backheight -= 8;
                }
                else
                    g = 1;
                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = ((Tileheight - hd - 1) * Tilewidth); u < ((Tileheight - hd) * Tilewidth); u++)
                {
                    if (u == -1) { break; }
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hd += 1;
                }
                else
                    g = 1;

                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = hl; u <= (((Tileheight - 1) * (Tilewidth)) + hl); u = u + Tilewidth)
                {
                    if (u == -1) { break; }
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tileheight))
                {
                    hl += 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = (Tilewidth - hr - 1); u < (((Tileheight) * (Tilewidth)) + Tilewidth - hr - 1); u = u + Tilewidth)
                {
                    if (u == -1) { break; }
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * (Tileheight)))
                {
                    hr += 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            Tilehight = Math.Max(Tileheight - hu - hd, 0);
            Tilewdth = Math.Max(Tilewidth - hl - hr, 0);
            Byte[,] NewTile = new Byte[(Tilehight * Tilewdth), 32];
            for (int f = 0; f < (Tilehight); f++)
            {
                for (int d = 0; d < Tilewdth; d++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        NewTile[(d + (Tilewdth * f)), k] = Tile[hl + d + (Tilewidth * (f + hu)), k];
                    }
                }
            }
            return NewTile;
        }
        static Byte[,] SeparateBack(byte[,] Tile, int Tilewidth, int Tileheight, int Back, int Backheight, out int Tilehight, out int Tilehight2, out byte[,] TileBack)
        {
            if (Back >= 1)
            {
                Tilehight = (Backheight / 8);
                Tilehight2 = Tileheight - (Backheight / 8);
            }
            else
            {
                Tilehight = Tileheight;
                Tilehight2 = 0;
            }
            Byte[,] TileFront = new Byte[Tilewidth * Tilehight, 32];
            TileBack = new Byte[Tilewidth * Tilehight2, 32];
            for (int i = 0; i < (Tilewidth * Tilehight); i++)
            {
                for (int j = 0; j < 32; j++)
                    TileFront[i, j] = Tile[i, j];
            }
            for (int i = 0; i < (Tilehight2 * Tilewidth); i++)
            {
                for (int j = 0; j < 32; j++)
                    TileBack[i, j] = Tile[i + (Tilewidth * Tilehight), j];
            }
            return TileFront;
        }
        static Byte[,] UniteTile(Byte[,] TileFront, Byte[,] TileBack, int Back, int Tilewidth, int Tileheight, int Tilewidth2, int Tileheight2, ref byte[] OAMFront, ref byte[] OAMBack, out int TilewidthUNITE, out int TileheightUNITE)
        {
            if (Tilewidth > Tilewidth2)
                TilewidthUNITE = Tilewidth;
            else
                TilewidthUNITE = Tilewidth2;
            TileheightUNITE = Tileheight + Tileheight2;
            if (TilewidthUNITE != Tilewidth)
            {
                int e = OAMFront.Count() / 8;
                for (int b = 0; b < e; b++)
                {
                    int N = OAMFront[(b * 8) + 4] + ((OAMFront[(b * 8) + 5] & 0x3) << 8);
                    N = N / Tilewidth;
                    N = N * TilewidthUNITE;
                    OAMFront[(b * 8) + 4] = (byte)(N & 0xFF);
                    OAMFront[(b * 8) + 5] = (byte)((N >> 8) & 0x3);
                }
            }
            if (TilewidthUNITE != Tilewidth2)
            {
                int e = OAMBack.Count() / 8;
                for (int b = 0; b < e; b++)
                {
                    int N = OAMBack[(b * 8) + 4] + ((OAMBack[(b * 8) + 5] & 0x3) << 8);
                    N = N / Tilewidth2;
                    N = N * TilewidthUNITE;
                    OAMBack[(b * 8) + 4] = (byte)(N & 0xFF);
                    OAMBack[(b * 8) + 5] = (byte)((N >> 8) & 0x3);
                }
            }
            Byte[,] Tile = new Byte[TilewidthUNITE * TileheightUNITE, 32];
            for (int i = 0; i < Tileheight; i++)
            {
                for (int k = 0; k < TilewidthUNITE; k++)
                {
                    if (k >= Tilewidth)
                    {
                        for (int j = 0; j < 32; j++)
                            Tile[(i * TilewidthUNITE) + k, j] = 0;
                    }
                    else
                        for (int j = 0; j < 32; j++)
                            Tile[(i * TilewidthUNITE) + k, j] = TileFront[(i * Tilewidth) + k, j];
                }
            }
            if (Back >= 1)
            {
                for (int i = 0; i < Tileheight2; i++)
                {
                    for (int k = 0; k < TilewidthUNITE; k++)
                    {
                        if (k >= Tilewidth2)
                        {
                            for (int j = 0; j < 32; j++)
                                Tile[((i + Tileheight) * TilewidthUNITE) + k, j] = 0;
                        }
                        else
                            for (int j = 0; j < 32; j++)
                                Tile[((i + Tileheight) * TilewidthUNITE) + k, j] = TileBack[(i * Tilewidth2) + k, j];
                    }
                }
            }
            return Tile;
        }
        public static void Import(ref Block alpha, IProgress<ProgressPercent> progress, string OutputO)
        {
            byte[] memblock = alpha.Data;
            int LastCCG = (memblock[FreeSpace.Pointers.BaseCCG]) + (memblock[FreeSpace.Pointers.BaseCCG + 1] << 8) + (memblock[FreeSpace.Pointers.BaseCCG + 2] << 16) + (memblock[FreeSpace.Pointers.BaseCCG + 3] << 24) + FreeSpace.Pointers.Base;
            int LastSOB = (memblock[FreeSpace.Pointers.BaseSOB]) + (memblock[FreeSpace.Pointers.BaseSOB + 1] << 8) + (memblock[FreeSpace.Pointers.BaseSOB + 2] << 16) + (memblock[FreeSpace.Pointers.BaseSOB + 3] << 24) + FreeSpace.Pointers.Base;
            FreeSpace.Pointers.Removal(ref memblock);
            List<FinalProducts> End = new List<FinalProducts>();
            int Num = -1;
            while (Num <= 256)
            {
                Num += 1;
                for (; Num <= 256; Num++)
                {
                    string Put="";
                    string Path = OutputO+"\\BattleSprites\\";
                    if (Num < 10)
                        Put += "00";
                    else if (Num < 100)
                    {
                        Put += "0";
                        Put += ConvertHexToChar(Num / 10);
                    }
                    else
                    {
                        Put += ConvertHexToChar(Num / 100);
                        Put += ConvertHexToChar((Num / 10) % 10);
                    }
                    Put += Num % 10;
                    Path += Put;
                    progress?.Report(new ProgressPercent("Reading Enemy " + Num+"'s Graphics",
                        ((Num * 100f) / 257)));
                    if (System.IO.File.Exists(Path + ".png"))
                    {
                        int Back = 0, Enemynum = Num;
                        if (System.IO.File.Exists(Path + "Back.png"))
                            Back = 1;
                        int k = 0, u = 0;
                        Bitmap img = new Bitmap(Path + ".png");
                        Bitmap img2 = img;
                        List<int> Times = new List<int>();
                        List<int> Connections = new List<int>();
                        List<Color> Maxcolors = new List<Color>();
                        int height = img.Height, lenght = 0;
                        if (Back == 1)
                        {
                            img2 = new Bitmap(Path + "Back.png");
                            if ((img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed) && (img.PixelFormat == img2.PixelFormat))
                            {
                                img = MergeTwo8bppFrontBack(img, img2);
                            }
                            else
                                img = MergeTwo32ArgbFrontBack(img, img2);
                        }
                        int width = img.Width;
                        int Backheight = height;
                        height = img.Height;
                        for (int i = 0; i < img.Height; i++)
                        {
                            for (int j = 0; j < img.Width; j++)
                            {
                                Color pixel = img.GetPixel(j, i);
                                for (k = 0; k < lenght; k++)
                                {
                                    u = 0;
                                    if ((((pixel.R / 8) & 31) == ((Maxcolors[k].R / 8) & 31)) && (((pixel.G / 8) & 31) == ((Maxcolors[k].G / 8) & 31)) && (((pixel.B / 8) & 31) == ((Maxcolors[k].B / 8) & 31)) && (pixel.A == Maxcolors[k].A))//Avoid colors that would be identical, but do not consider the first colour.
                                    {
                                        u = 1;
                                        Times[k] += 1;
                                        k = lenght;
                                    }
                                }
                                if (u == 0)
                                {
                                    lenght += 1;
                                    Times.Add(0);
                                    Maxcolors.Add(pixel);
                                }
                            }
                        }
                        List<int> SortedTimes = Times.OrderByDescending(o => o).ToList();
                        for (k = 0; k < lenght; k++)
                        {
                            for (u = 0; u < lenght; u++)
                            {
                                if ((SortedTimes[k] == Times[u]) && (!Connections.Exists(x => x == u)))
                                {
                                    Connections.Add(u);
                                    u = lenght;
                                }
                            }
                        }
                        if (Connections[0] != 0)
                        {
                            if (Connections.IndexOf(0) <= 15)
                                Connections[Connections.IndexOf(0)] = Connections[0];
                            else
                                Connections[15] = Connections[0];
                            Connections[0] = 0;
                        }
                        u = 0;
                        List<Color> FinalColors = new List<Color>();
                        for (k = 0; k < lenght; k++)
                        {
                            FinalColors.Add(Maxcolors[Connections[k]]);
                        }
                        Maxcolors.Clear();
                        Connections.Clear();
                        Times.Clear();
                        SortedTimes.Clear();
                        u = 0;
                        if (FinalColors.Exists(x => x.A <= 128))
                        {
                            while (u == 0)
                            {
                                if ((FinalColors.FindIndex(x => x.A <= 128)) != (FinalColors.FindLastIndex(x => x.A <= 128)))
                                {
                                    FinalColors.RemoveAt(FinalColors.FindLastIndex(x => x.A <= 128));
                                }
                                else
                                    u = 1;
                            }
                        }
                        while ((width % 8) != 0)
                            width += 1;
                        while ((height % 8) != 0)
                            height += 1;
                        int tileheight = height / 8;
                        int tilewidth = width / 8;
                        if ((tileheight * tilewidth) > 1024)
                        {
                            Console.WriteLine("Error! Image is too big!");
                            return;
                        }
                        if (FinalColors.Count > 16)
                            FinalColors.RemoveRange(16, FinalColors.Count - 16);
                        else
                            while (FinalColors.Count < 16)
                                FinalColors.Add(Color.Black);
                        List<Byte> Realhex = new List<Byte>();
                        for (k = 0; k < FinalColors.Count; k++)
                        {
                            int B = ((FinalColors[k].B >> 3) & 31) << 10;
                            int G = ((FinalColors[k].G >> 3) & 31) << 5;
                            int R = ((FinalColors[k].R >> 3) & 31);
                            Realhex.Add((byte)((B + G + R) & 0xFF));
                            Realhex.Add((byte)(((B + G + R) >> 8) & 0xFF));
                        }
                        while (Realhex.Count < 32)
                            Realhex.Add(0);
                        byte[] PALette = Realhex.ToArray();
                        Realhex = new List<Byte>();
                        byte rest;
                        for (u = 0; u < tileheight; u++)
                        {
                            for (k = 0; k < tilewidth; k++)
                            {
                                for (int j = 0; j < 8; j++)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        rest = 0;
                                        if (((u * 8) + j) < img.Height)
                                        {
                                            if ((8 * k) + (i * 2) < img.Width)
                                            {
                                                Color pixel = img.GetPixel((8 * k) + (i * 2), (u * 8) + j);
                                                if (HasColourInside(FinalColors.ToArray(), pixel) != -1)
                                                {
                                                    rest += (Byte)((HasColourInside(FinalColors.ToArray(), pixel)));
                                                }
                                                else
                                                {
                                                    int[] Similar = new int[FinalColors.Count];
                                                    for (int d = 0; d < FinalColors.Count; d++)
                                                    {
                                                        Similar[d] = 0;
                                                        if (FinalColors[d].A >= pixel.A)
                                                            Similar[d] += 1000 * (FinalColors[d].A - pixel.A);
                                                        else
                                                            Similar[d] += 1000 * (pixel.A - FinalColors[d].A);
                                                        if (FinalColors[d].B >= pixel.B)
                                                            Similar[d] += FinalColors[d].B - pixel.B;
                                                        else
                                                            Similar[d] += pixel.B - FinalColors[d].B;
                                                        if (FinalColors[d].G >= pixel.G)
                                                            Similar[d] += FinalColors[d].G - pixel.G;
                                                        else
                                                            Similar[d] += pixel.G - FinalColors[d].G;
                                                        if (FinalColors[d].R >= pixel.R)
                                                            Similar[d] += FinalColors[d].R - pixel.R;
                                                        else
                                                            Similar[d] += pixel.R - FinalColors[d].R;
                                                    }
                                                    int e = 1000000000;
                                                    int MostSimilar = 0;
                                                    for (int d = 0; d < FinalColors.Count; d++)
                                                    {
                                                        if (Similar[d] <= e)
                                                        {
                                                            e = Similar[d];
                                                            MostSimilar = d;
                                                        }
                                                    }
                                                    rest = (Byte)(MostSimilar);
                                                }
                                            }
                                        }
                                        if (((u * 8) + j) < img.Height)
                                        {
                                            if (((8 * k) + (i * 2) + 1) < img.Width)
                                            {
                                                Color pixel = img.GetPixel((8 * k) + (i * 2) + 1, (u * 8) + j);
                                                if (HasColourInside(FinalColors.ToArray(), pixel) != -1)
                                                {
                                                    rest += (Byte)((HasColourInside(FinalColors.ToArray(), pixel)) * 16);
                                                }
                                                else
                                                {
                                                    int[] Similar = new int[FinalColors.Count];
                                                    for (int d = 0; d < FinalColors.Count; d++)
                                                    {
                                                        Similar[d] = 0;
                                                        if (FinalColors[d].A >= pixel.A)
                                                            Similar[d] += 1000 * (FinalColors[d].A - pixel.A);
                                                        else
                                                            Similar[d] += 1000 * (pixel.A - FinalColors[d].A);
                                                        if (FinalColors[d].B >= pixel.B)
                                                            Similar[d] += FinalColors[d].B - pixel.B;
                                                        else
                                                            Similar[d] += pixel.B - FinalColors[d].B;
                                                        if (FinalColors[d].G >= pixel.G)
                                                            Similar[d] += FinalColors[d].G - pixel.G;
                                                        else
                                                            Similar[d] += pixel.G - FinalColors[d].G;
                                                        if (FinalColors[d].R >= pixel.R)
                                                            Similar[d] += FinalColors[d].R - pixel.R;
                                                        else
                                                            Similar[d] += pixel.R - FinalColors[d].R;
                                                    }
                                                    int e = 1000000000;
                                                    int MostSimilar = 0;
                                                    for (int d = 0; d < FinalColors.Count; d++)
                                                    {
                                                        if (Similar[d] <= e)
                                                        {
                                                            e = Similar[d];
                                                            MostSimilar = d;
                                                        }
                                                    }
                                                    rest += (Byte)(MostSimilar * 16);
                                                }
                                            }
                                        }
                                        Realhex.Add(rest);
                                    }
                                }

                            }
                        }
                        FinalColors.Clear();
                        for (k = 0; k <= 31; k++)
                        {
                            Realhex.Add(255);//Prepare the image for CCG compression.
                        }
                        Byte[] Image = Realhex.ToArray();
                        Byte[,] Tile = new Byte[((Image.Count()) / 32), 32];
                        for (u = 0; u < ((Image.Count()) / 32); u++)
                        {
                            for (int j = 0; j < 32; j++)
                            {
                                Tile[u, j] = Image[(u * 32) + j];
                                //if(Tile[u,j]<=15)
                                //Console.Write("0"); Testing purpose.
                                //Console.Write("{0:X}|",Tile[u, j]);
                            }
                            //Console.WriteLine("");
                        }
                        byte[,] NewTile = Tile;
                        if (tileheight > 1)
                            NewTile = RemoveExceedingTiles(Tile, tilewidth, tileheight, ref Backheight, out tilewidth, out tileheight);
                        List<List<int>> Reverse = new List<List<int>>();
                        List<int> ReverseCorrespond = new List<int>();
                        List<List<int>> Same = new List<List<int>>();
                        List<int> SameCorrespond = new List<int>();
                        List<List<int>> HFlip = new List<List<int>>();
                        List<int> HFlipCorrespond = new List<int>();
                        List<List<int>> VFlip = new List<List<int>>();
                        List<int> VFlipCorrespond = new List<int>();
                        for (u = 0; u < (tileheight * tilewidth); u++)
                        {
                            for (k = u + 1; k < (tileheight * tilewidth); k++)
                            {
                                int Result = Comparison(NewTile, u, k);

                                if (Result >= 1000)
                                {
                                    if (!ReverseCorrespond.Exists(x => x == k))
                                    {
                                        ReverseCorrespond.Add(k);
                                        Reverse.Add(new List<int>());
                                    }
                                    Reverse[ReverseCorrespond.IndexOf(k)].Add(u);
                                    Result -= 1000;
                                }
                                if (Result >= 100)
                                {
                                    if (!VFlipCorrespond.Exists(x => x == k))
                                    {
                                        VFlipCorrespond.Add(k);
                                        VFlip.Add(new List<int>());
                                    }
                                    VFlip[VFlipCorrespond.IndexOf(k)].Add(u);
                                    Result -= 100;
                                }
                                if (Result >= 10)
                                {
                                    if (!HFlipCorrespond.Exists(x => x == k))
                                    {
                                        HFlipCorrespond.Add(k);
                                        HFlip.Add(new List<int>());
                                    }
                                    HFlip[HFlipCorrespond.IndexOf(k)].Add(u);
                                    Result -= 10;
                                }
                                if (Result == 1)
                                {
                                    if (!SameCorrespond.Exists(x => x == k))
                                    {
                                        SameCorrespond.Add(k);
                                        Same.Add(new List<int>());
                                    }
                                    Same[SameCorrespond.IndexOf(k)].Add(u);
                                    Result -= 1;
                                }
                            }
                        }
                        Byte[,] Tileback, OAMTile = new Byte[1, 1];
                        Byte[] OAMFront, OAMBack, SOB = new Byte[1];
                        int tilewidth1 = tilewidth;
                        int tileheight1 = 0;
                        int tilewidth2 = tilewidth;
                        int tileheight2 = 0;
                        int tileheight1temp = tileheight;
                        int tileheight2temp = tileheight;
                        int Tiletemph = tileheight;
                        int Tiletempw = tilewidth;
                        if ((Back == 1))
                        {
                            NewTile = SeparateBack(NewTile, tilewidth, tileheight, Back, Backheight, out tileheight1, out tileheight2, out Tileback);
                            NewTile = RemoveExceedingTiles(NewTile, tilewidth, tileheight1, ref Backheight, out tilewidth1, out tileheight1);
                            if (tileheight2 > 0)
                            {
                                Tileback = RemoveExceedingTiles(Tileback, tilewidth2, tileheight2, ref Backheight, out tilewidth2, out tileheight2);
                                tileheight1temp = tileheight1;
                                tileheight2temp = tileheight2;
                                OAMTile = TileOAM(NewTile, tileheight1, tilewidth1, out tileheight1, out tilewidth1, out OAMFront);
                                Tileback = TileOAM(Tileback, tileheight2, tilewidth2, out tileheight2, out tilewidth2, out OAMBack);
                                OAMTile = UniteTile(OAMTile, Tileback, Back, tilewidth1, tileheight1, tilewidth2, tileheight2, ref OAMFront, ref OAMBack, out tilewidth, out tileheight);
                                SOB = SOBGen(OAMFront, OAMBack, tileheight1, tilewidth1, tilewidth2, Back);
                            }
                            else Back = 0;
                        }
                        if (Back == 0)
                        {
                            OAMTile = TileOAM(NewTile, tileheight, tilewidth, out tileheight, out tilewidth, out OAMFront);
                            OAMBack = new Byte[0];
                            SOB = SOBGen(OAMFront, OAMBack, tileheight, tilewidth, tilewidth2, Back);
                        }
                        OAMTile = Finalize(OAMTile, ref SOB, tilewidth);
                        /*
                        int j, p;
                        for (u = 0; u < (tileheight * tilewidth); u++)
                        {
                            for (k = u + 1; k < (tileheight * tilewidth); k++)
                            {
                                int f = k;
                                for (p = 0; p < 8; p++)
                                {
                                    for (j = 0; j < 8; j++)
                                    {
                                        if (ReverseCorrespond.Exists(x => x == f+ j))
                                        {
                                            if (Reverse[ReverseCorrespond.IndexOf(f + j)].Exists(x => x == u + j))
                                            {
                                                f += 1;
                                            }
                                        }
                                        else {
                                            j += 19;
                                            p += 19;
                                        }
                                    }
                                    f = k + (tilewidth * p);

                                }
                            }
                        }*/
                        Image = new Byte[OAMTile.Length + 32];
                        for (u = 0; u < OAMTile.Length / 32; u++)
                        {
                            for (k = 0; k < 32; k++)
                                Image[(u * 32) + k] = OAMTile[u, k];
                        }
                        for (k = 0; k <= 31; k++)
                        {
                            Image[OAMTile.Length + k] = 255;//Prepare the image for CCG compression.
                        }
                        int Tiletemp = Image.Count() / 32;
                        Image = GBA.LZ77.Compress(Image);
                        byte[] CCG = new byte[Image.Count() + 16];
                        CCG[0] = 0x63;
                        CCG[1] = 0x63;
                        CCG[2] = 0x67;
                        CCG[3] = 0x20;
                        CCG[4] = 0x2;
                        CCG[5] = 0;
                        CCG[6] = 0;
                        CCG[7] = 0;
                        CCG[8] = ((byte)(Tiletemp & 0xFF));
                        CCG[9] = ((byte)((Tiletemp >> 8) & 0xFF));
                        CCG[10] = ((byte)((Tiletemp >> 16) & 0xFF));
                        CCG[11] = ((byte)((Tiletemp >> 24) & 0xFF));
                        for (int o = 0; o < Image.Count(); o++)
                            CCG[o + 12] = Image[o];
                        CCG[Image.Count() + 12] = 0x7E;
                        CCG[Image.Count() + 13] = 0x63;
                        CCG[Image.Count() + 14] = 0x63;
                        CCG[Image.Count() + 15] = 0x67;
                        int Enemytable = 0xD0D28; //If it has a back sprite, make it turnable in battle and in memo. I'll leave this commented since this should be in the EnemyData.
                                                  /*if (Back >= 1)
                                                  {
                                                      memblock[Enemytable + (Enemynum * 0x90) + 0x74] = 1;
                                                      memblock[Enemytable + (Enemynum * 0x90) + 0x75] = 1;
                                                  }
                                                  else
                                                  {
                                                      memblock[Enemytable + (Enemynum * 0x90) + 0x74] = 0;
                                                      memblock[Enemytable + (Enemynum * 0x90) + 0x75] = 0;
                                                  }*/
                        memblock[Enemytable + (Enemynum * 0x90) + 0x6C] = 1;//Fix the heights, so we avoid problems.
                                                                            //I suggest making an algorythm that edits the value at "0xC6D62 + (Enemynum * 2)" based on the number of initial breaks in Memo entries. This value will be then used by my algorythm to make everything look pretty.
                        if (memblock[0xC6D62 + (Enemynum * 2)] >= 128)
                        {
                            memblock[Enemytable + (Enemynum * 0x90) + 0x70] = (byte)(36 - ((tileheight1temp << 3) / 2) + (256 - ((256 - memblock[0xC6D62 + (Enemynum * 2)]) / 2)));
                            memblock[Enemytable + (Enemynum * 0x90) + 0x71] = (byte)(36 - ((tileheight2temp << 3) / 2) + (256 - ((256 - memblock[0xC6D62 + (Enemynum * 2)]) / 2)));
                        }
                        else
                        {
                            memblock[Enemytable + (Enemynum * 0x90) + 0x70] = (byte)(36 - ((tileheight1temp << 3) / 2) + (memblock[0xC6D62 + (Enemynum * 2)] / 2));
                            memblock[Enemytable + (Enemynum * 0x90) + 0x71] = (byte)(36 - ((tileheight2temp << 3) / 2) + (memblock[0xC6D62 + (Enemynum * 2)] / 2));
                        }
                        if (tileheight1temp >= 12)
                            memblock[Enemytable + (Enemynum * 0x90) + 0x72] = (byte)(256 - (((tileheight1temp - 12) << 3) / 2));
                        else
                            memblock[Enemytable + (Enemynum * 0x90) + 0x72] = 0;
                        if (tileheight2temp >= 12)
                            memblock[Enemytable + (Enemynum * 0x90) + 0x73] = (byte)(256 - (((tileheight2temp - 12) << 3) / 2));
                        else
                            memblock[Enemytable + (Enemynum * 0x90) + 0x73] = 0;
                        FinalProducts a = new FinalProducts();
                        a.CCG = CCG;
                        a.SOB = SOB;
                        a.Palette = PALette;
                        End.Add(a);
                    }
                    else
                    {
                        if (Num != 0)
                        {
                            FinalProducts a = new FinalProducts();
                            a.CCG = End[0].CCG;
                            a.SOB = End[0].SOB;
                            a.Palette = End[0].Palette;
                            End.Add(a);
                        }
                        else
                        {
                            FinalProducts a = new FinalProducts();
                            byte[] u = new byte[32];
                            for (int h = 0; h < 32; h++)
                                u[h] = (byte)0;
                            byte[] i = GBA.LZ77.Compress(u);
                            byte[] CCG = new byte[i.Length + 16];
                            CCG[0] = 0x63;
                            CCG[1] = 0x63;
                            CCG[2] = 0x67;
                            CCG[3] = 0x20;
                            CCG[4] = 0x2;
                            CCG[5] = 0;
                            CCG[6] = 0;
                            CCG[7] = 0;
                            CCG[8] = 1;
                            CCG[9] = 0;
                            CCG[10] = 0;
                            CCG[11] = 0;
                            for (int o = 0; o < i.Length; o++)
                                CCG[o + 12] = i[o];
                            CCG[i.Length + 12] = 0x7E;
                            CCG[i.Length + 13] = 0x63;
                            CCG[i.Length + 14] = 0x63;
                            CCG[i.Length + 15] = 0x67;
                            a.CCG = CCG;
                            List<byte> SOB = new List<byte>();
                            SOB.Add(0x73);
                            SOB.Add(0x6F);
                            SOB.Add(0x62);
                            SOB.Add(0x20);
                            SOB.Add(2);
                            SOB.Add(0);
                            SOB.Add(2);
                            SOB.Add(0);
                            SOB.Add(0x10);
                            SOB.Add(0);
                            SOB.Add(0x10);
                            SOB.Add(0);
                            SOB.Add((Byte)(0x14 + 8));
                            SOB.Add(0);
                            SOB.Add((Byte)(0x14 + 8));
                            SOB.Add(0);
                            SOB.Add(1);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(4);
                            SOB.Add(0);
                            SOB.Add(1);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0);
                            SOB.Add(0x7E);
                            SOB.Add(0x73);
                            SOB.Add(0x6F);
                            SOB.Add(0x62);
                            a.SOB = SOB.ToArray();
                            a.Palette = u;
                            End.Add(a);
                        }
                    }
                }
            }
            for (int Enemynum = 0; Enemynum <= 256; Enemynum++)
            {
                progress?.Report(new ProgressPercent("Inserting Enemy " + Enemynum + "'s Graphics",
                    ((Enemynum * 100f) / 257)));
                int BaseSOB = FreeSpace.Pointers.BaseSOB + (Enemynum * 8);
                int BaseCCG = FreeSpace.Pointers.BaseCCG + (Enemynum * 8);
                int BasePAL = FreeSpace.Pointers.BasePAL + (Enemynum * 8);
                End[Enemynum].PointerCCG = BaseCCG;
                End[Enemynum].PointerSOB = BaseSOB;
                End[Enemynum].PointerPAL = BasePAL;
                End[Enemynum].ToCCG = Enemynum;
                End[Enemynum].ToSOB = Enemynum;
                int flag1 = 0, flag2 = 0;
                for (int i = 0; i < Enemynum; i++)
                {
                    if ((AreArraySame(End[Enemynum].CCG, End[i].CCG)) && (flag1 == 0))
                    {
                        End[Enemynum].ToCCG = i;
                        flag1 = 1;
                    }
                    if ((AreArraySame(End[Enemynum].SOB, End[i].SOB)) && (flag2 == 0))
                    {
                        flag2 = 1;
                        End[Enemynum].ToSOB = i;
                    }
                }
            }
            for (int Enemynum = 0; Enemynum <= 256; Enemynum++)
            {
                if (End[Enemynum].ToCCG == Enemynum)
                {
                    End[Enemynum].AddressCCG = LastCCG;
                    LastCCG = InsertPointer(memblock, End[Enemynum].CCG, End[Enemynum].PointerCCG, LastCCG);
                }
                else
                {
                    End[Enemynum].AddressCCG = End[End[Enemynum].ToCCG].AddressCCG;
                    InsertOldPointer(memblock, End[Enemynum].PointerCCG, End[End[Enemynum].ToCCG].AddressCCG, End[Enemynum].CCG);
                }
                if (End[Enemynum].ToSOB == Enemynum)
                {
                    End[Enemynum].AddressSOB = LastSOB;
                    LastSOB = InsertPointer(memblock, End[Enemynum].SOB, End[Enemynum].PointerSOB, LastSOB);
                }
                else
                {
                    End[Enemynum].AddressSOB = End[End[Enemynum].ToSOB].AddressSOB;
                    InsertOldPointer(memblock, End[Enemynum].PointerSOB, End[End[Enemynum].ToSOB].AddressSOB, End[Enemynum].SOB);
                }
                int OffsetPAL = memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8)] + (memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8) + 1] << 8) + (memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8) + 2] << 16) + (memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8) + 3] << 24) + FreeSpace.Pointers.Base;//Palette
                for (int i = 0; i < 32; i++)
                    memblock[OffsetPAL + i] = (byte)(End[Enemynum].Palette[i] & 0xFF);
            }
            alpha.CopyFrom(memblock);
        }
    }
}