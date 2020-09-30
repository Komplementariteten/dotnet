using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DataHandlers;

namespace TestRunner
{
    public class LocalSocketUnserializer
    {
        private TimeSpan serializationTime;

        private SocketUnserializer reader;

        public LocalSocketUnserializer()
        {
            this.reader = new SocketUnserializer();
        }

        public (double BytesReaden, TimeSpan RunTime, TimeSpan SerialzationTime) RunBig()
        {
            var serverCancellationSource = new CancellationTokenSource();
            var clientCancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();
            TimeSpan elapesedTime;
            ulong reader_count = 0;
            try
            {
                var sw = new SocketWriter();
                var socks = LocalSocketUnserializer.GetLocalSockets();

                var wirter = sw.Run(DataType.Big, serverCancellationSource.Token, socks.Server);

                var readerTask = this.reader.Run(DataType.Big, clientCancellationSource.Token, socks.Client);

                serverCancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                _ = wirter.Result;

                clientCancellationSource.Cancel();

                reader_count = readerTask.Result;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                elapesedTime = stopWatch.Elapsed;
                stopWatch.Stop();
                this.serializationTime = this.reader.StopWatch.Elapsed;
            }
            double readen_kbPerS = (reader_count / 1024) / elapesedTime.TotalSeconds;
            return (readen_kbPerS, this.serializationTime, elapesedTime);
        }

        public (double BytesReaden, TimeSpan RunTime, TimeSpan SerialzationTime) RunBigAsync()
        {
            var serverCancellationSource = new CancellationTokenSource();
            var clientCancellationSource = new CancellationTokenSource();
            TimeSpan elapesedTime;
            ulong reader_count = 0;
            var sr = new SocketUnserializerAsync();
            try
            {
                var sw = new SocketWriter();
                var socks = LocalSocketUnserializer.GetLocalSockets();


                var wirter = sw.Run(DataType.Big, serverCancellationSource.Token, socks.Server);

                var readerTask = sr.Run(DataType.Big, clientCancellationSource.Token, socks.Client);

                serverCancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                _ = wirter.Result;

                clientCancellationSource.Cancel();

                reader_count = readerTask.Result;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                this.serializationTime = sr.StopWatch.Elapsed;
            }
            double readen_kbPerS = (reader_count / 1024) / sr.RunWatch.Elapsed.TotalSeconds;
            return (readen_kbPerS, this.serializationTime, sr.RunWatch.Elapsed);
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
