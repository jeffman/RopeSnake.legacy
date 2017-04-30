using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using RopeSnake.Core.Validation;
using RopeSnake.Core;

namespace Rendering
{
    class SinglePiece
    {
        public static BitmapData LockBits(Bitmap bmp, ImageLockMode imageLockMode)
        {
            return bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), imageLockMode, bmp.PixelFormat);
        }
        unsafe public static Bitmap SingleRender(GBA.OAM OAMEntry, GBA.PAL[] Palette, byte[] Image)
        {
            Bitmap Neo = new Bitmap(OAMEntry.Width, OAMEntry.Height, PixelFormat.Format8bppIndexed);
            GBA.PAL.CopyPalette(Neo, Palette);
            BitmapData N = LockBits(Neo, ImageLockMode.WriteOnly);
            byte* ptr = (byte*)N.Scan0;
            for (int y = 0; y < OAMEntry.Height / 8; y++)
                for (int u = 0; u < OAMEntry.Width / 8; u++)
                    for (int i = 0; i < 8; i++)
                        for (int j = 0; j < 4; j++)
                        {
                            if (((OAMEntry.Tile * 0x20) + j + (i * 4) + (u * 32) + (y * (OAMEntry.Width * 4))) < Image.Count())
                            {
                                ptr[((j * 2) + 1 + (u * 8) + (i * N.Stride) + ((y * N.Stride) * 8))] = (byte)((Image[((OAMEntry.Tile * 0x20) + j + (i * 4) + (u * 32) + (y * (OAMEntry.Width * 4)))] >> 4) & 0xF);
                                ptr[((j * 2) + (u * 8) + (i * N.Stride) + ((y * N.Stride) * 8))] = (byte)((Image[(((OAMEntry.Tile * 0x20) + j + (i * 4) + (u * 32) + (y * (OAMEntry.Width * 4))))]) & 0xF);
                            }
                        }
            Neo.UnlockBits(N);
            if (OAMEntry.Flips == 1)
            {
                Neo.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            else if (OAMEntry.Flips == 2)
            {
                Neo.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            else if (OAMEntry.Flips == 3)
            {
                Neo.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            }
            return Neo;
        }
    }
    class Compose
    {
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
        public static Bitmap MergeTwo32Argb(Bitmap firstImage, Bitmap secondImage, int xsecond, int ysecond)
        {
            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }
            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }
            int outputImageWidth = firstImage.Width;
            int outputImageHeight = firstImage.Height;
            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(firstImage, new Rectangle(new Point(), firstImage.Size),
                    new Rectangle(new Point(), firstImage.Size), GraphicsUnit.Pixel);
                graphics.DrawImage(secondImage, new Rectangle(new Point(xsecond, ysecond), secondImage.Size),
                    new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
            }
            return outputImage;
        }
        unsafe static bool CheckSame(Bitmap firstImage, Bitmap secondImage)
        {
            for (int i = 0; i < Math.Min(firstImage.Height, secondImage.Height); i++)
                for (int j = 0; j < Math.Min(firstImage.Width, secondImage.Width); j++)
                    if (firstImage.GetPixel(j, i) != secondImage.GetPixel(j, i))
                        return false;
            return true;
        }
        unsafe static Bitmap MergeTwo8bpp(Bitmap firstImage, Bitmap secondImage, int xsecond, int ysecond)
        {
            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }
            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }
            Bitmap outputImage = firstImage;
            BitmapData N = SinglePiece.LockBits(outputImage, ImageLockMode.WriteOnly);
            byte* ptr = (byte*)N.Scan0;
            BitmapData N2 = SinglePiece.LockBits(secondImage, ImageLockMode.ReadOnly);
            byte* ptr2 = (byte*)N2.Scan0;
            for (int i = 0; i < N2.Height; i++)
            {
                for (int y = 0; y < N2.Width; y++)
                {
                    if (ptr2[y + (i * N2.Stride)] != 0)
                        ptr[y + (i * N.Stride) + xsecond + (ysecond * N.Stride)] = ptr2[y + (i * N2.Stride)];
                }
            }
            outputImage.UnlockBits(N);
            secondImage.UnlockBits(N2);
            return outputImage;
        }
        static Bitmap CreateRender(List<GBA.OAM> OAMEntry, GBA.PAL[] Palette, byte[] Image, int Wanted)
        {
            List<Bitmap> Tempo = new List<Bitmap>();
            List<int> X = new List<int>();
            List<int> Y = new List<int>();
            for (int i = 0; i < OAMEntry.Count; i++)
            {
                if (OAMEntry[i].Num == Wanted)
                {
                    Tempo.Add(SinglePiece.SingleRender(OAMEntry[i], Palette, Image));
                    X.Add(OAMEntry[i].X);
                    Y.Add(OAMEntry[i].Y);
                }
            }
            int minY, minX;
            if (Y.Count > 0)
                minY = Y.Min();
            else
                minY = 0;
            if (X.Count > 0)
                minX = X.Min();
            else
                minX = 0;
            for (int i = 0; i < X.Count; i++)
            {
                X[i] -= minX;
                Y[i] -= minY;
            }
            int MaxX = 0;
            int MaxY = 0;
            for (int i = 0; i < X.Count; i++)
            {
                if (MaxX < X[i] + Tempo[i].Width)
                    MaxX = Tempo[i].Width + X[i];
                if (MaxY < Y[i] + Tempo[i].Height)
                    MaxY = Tempo[i].Height + Y[i];
            }
            if ((MaxX == 0) || (MaxY == 0))
            {
                MaxX = 8; MaxY = 8;
            }
            Bitmap Temporale = new Bitmap(MaxX, MaxY, PixelFormat.Format8bppIndexed);
            GBA.PAL.CopyPalette(Temporale, Palette);
            for (int i = 0; i < X.Count; i++)
                Temporale = MergeTwo8bpp(Temporale, Tempo[i], X[i], Y[i]);
            return Temporale;
        }
        public static void Output(List<GBA.OAM> OAMEntry, GBA.PAL[] Palette, byte[] Image, int Num, string outputPath, IProgress<ProgressPercent> Progress)
        {
            string a = outputPath;
            string b = "";
            if (Num < 10)
                b += "00";
            else if (Num < 100)
            {
                b += "0";
                b += ConvertHexToChar(Num / 10);
            }
            else
            {
                b += ConvertHexToChar(Num / 100);
                b += ConvertHexToChar((Num / 10) % 10);
            }
            b += Num % 10;
            a += b;
            Bitmap Temp, Temp2;
            if ((OAMEntry.Exists(x => x.Num == 1)))
            {
                Temp2 = CreateRender(OAMEntry, Palette, Image, 1);
                Temp = CreateRender(OAMEntry, Palette, Image, 0);
                if ((CheckSame(Temp, Temp2)) == false)
                {
                    Progress?.Report(new ProgressPercent("Writing /BattleSprites/"+b+"Back.png",
                        ((Num * 100f) / 257)));
                    Temp2.Save(a + "Back.png");
                }
            }
            else
                Temp = CreateRender(OAMEntry, Palette, Image, 0);
            Progress?.Report(new ProgressPercent("Writing /BattleSprites/" +  b  + ".png",
                ((Num*100f) / 257)));
            Temp.Save(a + ".png");
        }
    }
}
namespace GBA
{
    public class OAM
    {
        public int Y, X, Width, Height, Flips, Tile;
        public int Num;
        public static List<OAM> OAMGet(byte[] data, int address)
        {
            List<OAM> OAM = new List<OAM>();
            int count = data[address + 4] + (data[address + 5] << 8);
            for (int i = 0; i < count; i++)
            {
                int Internal = address + (data[address + 8 + (i * 2)] + (data[address + 9 + (i * 2)] << 8));
                int num = data[Internal + 2] + (data[Internal + 3] << 8);
                for (int j = 0; j < num; j++)
                {
                    OAM Tuk = new OAM();
                    Tuk.Num = i;
                    Tuk.Y = data[Internal + 4 + (j * 8)];
                    if (Tuk.Y >= 0x80)
                        Tuk.Y -= 0x80;
                    else
                        Tuk.Y += 0x80;
                    Tuk.X = data[Internal + 6 + (j * 8)] + ((data[Internal + 7 + (j * 8)] & 0x1) << 8);
                    if (Tuk.X >= 0x100)
                        Tuk.X -= 0x100;
                    else
                        Tuk.X += 0x100;
                    int Shape = (data[Internal + 5 + (j * 8)] >> 6) & 0x3;
                    int Size = (data[Internal + 7 + (j * 8)] >> 6) & 0x3;
                    if (Shape == 0)
                    {
                        if (Size == 0)
                        {
                            Tuk.Width = 8;
                            Tuk.Height = Tuk.Width;
                        }
                        else if (Size == 1)
                        {
                            Tuk.Width = 16;
                            Tuk.Height = Tuk.Width;
                        }
                        else if (Size == 2)
                        {
                            Tuk.Width = 32;
                            Tuk.Height = Tuk.Width;
                        }
                        else if (Size == 3)
                        {
                            Tuk.Width = 64;
                            Tuk.Height = Tuk.Width;
                        }
                    }
                    else if (Shape == 1)
                    {
                        if (Size == 0)
                        {
                            Tuk.Width = 16;
                            Tuk.Height = Tuk.Width / 2;
                        }
                        else if (Size == 1)
                        {
                            Tuk.Width = 32;
                            Tuk.Height = Tuk.Width / 4;
                        }
                        else if (Size == 2)
                        {
                            Tuk.Width = 32;
                            Tuk.Height = Tuk.Width / 2;
                        }
                        else if (Size == 3)
                        {
                            Tuk.Width = 64;
                            Tuk.Height = Tuk.Width / 2;
                        }
                    }
                    else if (Shape == 2)
                    {
                        if (Size == 0)
                        {
                            Tuk.Width = 8;
                            Tuk.Height = Tuk.Width * 2;
                        }
                        else if (Size == 1)
                        {
                            Tuk.Width = 8;
                            Tuk.Height = Tuk.Width * 4;
                        }
                        else if (Size == 2)
                        {
                            Tuk.Width = 16;
                            Tuk.Height = Tuk.Width * 2;
                        }
                        else if (Size == 3)
                        {
                            Tuk.Width = 32;
                            Tuk.Height = Tuk.Width * 2;
                        }
                    }
                    else
                        break;
                    Tuk.Flips = (data[Internal + 7 + (j * 8)] >> 4) & 0x3;
                    Tuk.Tile = (data[Internal + 8 + (j * 8)]) + (((data[Internal + 9 + (j * 8)]) & 0x3) << 8);
                    OAM.Add(Tuk);
                }
            }
            return OAM;
        }
    }
    public class PAL
    {
        public Color Palette;
        public static void CopyPalette(Bitmap bmp, PAL[] pal)
        {
            ColorPalette cp = bmp.Palette;

            for (int i = 0; i < Math.Min(256, pal.Length); i++)
                cp.Entries[i] = pal[i].Palette;
            for (int i = Math.Min(256, pal.Length); i < 256; i++)
                cp.Entries[i] = Color.Black;

            bmp.Palette = cp;
        }
        public static GBA.PAL[] PALGet(byte[] data, int address)
        {
            GBA.PAL[] Transform = new GBA.PAL[16];
            for (int i = 0; i < 16; i++)
            {
                Transform[i] = new GBA.PAL();
                int Temp = data[address + (i * 2)] + (data[address + 1 + (i * 2)] << 8);
                byte R = (byte)((Temp & 31) << 3);
                byte G = (byte)(((Temp >> 5) & 31) << 3);
                byte B = (byte)(((Temp >> 10) & 31) << 3);
                byte Alpha = 255;
                if (i == 0)
                    Alpha = 0;
                Transform[i].Palette = Color.FromArgb(Alpha, R, G, B);
            }
            return Transform;
        }
    }
}
namespace RopeSnake.Mother3.Enemy_Graphics
{
    public class Extraction
    {
        public static int Base = 0x1C90960;
        public static int BaseSOB = 0x1C91E88;//Start of enemy SOB blocks pointers
        public static int BaseCCG = 0x1C909A8;//Start of enemy CCG blocks pointers
        public static int BasePAL = 0x1C91530;//Start of enemy palettes pointers
        public static void Extract(byte[] memblock, string outputPath, IProgress<ProgressPercent> Progress)
        {
            Directory.CreateDirectory(outputPath);
            for (int i = 0; i <= 256; i++)
            {
                int Enemynum = i;
                int PoiSOB = BaseSOB + (Enemynum * 8);
                int PoiCCG = BaseCCG + (Enemynum * 8);
                int PoiPAL = BasePAL + (Enemynum * 8);
                PoiSOB = memblock[PoiSOB] + (memblock[PoiSOB + 1] << 8) + (memblock[PoiSOB + 2] << 16) + (memblock[PoiSOB + 3] << 24) + Base;//SOB
                PoiCCG = memblock[PoiCCG] + (memblock[PoiCCG + 1] << 8) + (memblock[PoiCCG + 2] << 16) + (memblock[PoiCCG + 3] << 24) + Base;//CCG
                PoiPAL = memblock[PoiPAL] + (memblock[PoiPAL + 1] << 8) + (memblock[PoiPAL + 2] << 16) + (memblock[PoiPAL + 3] << 24) + Base;//Palette
                Byte[] Image;
                GBA.LZ77.Decompress(memblock, PoiCCG + 12, out Image);
                List<GBA.OAM> OAMEntries = GBA.OAM.OAMGet(memblock, PoiSOB);
                GBA.PAL[] Palette = GBA.PAL.PALGet(memblock, PoiPAL);
                Rendering.Compose.Output(OAMEntries, Palette, Image, Enemynum, outputPath, Progress);
            }
        }
    }
}
