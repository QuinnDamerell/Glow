using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon.Interfaces;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using GlowCommon.DataObjects;

namespace GlowCommon
{
    public class CommandServer
    {
        private const int GLOW_SERVER_PORT = 48593;

        public enum CommmandServerMode
        {
            Server,
            Client
        }

        //
        // Private Vars
        //
        ICommandServerListener m_listener;
        StreamSocketListener m_socketListener;
        StreamSocket m_socket;
        CommmandServerMode m_mode;

        //
        // Constructor
        //
        public CommandServer(ICommandServerListener listener, CommmandServerMode mode, string ipAddress = "")
        {
            if (listener == null)
            {
                throw new Exception("The listener can't be null!");
            }
            m_listener = listener;
            m_mode = mode;

            // This the setup on a new task
            new Task(async () =>
            {
                try
                {
                    if (mode == CommmandServerMode.Server)
                    {
                        // Bind the socket
                        m_socketListener = new StreamSocketListener();
                        m_socketListener.ConnectionReceived += SocketAccpet;
                        await m_socketListener.BindServiceNameAsync("" + GLOW_SERVER_PORT);
                    }
                    else
                    {
                        if(ipAddress == null)
                        {
                            throw new Exception("The Ip address can't be empty!");
                        }

                        // Open the socket
                        m_socket = new StreamSocket();
                        await m_socket.ConnectAsync(new Windows.Networking.HostName(ipAddress), GLOW_SERVER_PORT + "");

                        // Start the listener
                        ServiceSocket(m_socket);
                    }
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Socket Create Failed: " + e.Message);
                    m_listener.OnFatalError();
                }
            }).Start();
        }

        /// <summary>
        /// Can be called by the client to send a message.
        /// </summary>
        /// <param name="cmd">Message to be sent</param>
        public async Task<bool> SendMessage(Command cmd)
        {
            if(m_mode == CommmandServerMode.Server)
            {
                throw new NotImplementedException("Server can't send message right now");
            }

            if(m_socket == null)
            {
                throw new Exception("The socket isn't open!");
            }

            // Send the message
            await InternalSendMessage(cmd, new DataWriter(m_socket.OutputStream));

            return true;
        }

        /// <summary>
        /// Only used by the server side, accepts new clients.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SocketAccpet(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            ServiceSocket(args.Socket);
        }

        /// <summary>
        /// Called by both the server and client to service the socket.
        /// This function will return when the socket dies.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private void ServiceSocket(StreamSocket socket)
        {
            // Sleep a thread on the socket
            new Task(async () =>
            {
                try
                {
                    // The the listener someone connected
                    m_listener.OnConnect();

                    // Make the readers and writers
                    DataReader reader = new DataReader(socket.InputStream);
                    DataWriter writer = new DataWriter(socket.OutputStream);

                    // Loop
                    while (true)
                    {
                        // Wait for a command
                        Command cmd = await WaitForCommand(reader);

                        // Send the command to the consumer
                        Command response = m_listener.OnCommand(cmd);

                        // If the gave a message back, send it.
                        if(response != null)
                        {
                            await InternalSendMessage(response, writer);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("The socket listener hit an exception: " + e.Message);
                    m_listener.OnDisconnected();
                }
            }).Start();      
        }        

        private async Task<Command> WaitForCommand(DataReader reader)
        {
            // Wait for a new message, here we wait for enough data to represent the string size
            // if we don't get all of the data, the socket was closed.
            uint waitData = await reader.LoadAsync(sizeof(uint));
            if (waitData != sizeof(uint))
            {
                // We didn't get it all, the socket is closed.
                throw new Exception("Socket closed");
            }
            // Get the string size
            uint stringLen = reader.ReadUInt32();

            // Now wait for the actual string data
            uint stringWaitData = await reader.LoadAsync(stringLen);
            if (stringWaitData != stringLen)
            {
                // We couldn't read the full string length.
                throw new Exception("Socket closed");
            }

            // Get the actual string
            string commandString = reader.ReadString(stringLen);

            // Parse the command
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Command>(commandString);
        }

        private async Task InternalSendMessage(Command cmd, DataWriter writer)
        {
            if(cmd == null)
            {
                return;
            }

            if(cmd.Program == GlowPrograms.None)
            {
                throw new Exception("The program can't be none!");
            }

            // Serialize the cmd
            string cmdJson = Newtonsoft.Json.JsonConvert.SerializeObject(cmd);
            writer.WriteUInt32((uint)cmdJson.Length);
            writer.WriteString(cmdJson);
            await writer.StoreAsync();
            await writer.FlushAsync();
        }
    }
}
