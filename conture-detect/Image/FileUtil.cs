namespace Image {
    using System.Runtime.InteropServices;
    public static class FileUtil {

        public static bool TryLoadBmp(string path, out CBitmap bitmap) {
            if(DetectImageType(path) != ImageType.BMP) {
                bitmap = default;
                return false;
            }
            if(TryLoadFile(path, out ReadOnlySpan<byte> bytes)){
                bitmap = bytes.AsBmp();
                return true; 
            }
            bitmap = default;
            return false;
        }

        public static bool TryLoadFile(string path, out ReadOnlySpan<byte> bytes) {
            
            var fi = new FileInfo(path);

            if(fi.Exists) {
                try {
                    bytes = File.ReadAllBytes(path);
                    return true;
                } catch {
                    bytes = Array.Empty<byte>();
                    return false;
                }
            }
            
            bytes = Array.Empty<byte>();
            return false;
        }

        private static ImageType DetectImageType(string path) => path switch {
            var _ when path.EndsWith("png", true, null) => ImageType.PNG,
            var _ when path.EndsWith("bmp", true, null) => ImageType.BMP,
            _ => ImageType.UNKOWN 
        };

        public static CBitmap AsBmp(this ReadOnlySpan<byte> bytes) {
            var headerSlice = bytes.Slice(0, Consts.BmpFileHeaderSize);
            var header = MemoryMarshal.AsRef<BmpFileHeader>(headerSlice);
            var infoSlice = bytes.Slice(headerSlice.Length, Consts.BmpFileInfoSize);
            var info = MemoryMarshal.AsRef<BmpFileInfo>(infoSlice);

            if(info.BiBitCount != 32) {
                throw new NotSupportedException($"{info.BiBitCount} bit bmp not supported");
            }
            if(info.BiCrlUsed != 0) {
                throw new NotSupportedException($"Color table bmp not supported");
            }
            var rgbSlice = bytes.Slice((int)header.BfOffBits);
            var rgb = new List<RGBA>();
            while(rgbSlice.Length >= Consts.RGBASize) {
                var pixel = MemoryMarshal.AsRef<RGBA>(rgbSlice);
                rgb.Add(pixel);
                rgbSlice = rgbSlice.Slice(Consts.RGBASize);
            }

            return new CBitmap(header, info, rgb.ToArray());
        }       
    }
}