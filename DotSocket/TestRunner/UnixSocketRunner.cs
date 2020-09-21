namespace TestRunner
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using DataHandlers;

    public class UnixSocketRunner
    {
        public UnixSocketRunner()
        {

        }

        public static void Run()
        {

            var tempFileName = "abc.sock";
            var cancellationSource = new CancellationTokenSource();
            try
            {
                var sSock = UnixSocketRunner.GetUnixSocket(ProtocolType.Unspecified, tempFileName);

                var sw = new SocketWriter();
                var wirter = sw.Run(DataType.Big, cancellationSource.Token, sSock);
            }
            catch (Exception e)
            {
            }

            File.Delete(tempFileName);

        }

        private static Socket GetUnixSocket(ProtocolType protocolType, string socketPath)
        {
            var usep = new UnixDomainSocketEndPoint(socketPath);
            var unixSocket = new Socket(AddressFamily.Unix, SocketType.Stream, protocolType);
            unixSocket.Bind(usep);
            unixSocket.Listen(1);
            return unixSocket;
        }
    }
}
