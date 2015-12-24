using System;
using System.Net;
using System.Net.Sockets;

namespace Proxy
{
    class Core
    {
        static void Main(string[] args)
        {
            Core core = new Core();
            int port = core.getPort();

            // Send host port to the proxy
            Console.WriteLine("Starting server on Port " + port);
            TcpListener listener = new TcpListener(IPAddress.Any, port);    // TcpListener object

            // Start the server in 'Server.cs'
            Console.WriteLine("Starting the TCP Listener...");
            listener.Start();
            Console.WriteLine("Looping interface...");

            // While true accept new connections
            while (true)
            {
                Socket newClient = listener.AcceptSocket();
                Proxy client = new Proxy(newClient);

                // Start handling the client connection in 'Handler.cs'
                client.StartHandling();
            }
        }

        private int getPort()
        {
            Console.WriteLine("Enter hosting port or press Return for default (6000): ");
            string portString = Console.ReadLine();
            if (portString.Equals(""))
                return 6000;
            else
            {
                int port = Convert.ToInt32(portString);
                return port;
            }
        }
    }
}
