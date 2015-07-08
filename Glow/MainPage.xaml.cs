using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using GlowCommon.DataObjects;
using GlowCommon;
using System.Threading.Tasks;
using Windows.UI;
using GlowCommon.Interfaces;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Glow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDiscoveryServerListener
    {
        DispatcherTimer m_timer;
        ManualColorSettings m_lastSettings = new ManualColorSettings();
        DataWriter m_socketDataWriter = null;
        StreamSocket m_socket = null;
        bool m_hasUpdates = true;
        DiscoveryServer m_discovery;
        CoreDispatcher m_dispatcher;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            m_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0,0,0,0,50);
            m_timer.Tick += Timer_Tick;
            m_timer.Start();

            // Setup discovery
            m_discovery = new DiscoveryServer(DiscoveryServer.DiscoveryMode.Listen);
            m_discovery.SetListener(this);
            SetStatus("Listening...");
        }

        private void SetStatus(string text)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            m_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                 u_StatusText.Text = "Status: " + text;
             });
            
        }
        
        // Gather the values on the timer tick
        private async void Timer_Tick(object sender, object e)
        {
            // Don't update unless we have something
            if(!m_hasUpdates)
            {
                return;
            }
            m_hasUpdates = false;

            ManualColorSettings newSettings = new ManualColorSettings();
            newSettings.CurrentLedState.Add(new SerlizableLed(GetValue(u_led1r), GetValue(u_led1g), GetValue(u_led1b), GetValue(u_led1i)));
            newSettings.CurrentLedState.Add(new SerlizableLed(GetValue(u_led2r), GetValue(u_led2g), GetValue(u_led2b), GetValue(u_led2i)));
            newSettings.CurrentLedState.Add(new SerlizableLed(GetValue(u_led3r), GetValue(u_led3g), GetValue(u_led3b), GetValue(u_led3i)));
            newSettings.CurrentLedState.Add(new SerlizableLed(GetValue(u_led4r), GetValue(u_led4g), GetValue(u_led4b), GetValue(u_led4i)));
            newSettings.CurrentLedState.Add(new SerlizableLed(GetValue(u_led5r), GetValue(u_led5g), GetValue(u_led5b), GetValue(u_led5i)));

            // update the UI
            byte r = (byte)(GetValue(u_led1r) * GetValue(u_led1i) * 255);
            byte g = (byte)(GetValue(u_led1g) * GetValue(u_led1i) * 255);
            byte b = (byte)(GetValue(u_led1b) * GetValue(u_led1i) * 255);
            u_led1.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));

            r = (byte)(GetValue(u_led2r) * GetValue(u_led2i) * 255);
            g = (byte)(GetValue(u_led2g) * GetValue(u_led2i) * 255);
            b = (byte)(GetValue(u_led2b) * GetValue(u_led2i) * 255);
            u_led2.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));

            r = (byte)(GetValue(u_led3r) * GetValue(u_led3i) * 255);
            g = (byte)(GetValue(u_led3g) * GetValue(u_led3i) * 255);
            b = (byte)(GetValue(u_led3b) * GetValue(u_led3i) * 255);
            u_led3.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));

            r = (byte)(GetValue(u_led4r) * GetValue(u_led4i) * 255);
            g = (byte)(GetValue(u_led4g) * GetValue(u_led4i) * 255);
            b = (byte)(GetValue(u_led4b) * GetValue(u_led4i) * 255);
            u_led4.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));

            r = (byte)(GetValue(u_led5r) * GetValue(u_led5i) * 255);
            g = (byte)(GetValue(u_led5g) * GetValue(u_led5i) * 255);
            b = (byte)(GetValue(u_led5b) * GetValue(u_led5i) * 255);
            u_led5.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));

            // Send the settings
            await SendNewSettings(newSettings);
            m_lastSettings = newSettings;
        }

        private double GetValue(Slider slider)
        {
            return slider.Value / 100;
        }

        private async Task SendNewSettings(ManualColorSettings settings)
        {
            if(m_socketDataWriter == null)
            {
                return;
            }

            Command cmd = new Command();
            cmd.Program = GlowPrograms.ManualColors;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(settings);

            // Serialize the cmd
            string cmdJson = Newtonsoft.Json.JsonConvert.SerializeObject(cmd);
            m_socketDataWriter.WriteUInt32((uint)cmdJson.Length);
            m_socketDataWriter.WriteString(cmdJson);
            await m_socketDataWriter.StoreAsync();
            await m_socketDataWriter.FlushAsync();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_hasUpdates = true;
        }

        public async void OnClientFound(string ipAddres)
        {
            SetStatus("Connecting...");
            try
            {
                m_socket = new StreamSocket();
                HostName name = new HostName(ipAddres);
                await m_socket.ConnectAsync(name, CommandServer.GLOW_SERVER_PORT + "");
                SetStatus("Connected to "+ipAddres);
                m_socketDataWriter = new DataWriter(m_socket.OutputStream);

                // Force an update.
                m_hasUpdates = true;
            }
            catch (Exception ex)
            {
                SetStatus("Failed - " + ex.Message);
            }
        }
    }
}
