namespace DotSocket
{
    using System;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net.Sockets;

    public class SocketWriter
    {
        private byte[] smallBuffer;
        private byte[] bigBuffer;

        private Socket target;

        public SocketWriter()
        {
            var small = new SomeData(128);
            var big = new SomeData(64824);
            var smallSize = Marshal.SizeOf(small);
            var bigSize = Marshal.SizeOf(big);
            this.smallBuffer = new byte[smallSize];
            this.bigBuffer = new byte[bigSize];

            IntPtr smallPtr = Marshal.AllocHGlobal(smallSize);
            Marshal.StructureToPtr(small, smallPtr, true);
            Marshal.Copy(smallPtr, this.smallBuffer, 0, smallSize);
            Marshal.FreeHGlobal(smallPtr);

            IntPtr bigPtr = Marshal.AllocHGlobal(bigSize);
            Marshal.StructureToPtr(big, bigPtr, true);
            Marshal.Copy(bigPtr, this.bigBuffer, 0, bigSize);
            Marshal.FreeHGlobal(bigPtr);
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
                while (!token.IsCancellationRequested)
                {
                    if (dataType == DataType.Big)
                        count += (ulong)this.target.Send(this.bigBuffer);
                    else
                        count += (ulong)this.target.Send(this.smallBuffer);
                }
                return count;
            };
        }
    }

}
