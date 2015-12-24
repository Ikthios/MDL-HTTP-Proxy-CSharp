using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Proxy
{
    class Proxy
    {
        private Socket clientSocket;

        public Proxy() { }     // Default constructor

        public Proxy(Socket client)
        {
            this.clientSocket = client;
        }

        // Create a new thread for a new client
        public void StartHandling()
        {
            Console.WriteLine("Accepting connection.\nStarting handler...");
            Thread handler = new Thread(requestHandler);
            handler.Priority = ThreadPriority.Normal;
            handler.Start();
        }

        private void requestHandler()
        {
            /*
            Begin variables section
            */
            List<string> requestLines = new List<string>();     // String list to hold forwarded data
            int byteCounter = 0;                                // Counter the number of bytes for entire session
            string EOL = "\r\n";                                // End of line characters found at the end of HTTP requests
            string requestPayload = "";                         // Data to be forwarded to the webserver
            string requestTempLine = "";                        // Temporary data line
            bool receiveRequest = true;                         // Boolean for if requests are being received
            byte[] requestBuffer = new byte[1];
            byte[] responseBuffer = new byte[1];

            requestLines.Clear();   // Clear lines from the previous request

            try
            {
                // Handle request from the client
                while (receiveRequest)
                {
                    this.clientSocket.Receive(requestBuffer);
                    /*
                    ASCIIEncoding is legacy and does not provide error detection/handling.
                    UTF[7, 8, 32]Encoding does provide these services.
                    */
                    string fromByte = UTF32Encoding.ASCII.GetString(requestBuffer);     // Decodes all the bytes in the array into a string
                    requestPayload += fromByte;
                    requestTempLine += fromByte;
                    byteCounter++;

                    if (requestTempLine.EndsWith(EOL))
                    {
                        requestLines.Add(requestTempLine.Trim());
                        requestTempLine = "";
                    }

                    if (requestPayload.EndsWith(EOL + EOL))
                    {
                        receiveRequest = false;
                    }
                }

                Console.WriteLine("Raw request received...");
                Console.WriteLine(requestPayload);

                // Rebuild request info and create connection to destination server
                string remoteHost = requestLines[0].Split(' ')[1].Replace("http://", "").Split('/')[0];
                string requestFile = requestLines[0].Replace("http://", "").Replace(remoteHost, "");
                requestLines[0] = requestFile;

                requestPayload = "";
                foreach (string line in requestLines)
                {
                    requestPayload += line;
                    requestPayload += EOL;
                }

                // Connect to the outside web server
                Socket destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                destServerSocket.Connect(remoteHost, 80);

                // Send new request info to destination server and relay response to client
                destServerSocket.Send(UTF32Encoding.ASCII.GetBytes(requestPayload));    // Sends data to the connected socket

                Console.WriteLine("Receiving response...");
                while (destServerSocket.Receive(responseBuffer) != 0)
                {
                    // Send data to the client web browser
                    this.clientSocket.Send(responseBuffer);
                }

                // Cleanup
                destServerSocket.Disconnect(false);
                destServerSocket.Dispose();
                this.clientSocket.Disconnect(false);
                this.clientSocket.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Proxy Error: " + ex.Message);
            }

        }

        private string GetUrl(String payload)
        {
            String[] tokens = payload.Split(' ');   // Split the requestPayload and get the url (second token item)
            return tokens[1];
        }
    }
}
