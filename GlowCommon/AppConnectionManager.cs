﻿using GlowCommon.Interfaces;
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

        private void DestoryDiscoverPingTimer()
        {
            m_discoveryPingTimer.Cancel();
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

            if (OnClientConnected != null)
            {
                OnClientConnected();
            }
        }

        public void OnFatalError()
        {
            // This is bad
            // #todo handle
        }

        public Command OnCommand(Command command)
        {
            // #todo handle   
            return null;
        }

        public async Task<bool> SendCommand(Command cmd)
        {
            lock (objectLock)
            {
                if(!IsConnected)
                {
                    return false;
                }
            }

            // Send the message
            return await m_commandServer.SendMessage(cmd);
        }
    }
}