using GlowCommon;
using GlowCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Glow
{
    #pragma warning disable CS4014
    public sealed partial class GlowDiscovery : Page, IDiscoveryServerListener
    {
        private static byte[,] s_colorArray = new byte[,]
        {
            {36, 50, 255},
            {161, 22, 66},
            {171, 255, 171},
            {255, 110, 20},
            {117, 202, 255},
            {134, 73, 191},
            {30, 150, 0}
        };

        // UI vars
        CoreDispatcher m_dispatcher;
        DispatcherTimer m_timer;
        int m_timerTickCount = 0;
        int m_currentColor = s_colorArray.Length;
        bool softDisableButtons = true;

        // Discovery vars
        DiscoveryServer m_discoveryServer;

        public GlowDiscovery()
        {
            this.InitializeComponent();
            this.Loaded += GlowDiscovery_Loaded;
            m_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

            // Hide the animated UI elements
            ui_manualButtom.Opacity = 0;
            ui_titleText.Opacity = 0;
            ui_helpButton.Opacity = 0;
        }

        private void GlowDiscovery_Loaded(object sender, RoutedEventArgs e)
        {
            // Setup the timer
            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0, 0, 5);
            m_timer.Tick += Timer_tick;
            m_timer.Start();

            // Call the function to start the animation now.
            Timer_tick(null, null);

            // Delay the text animation and kick it off
            ui_animTitleText.BeginTime = new TimeSpan(0, 0, 3);
            ui_storyText.Begin();

            new Task(async () =>
            {
                // Don't start the discovery server too soon to it won't
                // interrupt the animations.
                await Task.Delay(TimeSpan.FromSeconds(3));

                m_discoveryServer = new DiscoveryServer(DiscoveryServer.DiscoveryMode.Listen);
                m_discoveryServer.SetListener(this);
            }).Start();
        }

        private void Timer_tick(object sender, object e)
        {
            // Check if we should show the other buttons.
            m_timerTickCount++;
            if (m_timerTickCount == 3)
            {
                ui_storyHelpButton.Begin();
                ui_storyManualButtom.Begin();
                softDisableButtons = false;
            }

            // Update the color count
            m_currentColor++;
            if(m_currentColor >= s_colorArray.GetLength(0))
            {
                m_currentColor = 0;
            }

            // Set the colors
            ui_animCircleColor.From = ((SolidColorBrush)(ui_glowCircle.Fill)).Color;
            ui_animCircleColor.To = Windows.UI.Color.FromArgb(255, s_colorArray[m_currentColor, 0], s_colorArray[m_currentColor, 1], s_colorArray[m_currentColor, 2]);

            // Play the animation.
            ui_animCircleColor.Duration = new TimeSpan(0,0,5);
            ui_storyCircleColor.Begin();
        }

        private void Help_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (softDisableButtons)
            {
                return;
            }
            MessageDialog message = new MessageDialog("We are searching for your glow device. Ensure your device is on and that it is connected to the same local network as this device. If all is good, we should find it in a few seconds.", "Searching...");
            message.ShowAsync();
        }

        private void Manual_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(softDisableButtons)
            {
                return;
            }
            MessageDialog message = new MessageDialog("Not done yet.", "Oops");
            message.ShowAsync();
        }

        public void OnClientFound(string ipAddres)
        {
            // Save the IP address
            App.GlowBack.AppSetting.DeviceIp = ipAddres;

            // Kill the callback from the discovery
            m_discoveryServer.SetListener(null);

            // Run on the UI thread.
            m_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NavigateAwaySuccess();
            });
        }

        private async void NavigateAwaySuccess()
        {
            // Stop the timer
            m_timer.Stop();
            softDisableButtons = true;

            // First fade out the text so we can replace it
            ui_animTitleText.BeginTime = new TimeSpan(0, 0, 0);
            ui_animTitleText.From = ui_titleText.Opacity;
            ui_animTitleText.To = 0;
            ui_storyText.Stop();
            ui_storyText.Begin();

            // Also fade out the buttons if they are in.
            ui_animHelpButton.BeginTime = new TimeSpan(0, 0, 0);
            ui_animHelpButton.From = ui_helpButton.Opacity;
            ui_animHelpButton.To = 0;
            ui_animManualButtom.BeginTime = new TimeSpan(0, 0, 0);
            ui_animManualButtom.From = ui_manualButtom.Opacity;
            ui_animManualButtom.To = 0;

            ui_storyHelpButton.Stop();
            ui_storyManualButtom.Stop();
            ui_storyHelpButton.Begin();
            ui_storyManualButtom.Begin();

            // Sleep to wait for the fade
            await Task.Delay(TimeSpan.FromSeconds(1));

            // We need to swap the text and fade back in.
            ui_titleText.Text = "Found it";
            ui_animTitleText.From = 0;
            ui_animTitleText.To = 1;
            ui_storyText.Begin();

            // Sleep to wait for the fade and to read
            await Task.Delay(TimeSpan.FromSeconds(3));

            // Fade the rest out
            ui_animTitleText.BeginTime = new TimeSpan(0, 0, 0);
            ui_animTitleText.From = ui_titleText.Opacity;
            ui_animTitleText.To = 0;
            ui_animCircleColor.Duration = new TimeSpan(0, 0, 1);
            ui_animCircleColor.To = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            ui_animCircleColor.From = ((SolidColorBrush)(ui_glowCircle.Fill)).Color;

            // Restart the animations
            ui_storyCircleColor.Stop();
            ui_storyText.Stop();
            ui_storyText.Begin();
            ui_storyCircleColor.Begin();

            ui_storyCircleColor.Completed += (object s, object obj) =>
            {
                // The the animation is complete navigate
                this.Frame.Navigate(typeof(MainPage), null);
            };
        }
    }
}
