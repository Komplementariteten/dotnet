using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataHandlers
{
    public class SocketUnserializerAsync
    {
        private Socket source;

        public ActionBlock<SomeData> StoreAction { get; }

        private TransformBlock<byte[], SomeData> transformBlock;

        private BatchBlock<byte> batchBlock;

        private List<SomeData> dataStore = new List<SomeData>();

        public Stopwatch StopWatch { get; } = new Stopwatch();

        private DataType dataType;

        private ulong dataReaden;

        private int numberOfItems = 0;

        private int sizeOfSeperator = 0;

        private int sizeOfGlobal = 0;

        private int sizeOfAmp = 0;

        private int dataSize;

        public SocketUnserializerAsync()
        {
            this.transformBlock = new TransformBlock<byte[], SomeData>(this.transformBytesToData, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

            this.StoreAction = new ActionBlock<SomeData>(this.StoreDataAction, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

            this.transformBlock.LinkTo(this.StoreAction);

            this.transformBlock.Completion.ContinueWith((a) => this.StopWatch.Stop());
        }

        public void StoreDataAction(SomeData obj)
        {
            this.dataStore.Add(obj);
        }

        private SomeData transformBytesToDataBf(byte[] arg)
        {
            if (!this.StopWatch.IsRunning)
            {
                this.StopWatch.Restart();
            }

            var data = new SomeData(this.numberOfItems);
            using (var ms = new MemoryStream(arg))
            {
                var bf = new BinaryFormatter();
                data = (SomeData)bf.Deserialize(ms);
            }

            return data;
        }

        private SomeData transformBytesToData(byte[] arg)
        {
            if (!this.StopWatch.IsRunning)
            {
                this.StopWatch.Restart();
            }

            if (arg.Length != this.dataSize) return new SomeData(1);

            var dataSpan = new Span<byte>(arg);
            SomeData data = new SomeData();
            var globalSlice = dataSpan.Slice(this.sizeOfSeperator, this.sizeOfGlobal);
            var gdata = MemoryMarshal.AsRef<Global>(globalSlice);
            data.Global = gdata;

            var contentSilce = dataSpan.Slice(this.sizeOfGlobal + this.sizeOfSeperator);
            data.Content = new AmpTof[this.numberOfItems];

            for (var i = 0; i < this.numberOfItems; i++)
            {
                var amp_silice = contentSilce.Slice(i * sizeOfAmp, sizeOfAmp);
                var ampdata = MemoryMarshal.AsRef<AmpTof>(amp_silice);
                data.Content[i] = ampdata;
            }

            return data;
        }

        public Task<ulong> Run(DataType dataType, CancellationToken token, Socket source)
        {
            this.source = source;
            this.dataType = dataType;
            this.numberOfItems = this.dataType == DataType.Big ? Constants.SIZEOF_BIG : Constants.SIZEOF_SMALL;
            this.sizeOfSeperator = Marshal.SizeOf<Seperator>();
            this.sizeOfGlobal = Marshal.SizeOf<Global>();
            this.sizeOfAmp = Marshal.SizeOf<AmpTof>();

            source.Blocking = false;

            this.dataSize = this.sizeOfGlobal + this.sizeOfSeperator + (this.numberOfItems * this.sizeOfAmp);
            this.batchBlock = new BatchBlock<byte>(this.dataSize);
            this.batchBlock.LinkTo(this.transformBlock);
            return Task.Factory.StartNew(this.ReadData(dataType, token), token, TaskCreationOptions.LongRunning);
        }

        private Func<object, ulong> ReadData(DataType dataType, CancellationToken token)
        {
            return (o) =>
            {
                ulong count = 0;
                var data = new List<SomeData>();
                var state = new StateObject();
                state.workSocket = this.source;

                while (this.source.Connected)
                {
                    this.source.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(this.RecieveCallback), state);
                }
                return this.HandleReturn(count);
            };
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            if (ar == null) return;
            try
            {
                StateObject s = (StateObject)ar.AsyncState;
                Socket c = s.workSocket;

                if (!c.Connected) return;

                int recieved = c.EndReceive(ar);

                this.dataReaden += (ulong)recieved;

                if (recieved > 0)
                {
                    for (int i = 0; i < s.buffer.Length; i++)
                    {
                        this.batchBlock.Post(s.buffer[i]);
                    }

                    c.BeginReceive(s.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), s);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private ulong HandleReturn(ulong count)
        {
            this.batchBlock.Complete();
            this.transformBlock.Complete();
            this.StoreAction.Completion.ContinueWith((o) =>
            {
                this.StopWatch.Stop();
            });
            this.StoreAction.Complete();
            return this.dataReaden;
        }
    }

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = Constants.SIZEOF_BIG / 4;
        public byte[] buffer = new byte[BufferSize];
    }
}
