﻿using System;
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
    public sealed partial class MainPage : Page
    {
        DispatcherTimer m_timer;
        ManualColorSettings m_lastSettings = new ManualColorSettings();
        bool m_hasUpdates = true;
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
        }

        private void SetStatus(string text)
        {
#pragma warning disable CS4014 
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
            Command cmd = new Command();
            cmd.Program = GlowPrograms.ManualColors;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(settings);

            await App.GlowBack.ConnectionManager.SendCommand(cmd);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_hasUpdates = true;
        }
    }
}
