using GlowCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using GlowCommon.DataObjects;

namespace GlowCommon
{
    public delegate void ConnectionEvent();

    public delegate void CommandRecieved(Command command);

    public class AppConnectionManager : IDiscoveryServerListener, ICommandServerListener
    {
        /// <summary>
        /// Indicates if we are connected or not.
        /// </summary>
        public bool IsConnected = false;

        /// <summary>
        /// Fired when a client is connected
        /// </summary>
        public event ConnectionEvent OnClientConnected;

        /// <summary>
        /// Fired when a client is disconnected
        /// </summary>
        public event ConnectionEvent OnClientDisconnected;

        /// <summary>
        /// Fired when a message is received
        /// </summary>
        public event CommandRecieved OnCommandRecieved;

        //
        // Private Vars
        //
        DiscoveryServer m_discoveryServer;
        ThreadPoolTimer m_discoveryPingTimer;
        CommandServer m_commandServer;
        bool m_isPendingConnection = false;
        object objectLock = new object();

        public AppConnectionManager()
        {
            // When created create a discover server to listen for servers
            m_discoveryServer = new DiscoveryServer(DiscoveryServer.DiscoveryMode.Listen);
            m_discoveryServer.SetListener(this);

            CreateDiscoveryPingTimmer();
        }

        /// <summary>
        /// Called when a server is found! Try to connect.
        /// </summary>
        /// <param name="ipAddress"></param>
        public void OnClientFound(string ipAddress)
        {
            lock(objectLock)
            {
                // Only let one attempt go through at a time
                if(m_isPendingConnection || IsConnected)
                {
                    return;
                }
                m_isPendingConnection = true;
            }

            // Disable the discovery ping
            DestoryDiscoverPingTimer();

            // We have the IP! Try to connect!
            m_commandServer = new CommandServer(this, CommandServer.CommmandServerMode.Client, ipAddress);
        }

        /// <summary>
        /// Fired when a new server connection has been made
        /// </summary>
        public void OnConnect()
        {
            // We connected! We are ready!
            lock (objectLock)
            {
                IsConnected = true;
                m_isPendingConnection = false;
            }

            if(OnClientConnected != null)
            {
                OnClientConnected();
            }            
        }

        /// <summary>
        /// Fired when a server connection has been lost
        /// </summary>
        public void OnDisconnected()
        {
            // We disconnected. O no.
            lock (objectLock)
            {
                m_isPendingConnection = false;
                IsConnected = false;
            }

            // Enable the ping timer to attempt to reconnect.
            CreateDiscoveryPingTimmer();

            // Kill the connection
            m_commandServer = null;

            if (OnClientDisconnected != null)
            {
                OnClientDisconnected();
            }
        }

        /// <summary>
        /// Fired when there has been an error we can't recover from.
        /// </summary>
        public void OnFatalError()
        {
            // This is bad
            // #todo handle
        }

        /// <summary>
        /// Fired when the server sends a command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Command OnCommand(Command command)
        {
            if(OnCommandRecieved != null)
            {
                OnCommandRecieved(command);
            }
            return null;
        }

        /// <summary>
        /// Called by the consumer when they want to send a message to the other side.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public async Task<bool> SendCommand(Command cmd)
        {
            lock (objectLock)
            {
                if(!IsConnected || m_commandServer == null)
                {
                    return false;
                }
            }

            // Send the message
            return await m_commandServer.SendMessage(cmd);
        }


        /// <summary>
        /// Used to create a tick timer for the broadcast pinger
        /// </summary>
        private void CreateDiscoveryPingTimmer()
        {
            // Create the timer
            m_discoveryPingTimer = ThreadPoolTimer.CreatePeriodicTimer(async (ThreadPoolTimer source) =>
            {
                // When the timer fires try to send a message.
                await m_discoveryServer.InvokeBroadcast();
            },
            // Fire every 2 seconds
            new TimeSpan(0, 0, 2));
        }

        /// <summary>
        /// Used to stop the timer for the ping timer.
        /// </summary>
        private void DestoryDiscoverPingTimer()
        {
            m_discoveryPingTimer.Cancel();
        }
    }
}
