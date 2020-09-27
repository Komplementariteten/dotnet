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
    using System.Collections.Generic;

    public class SocketWriter
    {
        private byte[] smallBuffer;
        private byte[] bigBuffer;

        private Socket target;

        public SocketWriter()
        {
            var small = new SomeData(Constants.SIZEOF_SMALL);
            var big = new SomeData(Constants.SIZEOF_BIG);


            var sepSpan = new Span<Seperator>(new Seperator[] { small.Seperator });
            var sepBytes = MemoryMarshal.AsBytes(sepSpan).ToArray();

            var globSpan = new Span<Global>(new Global[] { small.Global });
            var globBytes = MemoryMarshal.AsBytes(globSpan).ToArray();

            var smallSpan = new Span<AmpTof>(small.Content);
            var smallBytes = MemoryMarshal.AsBytes(smallSpan).ToArray();

            var bigSpan = new Span<AmpTof>(big.Content);
            var bigBytes = MemoryMarshal.AsBytes(bigSpan).ToArray();
            var smallByteList = new List<byte>();

            smallByteList.AddRange(sepBytes);
            smallByteList.AddRange(globBytes);
            smallByteList.AddRange(smallBytes);
            this.smallBuffer = smallByteList.ToArray();

            var bigByteList = new List<byte>();
            bigByteList.AddRange(sepBytes);
            bigByteList.AddRange(globBytes);
            bigByteList.AddRange(bigBytes);
            this.bigBuffer = bigByteList.ToArray();
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
