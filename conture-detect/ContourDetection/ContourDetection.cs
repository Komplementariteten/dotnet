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

public class ContureCollection
{

    private int _width;
    private int _height;

    private List<Conture> _contures = new List<Conture>();

    private Conture _current;

    public int Length => this._contures.Count;

    public Conture this[int index] => this._contures[index];

    private (int X, int Y) GetCoordinates(int pixelIndex)
    {
        var x = pixelIndex % this._width;
        var y = (pixelIndex / this._width);
        return (x, y);
    }

    public void AddBorderPixel(int pixelIndex)
    {
        var (x, y) = GetCoordinates(pixelIndex);
        if (this._contures.Count == 0)
        {
            this._current = new Conture(x, y);
            this._contures.Add(this._current);
        }
        else if (!this._current.TryAddBorderPixel(x, y))
        {
            var contureFound = false;
            for (int i = 0; i < this._contures.Count; i++)
            {
                if (this._contures[i].TryAddBorderPixel(x, y))
                {
                    this._current = this._contures[i];
                    contureFound = true;
                }
            }

            if (!contureFound)
            {
                this._current = new Conture(x, y);
                this._contures.Add(this._current);
            }
        }
    }

    public void AddBodyPixel()
    {
        this._current.AddBodyPixel();
    }

    public ContureCollection(int width, int height)
    {
        this._height = height;
        this._width = width;
    }
}

public class Conture
{

    private (int X, int Y) _topLeft;
    private (int X, int Y) _bottomRight;

    public (int X, int Y) TopLeft => this._topLeft;
    public (int X, int Y) BottomRight => this._bottomRight;

    public int Area => this._areaSize;

    private int _areaSize;

    private int _range = 20;

    public void AddBorderPixel(int x, int y)
    {
        this._areaSize++;
        if (this._topLeft.X > x)
        {
            this._topLeft.X = x;
        }
        else if (this._bottomRight.X < x)
        {
            this._bottomRight.X = x;
        }

        if (this._topLeft.Y > y)
        {
            this._topLeft.Y = y;
        }
        else if (this._bottomRight.Y < y)
        {
            this._bottomRight.Y = y;
        }
    }

    public bool TryAddBorderPixel(int x, int y)
    {
        if (x < (this._topLeft.X - this._range))
        {
            return false;
        }
        if (x > (this._bottomRight.X + this._range))
        {
            return false;
        }
        if (y < (this._topLeft.Y - this._range))
        {
            return false;
        }
        if (y > (this._bottomRight.Y + this._range))
        {
            return false;
        }
        this.AddBorderPixel(x, y);
        return true;
    }

    public void AddBodyPixel()
    {
        this._areaSize++;
    }

    public Conture(int x, int y)
    {
        this._topLeft = (x, y);
        this._bottomRight = (x, y);
        this._areaSize = 1;
    }

    public Conture()
    {
        this._topLeft = (int.MaxValue, int.MaxValue);
        this._bottomRight = (int.MinValue, int.MinValue);
        this._areaSize = 0;
    }
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

    public static void Detect(CBitmap bitmap, int threshold, MarkType markType)
    {
        var eventMap = new AmplitudeImage(bitmap.AbsWidth, bitmap.AbsHeight);
        var amps = bitmap.AsAmplitudeImage();
        ParallelMarkThresholdContures(amps, threshold, eventMap);
        var contures = ApplyEventMapToBitmap(eventMap, bitmap, markType);
    }

    public static void ParallelMarkThresholdContures(AmplitudeImage image, int threshold, AmplitudeImage eventMap)
    {
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
                        eventMap[ampImdex] = 2;
                    }
                    else
                    {
                        eventMap[ampImdex] = 1;
                        contureStarted = true;
                    }
                }
                else if (contureStarted)
                {
                    eventMap[ampImdex - 1] = 1;
                    contureStarted = false;
                }
            }
        });
    }

    public static ContureCollection ApplyEventMapToBitmap(AmplitudeImage eventMap, CBitmap bitmap, MarkType markType)
    {
        var contures = new ContureCollection(eventMap.Width, eventMap.Height);
        for (var i = 0; i < eventMap.Height; i++)
        {
            for (int j = 0; j < eventMap.Width; j++)
            {
                var ampImdex = (i * eventMap.Width) + j;
                if (eventMap[ampImdex] == 1)
                {
                    contures.AddBorderPixel(ampImdex);
                    if (markType.HasFlag(MarkType.MarkConture)) bitmap.Pixel[ampImdex].SetColor(0xffff00);
                }
                else if (eventMap[ampImdex] == 2)
                {
                    contures.AddBodyPixel();
                }
            }
        };

        if (markType.HasFlag(MarkType.MarkCluster))
        {
            for (var i = 0; i < contures.Length; i++)
            {
                var conture = contures[i];
                bitmap.DrawHorizontalLine(conture.TopLeft.Y - 5, conture.TopLeft.X - 5, conture.BottomRight.X + 5, 0xffffff);
                bitmap.DrawHorizontalLine(conture.BottomRight.Y + 5, conture.TopLeft.X - 5, conture.BottomRight.X + 5, 0xffffff);
                bitmap.DrawVerticalLine(conture.TopLeft.X - 5, conture.TopLeft.Y - 5, conture.BottomRight.Y + 5, 0xffffff);
                bitmap.DrawVerticalLine(conture.BottomRight.X + 5, conture.TopLeft.Y - 5, conture.BottomRight.Y + 5, 0xffffff);
            }
        }

        return contures;
    }
}