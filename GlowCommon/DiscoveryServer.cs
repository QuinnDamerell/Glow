using GlowCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace GlowCommon
{
    #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public class DiscoveryServer
    {
        public const int GLOW_SERVER_DISCOVER_PORT = 48595;
        private const int GLOW_SERVER_DISCOVER_INTERVAL_MS = 10 * 1000;
        public const int GLOW_SERVER_DISCOVER_HEADER = 554532;

        public enum DiscoveryMode
        {
            Listen,
            Broadcast
        }

        //
        // Private Vars
        //
        DiscoveryMode m_mode;
        int m_workCountDown = 0;
        DatagramSocket m_dataSocket;
        DataWriter m_dataWriter = null;
        bool m_isRunning = false;
        IDiscoveryServerListener m_listener;

        //
        // Constructor
        //
        public DiscoveryServer(DiscoveryMode mode)
        {
            m_mode = mode;
            m_dataSocket = new DatagramSocket();

            if (m_mode == DiscoveryMode.Listen)
            {
                m_dataSocket.MessageReceived += DataSocket_MessageRecieved;

                new Task(async () =>
                {
                    try
                    {
                        await m_dataSocket.BindServiceNameAsync(GLOW_SERVER_DISCOVER_PORT + "");
                    }
                    catch(Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("UDP unable to bind to port!. Message: " + e.Message);
                    }                    
                }).Start();                
            }
        }

        public void SetListener(IDiscoveryServerListener listener)
        {
            m_listener = listener;
        }

        private async void DataSocket_MessageRecieved(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                DataReader reader = new DataReader(args.GetDataStream());

                // Wait for a new message, here we wait for enough data to represent the string size
                // if we don't get all of the data, the socket was closed.
                uint waitData = await reader.LoadAsync(sizeof(uint));
                if (waitData != sizeof(uint))
                {
                    // We didn't get it all, the socket is closed.
                    return;
                }
                // Get the string size
                uint stringLen = reader.ReadUInt32();

                // Now wait for the actual string data
                uint stringWaitData = await reader.LoadAsync(stringLen);
                if (stringWaitData != stringLen)
                {
                    // We couldn't read the full string length.
                    return;
                }

                // Get the actual string
                string ipAddress = reader.ReadString(stringLen);

                // Do some simple validation
                Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                if(ip.IsMatch(ipAddress))
                {
                    if(m_listener != null)
                    {
                        m_listener.OnClientFound(ipAddress);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception in UDP message received. Message: " + e.Message);
            }
        }


        // Called only for broadcast for now, this invokes us to send a message.
        public void DoWork(uint timeElaspedMs)
        {
            m_workCountDown -= (int)timeElaspedMs;

            if(m_workCountDown < 0 && !m_isRunning)
            {
                // Reset work count down
                m_workCountDown = GLOW_SERVER_DISCOVER_INTERVAL_MS;

                // Kick off a thread to work in the background
                if(m_mode == DiscoveryMode.Broadcast)
                {
                    m_isRunning = true;
                    new Task(SendBroadcast).Start();
                }                
            }
        }

        private async void SendBroadcast()
        {
            try
            {
                // Get the current IP address.
                string ipAddress = "";
                foreach (Windows.Networking.HostName hostName in NetworkInformation.GetHostNames())
                {
                    if (hostName.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        ipAddress = hostName.CanonicalName;
                        break;
                    }
                }
                if (ipAddress == "")
                {
                    return;
                }
                         
                // Open the writer if needed
                if (m_dataWriter == null)
                {
                    var dataStream = await m_dataSocket.GetOutputStreamAsync(new Windows.Networking.HostName("255.255.255.255"), GLOW_SERVER_DISCOVER_PORT + "");
                    m_dataWriter = new DataWriter(dataStream);
                }
                if (m_dataWriter == null)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to open the datawriter for UDP!");
                    return;
                }

                // Finally, write the message
                m_dataWriter.WriteInt32(ipAddress.Length);
                m_dataWriter.WriteString(ipAddress);
                await m_dataWriter.StoreAsync();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception in broadcast. Message: "+e.Message);
            }

            // Indicate we are done.
            m_isRunning = false;
        }
    }
}
