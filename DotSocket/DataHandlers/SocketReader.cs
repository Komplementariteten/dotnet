using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DataHandlers
{
    public class SocketReader
    {
        private Socket source;

        public SocketReader()
        {
        }

        public Task<ulong> Run(DataType dataType, CancellationToken token, Socket source)
        {
            this.source = source;
            return Task.Factory.StartNew<ulong>(this.ReadData(dataType, token), token, TaskCreationOptions.LongRunning);
        }

        private Func<object, ulong> ReadData(DataType dataType, CancellationToken token)
        {
            return (o) =>
            {
                ulong count = 0;
                var data = new List<byte[]>();

                while (this.source.Connected)
                {
                    if (this.source.Available == 0)
                    {
                        if (token.IsCancellationRequested) return count;
                        Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
                        continue;
                    }
                    var buffer = new byte[this.source.Available];
                    var readen = this.source.Receive(buffer);
                    data.Add(buffer);
                    count += Convert.ToUInt64(readen);
                }
                return count;
            };
        }

    }
}
