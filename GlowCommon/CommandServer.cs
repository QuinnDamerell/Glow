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
                            return;
                        }

                        // Get the actual string
                        string commandString = reader.ReadString(stringLen);

                        // Deal with the string
                        ParseStringAndSendCommand(commandString);
                    }
                }
                catch(Exception e)
                {

                }
            });
            socketHandler.Start();
        }

        private void ParseStringAndSendCommand(string commandString)
        {
            Command cmd = Newtonsoft.Json.JsonConvert.DeserializeObject<Command>(commandString);
        }
    }
}
