namespace DataHandlers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Threading;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class SocketWriter
    {
        private byte[] smallBuffer;
        private byte[] bigBuffer;

        private Socket target;

        public SocketWriter()
        {
            var small = new SomeData(128);
            var big = new SomeData(64824);

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, big);
                this.bigBuffer = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, small);
                this.smallBuffer = ms.ToArray();
            }
        }

        public void WriteSmal(Socket s)
        {
            s.Send(this.smallBuffer);
        }

        public Task<ulong> Run(DataType dataType, CancellationToken token, Socket target)
        {
            this.target = target;
            return Task.Factory.StartNew<ulong>(this.SendData(dataType, token), token, TaskCreationOptions.LongRunning);
        }

        private Func<object, ulong> SendData(DataType dataType, CancellationToken token)
        {
            return (o) =>
            {
                ulong count = 0;
                var client = this.target.Accept();
                while (!token.IsCancellationRequested)
                {
                    if (dataType == DataType.Big)
                        count += (ulong)client.Send(this.bigBuffer);
                    else
                        count += (ulong)client.Send(this.smallBuffer);
                }
                return count;
            };
        }
    }

}
