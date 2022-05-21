namespace ContourDetection;

using System.Text;
using Image;

public struct AmplitudeImage
{

    private uint[] Amplitudes;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public AmplitudeImage(CBitmap bitmap, Func<RGBA, uint> ampResolution)
    {
        this.Height = bitmap.AbsHeight;
        this.Width = bitmap.AbsWidth;
        this.Amplitudes = new uint[bitmap.Pixel.Length];
        for (var i = 0; i < bitmap.Pixel.Length; i++)
        {
            this.Amplitudes[i] = ampResolution(bitmap.Pixel[i]);
        }
    }

    public uint this[int index]
    {
        get => this.Amplitudes[index];
        set => this.Amplitudes[index] = value;
    }

    public AmplitudeImage(CBitmap bitmap) : this(bitmap, AmplitudeContourDetection.UniversialRgbToAmp)
    {
    }

    public AmplitudeImage(int width, int height)
    {
        this.Height = height;
        this.Width = width;
        this.Amplitudes = new uint[width * height];
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < this.Amplitudes.Length; i++)
        {
            sb.Append($"{this.Amplitudes[i]:X8} ");
            if (i % this.Width == 0)
            {
                sb.Append("\n");
            }
        }
        return sb.ToString();
    }
}

public struct Conture {

}

public static class AmplitudeContourDetection
{
    public static uint UniversialRgbToAmp(RGBA pixel)
    {
        var red = pixel.Red << 16;
        var blue = pixel.Blue << 8;
        var green = pixel.Green;
        return Convert.ToUInt32(red + blue + green);
    }

    public static AmplitudeImage AsAmplitudeImage(this CBitmap bitmap)
    {
        return new AmplitudeImage(bitmap);
    }

    public static void Detect(CBitmap bitmap, int threshold)
    {
        var eventMap = new AmplitudeImage(bitmap.AbsWidth, bitmap.AbsHeight);
        var amps = bitmap.AsAmplitudeImage();
        var contures = ParallelMarkThresholdContures(amps, threshold, eventMap);
        ApplyEventMapToBitmap(eventMap, bitmap);
    }

    public static Conture[] FindContures(List<(int X, int Y)> pixel) {
        return Array.Empty<Conture>();
    }

    public static List<(int X, int Y)> ParallelMarkThresholdContures(AmplitudeImage image, int threshold, AmplitudeImage eventMap)
    {
        var eventCoordinates = new List<(int X, int Y)>();
        Parallel.For(0, image.Height, (index) =>
        {
            var contureStarted = false;
            for (int j = 0; j < image.Width; j++)
            {
                var ampImdex = (index * image.Width) + j;
                if (image[ampImdex] >= threshold)
                {
                    if (contureStarted)
                    {
                        eventMap[ampImdex] = 0;
                    }
                    else
                    {
                        eventMap[ampImdex] = 1;
                        eventCoordinates.Add((j, index));
                        contureStarted = true;
                    }
                }
                else if (contureStarted)
                {
                    eventCoordinates.Add((j, index));
                    eventMap[ampImdex - 1] = 1;
                    contureStarted = false;
                }
            }
        });
        return eventCoordinates;
    }

    public static void ApplyEventMapToBitmap(AmplitudeImage eventMap, CBitmap bitmap)
    {
        for (var i = 0; i < eventMap.Height; i++)
        {
            var pixelFound = 0;
            for (int j = 0; j < eventMap.Width; j++)
            {
                var ampImdex = (i * eventMap.Width) + j;
                if (eventMap[ampImdex] > 0)
                {
                    pixelFound++;
                    bitmap.Pixel[ampImdex].SetWhite();
                }
            }
        };
    }
}