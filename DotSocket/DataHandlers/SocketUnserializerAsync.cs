﻿using System;
using System.Collections.Concurrent;
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

        public Stopwatch RunWatch { get; } = new Stopwatch();

        private DataType dataType;

        private ConcurrentQueue<byte[]> dataQue = new ConcurrentQueue<byte[]>();

        private Task<int> processor;

        private ulong dataReaden;

        private int numberOfItems = 0;

        private int quedItems = 0;

        private bool isStarted = false;

        private int sizeOfSeperator = 0;

        private int sizeOfGlobal = 0;

        private int sizeOfAmp = 0;

        private int dataSize;

        public SocketUnserializerAsync()
        {
            this.transformBlock = new TransformBlock<byte[], SomeData>(this.transformBytesToData, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

            this.StoreAction = new ActionBlock<SomeData>(this.StoreDataAction, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

            this.transformBlock.LinkTo(this.StoreAction);

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
            this.processor = Task.Factory.StartNew((o) =>
            {
                byte[] item;
                while (!this.isStarted || this.quedItems > 0)
                {

                    if (this.dataQue.TryDequeue(out item))
                    {
                        for (int i = 0; i < item.Length; i++)
                        {
                            this.batchBlock.Post(item[i]);
                        }
                        this.quedItems--;
                    }
                    else
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(10));
                        if (this.dataQue.Count == 0 && this.dataStore.Count > 0)
                        {
                            return 1;
                        }
                    }
                }
                return 0;
            }, token, TaskCreationOptions.LongRunning);

            return Task.Factory.StartNew(this.ReadData(dataType, token), token, TaskCreationOptions.LongRunning);
        }


        private Func<object, ulong> ReadData(DataType dataType, CancellationToken token)
        {
            return (o) =>
            {
                ulong count = 0;
                var data = new List<SomeData>();
                var state = new StateObject();
                this.RunWatch.Start();
                state.workSocket = this.source;

                while (this.source.Connected)
                {
                    this.source.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(this.RecieveCallback), state);
                }
                this.RunWatch.Stop();
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
                    this.dataQue.Enqueue(s.buffer);
                    this.isStarted = true;
                    this.quedItems++;
                    c.BeginReceive(s.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), s);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private ulong HandleReturn(ulong count)
        {
            this.processor.Wait();
            this.batchBlock.Complete();
            this.transformBlock.Complete();
            this.StoreAction.Complete();
            this.StopWatch.Stop();
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
