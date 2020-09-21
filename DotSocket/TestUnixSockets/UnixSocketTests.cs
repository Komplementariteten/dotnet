namespace TestUnixSockets
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Net.Sockets;
    [TestClass]
    public class UnixSocketTests
    {

        [TestMethod]
        public void TestUnspecified()
        {
            var s = this.GetUnixSocket(ProtocolType.Unspecified);
            var writer = new SocketWriter();
        }
    }
}
