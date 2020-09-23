namespace DataHandlers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Threading;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Diagnostics;

    public class SocketWriter
    {
        private byte[] smallBuffer;
        private byte[] bigBuffer;

        private Socket target;

        public SocketWriter()
        {
            var small = new SomeData(16);
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

        public Task<ulong> Run(DataType dataType, CancellationToken token, Socket target)
        {
            double size_in_bytes = dataType == DataType.Big ? this.bigBuffer.Length : this.smallBuffer.Length;
            double size_in_kb = size_in_bytes / 1024;
            Console.WriteLine($"DataPackage has {Math.Round(size_in_kb, 4)} kByte");
            this.target = target;
            return Task.Factory.StartNew<ulong>(this.SendData(dataType, token), token, TaskCreationOptions.LongRunning);
        }

        private Func<object, ulong> SendData(DataType dataType, CancellationToken token)
        {
            return (o) =>
            {
                ulong count = 0;
                var client = this.target.Accept();
                client.ReceiveTimeout = 1000;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (dataType == DataType.Big)
                        {
                            if (!client.Connected) break;
                            client.Send(this.bigBuffer);
                            count += Convert.ToUInt64(this.bigBuffer.Length);
                        }
                        else
                        {
                            if (!client.Connected) continue;
                            client.Send(this.smallBuffer);
                            count += Convert.ToUInt64(this.smallBuffer.Length);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.StackTrace);
                    }
                }
                this.target.Close();
                return count;
            };
        }
    }

}
