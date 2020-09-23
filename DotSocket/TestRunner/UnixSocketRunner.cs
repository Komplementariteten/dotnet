namespace TestRunner
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using DataHandlers;

    public class UnixSocketRunner
    {
        public UnixSocketRunner()
        {

        }

        public static (double, double, TimeSpan) RunSmall()
        {

            var tempFileName = "abc1.sock";
            var serverCancellationSource = new CancellationTokenSource();
            var clientCancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();
            TimeSpan elapesedTime;
            ulong writer_count = 0, reader_count = 0;
            try
            {
                var sw = new SocketWriter();
                var sr = new SocketReader();
                var socks = UnixSocketRunner.GetUnixSockets(ProtocolType.Unspecified, tempFileName);

                var wirter = sw.Run(DataType.Small, serverCancellationSource.Token, socks.Server);

                var reader = sr.Run(DataType.Small, clientCancellationSource.Token, socks.Client);

                serverCancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                writer_count = wirter.Result;

                clientCancellationSource.Cancel();

                reader_count = reader.Result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                elapesedTime = stopWatch.Elapsed;
                stopWatch.Stop();

                File.Delete(tempFileName);
            }
            double readen_kbPerS = (reader_count / 1024) / elapesedTime.TotalSeconds;
            double writen_kbPerS = (writer_count / 1024) / elapesedTime.TotalSeconds;
            return (readen_kbPerS, writen_kbPerS, elapesedTime);
        }

        public static (double, double, TimeSpan) RunBig()
        {

            var tempFileName = "abc2.sock";
            var serverCancellationSource = new CancellationTokenSource();
            var clientCancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();
            TimeSpan elapesedTime;
            ulong writer_count = 0, reader_count = 0;
            try
            {
                var sw = new SocketWriter();
                var sr = new SocketReader();
                var socks = UnixSocketRunner.GetUnixSockets(ProtocolType.Unspecified, tempFileName);

                var wirter = sw.Run(DataType.Big, serverCancellationSource.Token, socks.Server);

                var reader = sr.Run(DataType.Big, clientCancellationSource.Token, socks.Client);

                serverCancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                writer_count = wirter.Result;

                clientCancellationSource.Cancel();

                reader_count = reader.Result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                elapesedTime = stopWatch.Elapsed;
                stopWatch.Stop();
                File.Delete(tempFileName);
            }
            double readen_kbPerS = (reader_count / 1024) / elapesedTime.TotalSeconds;
            double writen_kbPerS = (writer_count / 1024) / elapesedTime.TotalSeconds;
            return (readen_kbPerS, writen_kbPerS, elapesedTime);

        }

        private static (Socket Server, Socket Client) GetUnixSockets(ProtocolType protocolType, string socketPath)
        {
            var usep = new UnixDomainSocketEndPoint(socketPath);
            var unixSocket = new Socket(AddressFamily.Unix, SocketType.Stream, protocolType);
            unixSocket.Bind(usep);
            unixSocket.Listen(1);
            var clientSocket = new Socket(AddressFamily.Unix, SocketType.Stream, protocolType);
            clientSocket.Connect(usep);
            return (unixSocket, clientSocket);
        }
    }
}
