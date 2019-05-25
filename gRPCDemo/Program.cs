using System;
using Grpc.Core;

namespace gRPCDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Server server = new Server
                {
                    Ports = { new ServerPort("localhost", 11223, ServerCredentials.Insecure) },
                    Services = { Status.Status.BindService(new StatusServer()) }
                };
                Console.WriteLine("Status server listening on Port 11223");
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();

                server.ShutdownAsync().Wait();
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
