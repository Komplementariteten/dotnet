using System;

namespace TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("NEW TESTRUN");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("LocalHost -----");
            var ls = new LocalSocketUnserializer();
            var (readen1, time2, time1) = ls.RunBigAsync();
            Console.WriteLine($"DATARATE - in {Math.Round(time1.TotalSeconds, 4)}s Readen {Math.Round(readen1, 2)} kB/s - Serialization took {Math.Round(time2.TotalSeconds, 4)} s");

            var (readen, stime, rtime) = ls.RunBig();
            Console.WriteLine($"DATARATE - in {Math.Round(rtime.TotalSeconds, 4)}s Readen {Math.Round(readen, 2)} kB/s - Serialization took {Math.Round(stime.TotalSeconds, 4)} s");

            Console.WriteLine("LocalHost -----");

            /* Console.WriteLine("FirstIP ------");
            var (readen1, writen1, time1) = TcpSocketRunner.RunBig();
            Console.WriteLine($"DATARATE - in {Math.Round(time1.TotalSeconds, 4)}s Readen {Math.Round(readen1, 2)} kB/s - Writen {Math.Round(writen1, 2)} kB/s");

            var (readen2, writen2, time2) = TcpSocketRunner.RunSmall();
            Console.WriteLine($"DATARATE - in {Math.Round(time2.TotalSeconds, 4)}s Readen {Math.Round(readen2, 2)} kB/s - Writen {Math.Round(writen2, 2)} kB/s");

            var (readen2, writen2, time2) = LocalSocketRunner.RunSmall();
            Console.WriteLine($"DATARATE - in {Math.Round(time2.TotalSeconds, 4)}s Readen {Math.Round(readen2, 2)} kB/s - Writen {Math.Round(writen2, 2)} kB/s");

            Console.WriteLine("UnixSocket ----");

            (readen1, writen1, time1) = UnixSocketRunner.RunBig();
            Console.WriteLine($"DATARATE - in {Math.Round(time1.TotalSeconds, 4)}s Readen {Math.Round(readen1, 2)} kB/s - Writen {Math.Round(writen1, 2)} kB/s");

            (readen2, writen2, time2) = UnixSocketRunner.RunSmall();
            Console.WriteLine($"DATARATE - in {Math.Round(time2.TotalSeconds, 4)}s Readen {Math.Round(readen2, 2)} kB/s - Writen {Math.Round(writen2, 2)} kB/s");
            Console.WriteLine("-----"); */


        }
    }
}
