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
            TcpListener listener = new TcpListener(IPAddress.Any, port);

            // Start the server in 'Proxy.cs'
            Console.WriteLine("Starting the TCP Listener...");
            listener.Start();
            Console.WriteLine("Looping interface...");

            // While true accept new connections
            while (true)
            {
                Socket newClient = listener.AcceptSocket();
                Proxy client = new Proxy(newClient);

                // Start handling the client connection in 'Proxy.cs'
                client.StartHandling();
            }
        }

        private int getPort()
        {
            /*
            Used for starting the proxy on a custom port if desired.
            The default port is 6000 since it is my favorite non-well
            known port that has never failed me.
            */
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
