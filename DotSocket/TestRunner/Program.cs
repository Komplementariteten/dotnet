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
            Console.WriteLine("FirstIP ------");
            var (readen1, writen1, time1) = TcpSocketRunner.RunBig();
            Console.WriteLine($"DATARATE - in {Math.Round(time1.TotalSeconds, 4)}s Readen {Math.Round(readen1, 2)} kB/s - Writen {Math.Round(writen1, 2)} kB/s");

            var (readen2, writen2, time2) = TcpSocketRunner.RunSmall();
            Console.WriteLine($"DATARATE - in {Math.Round(time2.TotalSeconds, 4)}s Readen {Math.Round(readen2, 2)} kB/s - Writen {Math.Round(writen2, 2)} kB/s");

            Console.WriteLine("LocalHost -----");
            (readen1, writen1, time1) = LocalSocketRunner.RunBig();
            Console.WriteLine($"DATARATE - in {Math.Round(time1.TotalSeconds, 4)}s Readen {Math.Round(readen1, 2)} kB/s - Writen {Math.Round(writen1, 2)} kB/s");

            (readen2, writen2, time2) = LocalSocketRunner.RunSmall();
            Console.WriteLine($"DATARATE - in {Math.Round(time2.TotalSeconds, 4)}s Readen {Math.Round(readen2, 2)} kB/s - Writen {Math.Round(writen2, 2)} kB/s");

            Console.WriteLine("UnixSocket ----");

            (readen1, writen1, time1) = UnixSocketRunner.RunBig();
            Console.WriteLine($"DATARATE - in {Math.Round(time1.TotalSeconds, 4)}s Readen {Math.Round(readen1, 2)} kB/s - Writen {Math.Round(writen1, 2)} kB/s");

            (readen2, writen2, time2) = UnixSocketRunner.RunSmall();
            Console.WriteLine($"DATARATE - in {Math.Round(time2.TotalSeconds, 4)}s Readen {Math.Round(readen2, 2)} kB/s - Writen {Math.Round(writen2, 2)} kB/s");
            Console.WriteLine("-----");


        }
    }
}
