﻿using System;
namespace DataHandlers
{
    public static class Constants
    {
        public const int SIZEOF_BIG = 124824;
        public const int SIZEOF_SMALL = 16;
    }

    [Serializable]
    public struct SomeData
    {
        internal Seperator Seperator { get; set; }
        internal Global Global { get; set; }
        internal AmpTof[] Content { get; set; }

        public SomeData(int contentCount)
        {
            this.Seperator = new Seperator { Part1 = 1111_0000_1111_0000_1111 };
            this.Global = new Global { Type = 0000_0011, SomeData = 0001_1001_0101_1111 };
            this.Content = new AmpTof[contentCount];
            for (int i = 0; i < contentCount; i++)
            {
                this.Content[i] = new AmpTof { Amp = int.MaxValue, ToF = int.MaxValue };
            }
        }

    }

    [Serializable]
    internal struct AmpTof
    {
        internal uint Amp { get; set; }
        internal uint ToF { get; set; }
    }

    [Serializable]
    internal struct Global
    {
        internal uint Type;
        internal ulong Length;
        internal ulong SomeData;
    }

    [Serializable]
    internal struct Seperator
    {
        public ulong Part1;
    }

    internal struct PlainStruct
    {
        uint A;
        long B;
        byte C;
        int D;
    }
}