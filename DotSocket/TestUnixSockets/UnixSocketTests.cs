namespace TestUnixSockets
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Net.Sockets;
    using TestRunner;

    [TestClass]
    public class UnixSocketTests
    {

        [TestMethod]
        public void TestUnspecified()
        {
            UnixSocketRunner.RunBig();
            UnixSocketRunner.RunSmall();
        }
    }
}
