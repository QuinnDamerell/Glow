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
    public sealed partial class GlowDiscovery : Page
    {
        private static byte[,] s_colorArray = new byte[,]
        {
            {36, 50, 180},
            {161, 22, 66},
            {171, 180, 171},
            {210, 110, 20},
            {117, 202, 200},
            {134, 73, 191},
            {30, 150, 0}
        };

        // UI Vars
        CoreDispatcher m_dispatcher;
        DispatcherTimer m_timer;

        // Keeps track of the current ball color
        int m_currentColor = s_colorArray.Length;

        // Used to keep track of how many times the UI timer has fired
        int m_timerTickCount = 0;

        // Disables the buttons
        bool m_softDisableButtons = true;

        /// <summary>
        /// Set true when the UI is in a state where it is safe to leave if
        /// we have a connection.
        /// </summary>
        bool m_readyToNavigate = false;

        // Set true when the buttons shouldn't be shown.
        bool m_dontShowButtons = false;

        public GlowDiscovery()
        {
            this.InitializeComponent();
            this.Loaded += GlowDiscovery_Loaded;
            m_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

            // Hide the animated UI elements
            ui_manualButtom.Opacity = 0;
            ui_titleText.Opacity = 0;
            ui_helpButton.Opacity = 0;

            // Subscribe to the connected event.
            App.GlowBack.ConnectionManager.OnClientConnected += ConnectionManager_OnClientConnected;
        }

        /// <summary>
        /// Fired when a client is connected and we are ready to move on.
        /// </summary>
        private void ConnectionManager_OnClientConnected()
        {
            // Unsub from the event so we don't get it again
            App.GlowBack.ConnectionManager.OnClientConnected -= ConnectionManager_OnClientConnected;

            // If we are ready and we get connected then go go go.
            if (m_readyToNavigate)
            {
                // Run on the UI thread.
                m_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NavigateAwaySuccess();
                });
            }
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
            ui_animTitleText.BeginTime = new TimeSpan(0, 0, App.GlowBack.AppSetting.ShowShortIntro ? 3 : 1);
            ui_storyText.Completed += StoryText_Complete;
            ui_storyText.Begin();
        }

        /// <summary>
        /// Fired when the title text has finished the first time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoryText_Complete(object sender, object e)
        {
            // Remove the listener so we don't get called back again.
            ui_storyText.Completed -= StoryText_Complete;

            // Check if we are ready now, if so green light.
            if(App.GlowBack.ConnectionManager.IsConnected)
            {
                NavigateAwaySuccess();
            }

            // If not set the bool to true so when we
            m_readyToNavigate = true;
        }

        private void Timer_tick(object sender, object e)
        {
            // Check if we should show the other buttons, only show them if time time is right
            // and we aren't going to leave.
            m_timerTickCount++;
            if (m_timerTickCount == 3 && !m_dontShowButtons)
            {
                ui_storyHelpButton.Begin();
                ui_storyManualButtom.Begin();
                m_softDisableButtons = false;
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

        /// <summary>
        /// Fired when the help button is tapped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Help_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (m_softDisableButtons)
            {
                return;
            }
            MessageDialog message = new MessageDialog("We are searching for your glow device. Ensure your device is on, Glow is running, and the device is connected to the same local network as this device. If all is good, we should find it in a few seconds.", "Searching...");
            message.ShowAsync();
        }

        private void Manual_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(m_softDisableButtons)
            {
                return;
            }

            Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/QuinnDamerell/Glow", UriKind.Absolute));
        }

        /// <summary>
        /// Called when a client was found and we want to move on.
        /// </summary>
        private async void NavigateAwaySuccess()
        {
            // Kill the buttons
            m_softDisableButtons = true;
            m_dontShowButtons = true;

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
            await Task.Delay(TimeSpan.FromSeconds(App.GlowBack.AppSetting.ShowShortIntro ? 3 : 1));

            // Stop the ball animation timer
            m_timer.Stop();

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

            // Since we found one do it more quickly next time.
            App.GlowBack.AppSetting.ShowShortIntro = false;

            ui_storyCircleColor.Completed += (object s, object obj) =>
            {
                // Unsub from the event so we don't fire it.
                App.GlowBack.ConnectionManager.OnClientConnected -= ConnectionManager_OnClientConnected;

                // The the animation is complete navigate
                this.Frame.Navigate(typeof(MainLanding), null);
            };
        }
    }
}
