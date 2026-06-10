# Documented File: SimpleUDP.cs
**Repository Path:** `_NewLib\SimpleUDP.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using _Configuration;

namespace _NewLib
{
    /// <summary>
    /// This class provides methods provides that send and receive UDP messages to/from a
    /// prescribed dynamic port on <i>localHost</i> using the loopback address 
    /// 127.0.0.1
    /// </summary>
    /// <remarks>
    /// The following code fragments provide example usage of 
    /// the methods in this class.<b></b>
    /// <code>
    ///        // Example sender.
    ///        static void Main(string[] args)
    ///        {
    ///            Console.Write("\nSending message in 5 seconds...");
    ///
    ///            Thread.Sleep(5000);
    ///
    ///            int port = SimpleUDP.HashToDynamicPortNumber(999999999);
    ///
    ///            string message = "Hello from sender!";
    ///
    ///            int retVal = SimpleUDP.Send(port, message);
    ///
    ///            Console.Write("\nPort {0}: sent message: |{1}|", port, message);
    ///            Console.Write("\nretVal = " + retVal);
    ///
    ///            Environment.Exit(0);
    ///        }
    ///		
    ///        // Example receiver.
    ///        static void Main(string[] args)
    ///        {
    ///            Console.Write("\nWaiting for message...");
    ///
    ///            string message;
    ///
    ///            int port = SimpleUDP.HashToDynamicPortNumber(999999999);
    ///
    ///            // This will block until a message is received or an error occurs.
    ///            int retVal = SimpleUDP.Receive(port, out message);
    ///
    ///            Console.Write("\nPort {0} received message: |{1}|", port, message);
    ///            Console.Write("\nretVal = " + retVal);
    ///
    ///            Environment.Exit(0);
    ///        }
    /// </code>
    /// </remarks>
    public class SimpleUDP
    {
        private const string LOOP_BACK = "127.0.0.1";
        private const int DEFAULT_PORT = 11000;

        public const int LOWEST_DYNAMIC_PORT = 49152;
        public const int HIGHEST_DYNAMIC_PORT = 65535;
        public const int NUMBER_DYNAMIC_PORTS = HIGHEST_DYNAMIC_PORT - LOWEST_DYNAMIC_PORT + 1;

        /// <summary>
        /// This method provides 'one shot' UDP message transmission to a
        /// prescribed dynamic port on <i>localHost</i> using the loopback address 
        /// 127.0.0.1
        /// </summary>
        /// <param name="port"> - must be in the range 49152 to 65535, inclusive.</param>
        /// <param name="message"> - message to be sent.</param>
        /// <returns></returns>
        public static int Send(int port, string message)
        {
            int retVal = Constant.FAILURE;

            if (!IsValidDynamicPortNumber(port))
            {
                Log2.e("\nSimpleUDP.Send(): ERROR: invalid dynamic port number: " + port);

                return retVal;
            }

            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

            IPAddress send_to_address = IPAddress.Parse(LOOP_BACK);

            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, port);

            byte[] send_buffer = Encoding.ASCII.GetBytes(message);

            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);

                retVal = Constant.SUCCESS;

                //...Log2.v(String.Format("\nSimpleUDP.Send(): Message sent via UDP to address {0}:{1}", send_to_address, port));
            }
            catch (Exception send_exception)
            {
Log2.e("\nSimpleUDP.Send(): ERROR: {0}", send_exception.Message);
            }

            return retVal;
        }

        /// <summary>
        /// This method provides 'one shot' UDP message reception from a
        /// prescribed dynamic port on <i>localHost</i> using the loopback address 
        /// 127.0.0.1
        /// </summary>
        /// <param name="port"> - must be in the range 49152 to 65535, inclusive.</param>
        /// <param name="message"> - message received.</param>
        /// <returns></returns>
        public static int Receive(int port, out string message)
        {
            // 'out' requirement.
            message = "";

            int retVal = Constant.FAILURE;

            if (!IsValidDynamicPortNumber(port))
            {
                Log2.e("\nSimpleUDP.Receive(): ERROR: invalid dynamic port number: " + port);

                return retVal;
            }

            UdpClient listener = new UdpClient(port);

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

            string received_data;
            byte[] receive_byte_array;

            try
            {
                //...Log2.v(String.Format("\nSimpleUDP.Receive(): Listening on port: {0}", port));

                // This listener blocks until a UDP message is received on the prescribed port.
                receive_byte_array = listener.Receive(ref groupEP);

                received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);

                message = received_data;

                retVal = Constant.SUCCESS;

                //...Log2.v(String.Format("\nSimpleUDP.Receive(): message received on port {0}", port));
            }
            catch (Exception e)
            {
                Log2.e("\nSimpleUDP.Receive(): ERROR: " + e.ToString());
            }

            listener.Close();

            return retVal;
        }

        /// <summary>
        /// Returns true if a prescribed port number is a valid
        /// Dynamic Port number.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsValidDynamicPortNumber(int port)
        {
            bool result = false;

            if ((port >= LOWEST_DYNAMIC_PORT) && (port <= HIGHEST_DYNAMIC_PORT))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Given an arbitrary port number this method returns a 'hashed', valid
        /// port number for a Dynamic Port.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static int HashToDynamicPortNumber(int port)
        {
            int retVal = port - LOWEST_DYNAMIC_PORT;
            int offset = retVal % NUMBER_DYNAMIC_PORTS;

            if (offset < 0) offset += NUMBER_DYNAMIC_PORTS;

            retVal = offset + LOWEST_DYNAMIC_PORT;

            return retVal;
        }


    }
}

```
