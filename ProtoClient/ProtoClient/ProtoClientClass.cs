using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoClient
{
    public class ProtoClientClass
    {
        private Content content = null;
        private TcpClient tcpClient = null;
        public const String DEFAULT_NET_STRING = "127.0.0.1:62253";
        private CancellationTokenSource cancellation;
        private int bufferSize;
        private Thread processingThread;
        private int readCounter = 0;

        public ProtoClientClass(int buffersize)
        {
            cancellation = new CancellationTokenSource();
            this.bufferSize = buffersize;
        }

        private void ProcessStream()
        {
            var bytes = new byte[this.bufferSize];
            var bytesSpan = new Span<byte>(bytes);
            using (var con = this.tcpClient.GetStream())
            {
                for (; ; )
                {
                    if (this.cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    con.Read(bytesSpan);
                    var data = Content.Parser.ParseFrom(bytes);
                    this.readCounter++;
                }
            }
        }

        private Thread SetupAndStartStreamProcessor()
        {
            var task = new ThreadStart(this.ProcessStream);
            Thread worker = new Thread(task);
            worker.Start();
            return worker;
        }

        private IPEndPoint GetEndPoint(string netstr)
        {
            Uri uri;
            if (Uri.TryCreate(netstr, UriKind.Absolute, out uri))
            {
                return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
            }
            if (Uri.TryCreate(String.Concat("tcp://", netstr), UriKind.Absolute, out uri))
            {
                return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
            }
            throw new FormatException("Failed to Parse IP Endpoint");
        }

        public void Close()
        {
            this.cancellation.Cancel(false);
            this.processingThread.Join();
            this.tcpClient.Close();
        }

        public void Connect(string netstr)
        {
            this.tcpClient = new TcpClient();
            this.tcpClient.Connect(this.GetEndPoint(netstr));

            this.processingThread = this.SetupAndStartStreamProcessor();
        }

        public Task<int> WaitToFinish()
        {
            return Task.Factory.StartNew(() =>
            {
                while (this.readCounter < 1000000)
                {
                    Task.Delay(TimeSpan.FromSeconds(0.5));
                }

                return this.readCounter;
            });
        }
    }
}
