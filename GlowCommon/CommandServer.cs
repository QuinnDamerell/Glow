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
        public const int GLOW_SERVER_PORT = 48593;

        //
        // Private Vars
        //
        ICommandListener m_listener;
        StreamSocketListener m_socketListener;

        //
        // Constructor
        //
        public CommandServer(ICommandListener listener)
        {
            if (listener == null)
            {
                throw new Exception("The listener can't be null!");
            }
            m_listener = listener;
        }

        public async void Setup(int port)
        {
            // Bind to the socket
            m_socketListener = new StreamSocketListener();
            m_socketListener.ConnectionReceived += SocketAccpet;
            await m_socketListener.BindServiceNameAsync("" + port);            
        }

        private void SocketAccpet(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            // Kick off a task to handle the new client connecting.
            Task socketHandler = new Task(async () =>
            {
                try
                {
                    // Open the input stream
                    DataReader reader = new DataReader(args.Socket.InputStream);
                    DataWriter writer = new DataWriter(args.Socket.OutputStream);

                    // Loop while we wait for messages.
                    while(true)
                    {
                        // Wait for a new message, here we wait for enough data to represent the string size
                        // if we don't get all of the data, the socket was closed.
                        uint waitData = await reader.LoadAsync(sizeof(uint));
                        if(waitData != sizeof(uint))
                        {
                            // We didn't get it all, the socket is closed.
                            break;
                        }
                        // Get the string size
                        uint stringLen = reader.ReadUInt32();

                        // Now wait for the actual string data
                        uint stringWaitData = await reader.LoadAsync(stringLen);
                        if(stringWaitData != stringLen)
                        {
                            // We couldn't read the full string length.
                            break;
                        }

                        // Get the actual string
                        string commandString = reader.ReadString(stringLen);

                        // Deal with the string
                        Command response = ParseStringAndSendCommand(commandString);

                        // Send the response if one was given.
                        await SendResponse(writer, response);
                    }
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Socket Accept Fail: "+e.Message);
                }
            });
            socketHandler.Start();
        }

        private Command ParseStringAndSendCommand(string commandString)
        {
            Command cmd = Newtonsoft.Json.JsonConvert.DeserializeObject<Command>(commandString);
            return m_listener.OnCommand(cmd);
        }

        private async Task SendResponse(DataWriter writer, Command cmd)
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
