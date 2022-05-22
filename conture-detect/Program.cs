// See https://aka.ms/new-console-template for more information
using ContourDetection;
using Image;

var path = "example.bmp";

if(args.Length > 0) {
    path = args[0] as string;
}

if(Image.FileUtil.TryLoadBmp(path, out CBitmap bitmap)) {
    Console.WriteLine($"Bmp Header");
    Console.WriteLine(bitmap.Head);
    Console.WriteLine($"Bmp Info");
    Console.WriteLine(bitmap.Info);
    AmplitudeContourDetection.Detect(bitmap, 0xFFF, MarkType.MarkConture | MarkType.MarkCluster);
    bitmap.Save("result.bmp");
} else {
    Console.WriteLine($"{path} is not a bmp-file");
} 