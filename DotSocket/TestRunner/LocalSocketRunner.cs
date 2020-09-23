using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DataHandlers;

namespace TestRunner
{
    public class LocalSocketRunner
    {
        public LocalSocketRunner()
        {

        }

        public static (double, double, TimeSpan) RunSmall()
        {
            var serverCancellationSource = new CancellationTokenSource();
            var clientCancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();
            TimeSpan elapesedTime;
            ulong writer_count = 0, reader_count = 0;
            try
            {
                var sw = new SocketWriter();
                var sr = new SocketReader();
                var socks = LocalSocketRunner.GetLocalSockets();

                var wirter = sw.Run(DataType.Small, serverCancellationSource.Token, socks.Server);

                var reader = sr.Run(DataType.Small, clientCancellationSource.Token, socks.Client);

                serverCancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                writer_count = wirter.Result;

                clientCancellationSource.Cancel();

                reader_count = reader.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                elapesedTime = stopWatch.Elapsed;
                stopWatch.Stop();
            }
            double readen_kbPerS = (reader_count / 1024) / elapesedTime.TotalSeconds;
            double writen_kbPerS = (writer_count / 1024) / elapesedTime.TotalSeconds;
            return (readen_kbPerS, writen_kbPerS, elapesedTime);
        }

        public static (double, double, TimeSpan) RunBig()
        {
            var serverCancellationSource = new CancellationTokenSource();
            var clientCancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();
            TimeSpan elapesedTime;
            ulong writer_count = 0, reader_count = 0;
            try
            {
                var sw = new SocketWriter();
                var sr = new SocketReader();
                var socks = LocalSocketRunner.GetLocalSockets();

                var wirter = sw.Run(DataType.Big, serverCancellationSource.Token, socks.Server);

                var reader = sr.Run(DataType.Big, clientCancellationSource.Token, socks.Client);

                serverCancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                writer_count = wirter.Result;

                clientCancellationSource.Cancel();

                reader_count = reader.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                elapesedTime = stopWatch.Elapsed;
                stopWatch.Stop();
            }
            double readen_kbPerS = (reader_count / 1024) / elapesedTime.TotalSeconds;
            double writen_kbPerS = (writer_count / 1024) / elapesedTime.TotalSeconds;
            return (readen_kbPerS, writen_kbPerS, elapesedTime);
        }

        private static (Socket Server, Socket Client) GetLocalSockets()
        {
            var localHost = IPEndPoint.Parse("127.0.0.1");
            localHost.Port = 65111;
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(localHost);
            tcpSocket.Listen(1);
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(localHost);
            return (tcpSocket, clientSocket);
        }
    }
}
