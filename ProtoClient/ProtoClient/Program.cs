using System;
using System.Threading;

namespace ProtoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new ProtoClientClass(457);
            c.Connect(ProtoClientClass.DEFAULT_NET_STRING);
            var awaiter = c.WaitToFinish();
            awaiter.Wait();
            c.Close();
        }
    }
}
