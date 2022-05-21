using System.Runtime.InteropServices;
using System.Text;

namespace Image
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BmpFileHeader
    {
        public ushort BfType;
        public uint BfSize;
        public uint BfReserved;
        public uint BfOffBits;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Type:\t\t {this.BfType}, 0x{this.BfType:X2}\n");
            sb.Append($"Size:\t\t {this.BfSize}, 0x{this.BfSize:X2}\n");
            sb.Append($"Reserved:\t {this.BfReserved}, 0x{this.BfReserved:X2}\n");
            sb.Append($"Offset:\t\t {this.BfOffBits}, 0x{this.BfOffBits:X2}\n");
            return sb.ToString();
        }
    }
    public static class Consts
    {
        public static unsafe int BmpFileHeaderSize = sizeof(BmpFileHeader);
        public static unsafe int BmpFileInfoSize = sizeof(BmpFileInfo);
        public static unsafe int RGBASize = sizeof(RGBA);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BmpFileInfo
    {
        public uint BiSize;
        public int BiWidth;
        public int BiHeight;
        public ushort BiPlanes;
        public ushort BiBitCount;
        public uint BiCompression;
        public uint BiSizeImage;
        public int BiXPelsPerMeter;
        public int BiYPelsPerMeter;
        public uint BiCrlUsed;
        public uint BiClrImportant;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Size:\t\t {this.BiSize}, 0x{this.BiSize:X2}\n");
            sb.Append($"Width:\t\t {this.BiWidth}, 0x{this.BiWidth:X2}\n");
            sb.Append($"Height:\t\t {this.BiHeight}, 0x{this.BiHeight:X2}\n");
            sb.Append($"Planes:\t\t {this.BiPlanes}, 0x{this.BiPlanes:X2}\n");
            sb.Append($"BitCount:\t {this.BiBitCount}, 0x{this.BiBitCount:X2}\n");
            sb.Append($"Compression:\t {this.BiCompression}, 0x{this.BiCompression:X2}\n");
            sb.Append($"ImageSize:\t {this.BiSizeImage}, 0x{this.BiSizeImage:X2}\n");
            sb.Append($"XPels:\t\t {this.BiXPelsPerMeter}, 0x{this.BiXPelsPerMeter:X2}\n");
            sb.Append($"YPels:\t\t {this.BiYPelsPerMeter}, 0x{this.BiYPelsPerMeter:X2}\n");
            sb.Append($"ColorTable:\t {this.BiCrlUsed}, 0x{this.BiCrlUsed:X2}\n");
            sb.Append($"Import:\t\t {this.BiClrImportant}, 0x{this.BiClrImportant:X2}\n");
            return sb.ToString();
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RGBA
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;

        public override string ToString()
        {
            return $"r:{this.Red:X2}, g:{this.Green:X2}, b:{this.Blue:X2}, a:{this.Alpha:X2}";
        }

        public void SetWhite()
        {
            this.Red = byte.MaxValue;
            this.Blue = byte.MaxValue;
            this.Green = byte.MaxValue;
        }
    }

    public readonly ref struct CBitmap
    {
        public readonly BmpFileHeader Head;

        public readonly BmpFileInfo Info;

        public readonly RGBA[] Pixel;

        public int AbsWidth => Math.Abs(this.Info.BiWidth);

        public int AbsHeight => Math.Abs(this.Info.BiHeight);

        public CBitmap(BmpFileHeader header, BmpFileInfo info, RGBA[] rgba)
        {
            this.Head = header;
            this.Info = info;
            this.Pixel = rgba;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var p in this.Pixel)
            {
                sb.AppendLine(p.ToString());
            }
            return sb.ToString();
        }

        public unsafe void Save(string path)
        {
            Span<BmpFileHeader> head = stackalloc BmpFileHeader[1];
            head[0] = this.Head;
            var headBytes = MemoryMarshal.AsBytes<BmpFileHeader>(head);
            Span<BmpFileInfo> info = stackalloc BmpFileInfo[1];
            info[0] = this.Info;
            var infoBytes = MemoryMarshal.AsBytes<BmpFileInfo>(info);
            Span<RGBA> rgb = stackalloc RGBA[this.Pixel.Length];
            for (int i = 0; i < this.Pixel.Length; i++)
            {
                rgb[i] = this.Pixel[i];
            }
            var rgbBytes = MemoryMarshal.AsBytes<RGBA>(rgb);
            Span<byte> bytes = stackalloc byte[(int)this.Head.BfSize];
            headBytes.CopyTo(bytes.Slice(0));
            infoBytes.CopyTo(bytes.Slice(Consts.BmpFileHeaderSize));
            rgbBytes.CopyTo(bytes.Slice((int)this.Head.BfOffBits));
            File.WriteAllBytes(path, bytes.ToArray());
        }
    }
}
