using Glow.Interfaces;
using GlowCommon;
using GlowCommon.DataObjects;
using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Glow.PageControls
{
    public sealed partial class ManualColorPane : UserControl, IProgramPane
    {
        /// <summary>
        /// Indicates the status of the program
        /// </summary>
        bool m_programEnabled = false;

        /// <summary>
        /// Holds a reference to the master controller
        /// </summary>
        IProgramController m_controller;

        ManualColorSettings m_lastSettings = new ManualColorSettings();
        bool m_hasUpdates = true;
        CoreDispatcher m_dispatcher;

        public ManualColorPane(IProgramController controller)
        {
            this.InitializeComponent();
            m_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            m_controller = controller;

            // Get our initial state
            GetAndSetProgramState();
        }

        public void OnProgramListChanged()
        {
            // Check our state
            GetAndSetProgramState();
        }

        public void OnCommand(Command cmd)
        {
        }

        private async void ColorPicker_OnColorChanged(object sender, Controls.OnColorChangedArgs e)
        {
            ManualColorSettings newSettings = new ManualColorSettings();
            newSettings.CurrentLedStateList = new List<List<SerlizableLed>>();
            List<SerlizableLed> localList = new List<SerlizableLed>();
            SerlizableLed led = new SerlizableLed(e.Red / 255.0, e.Green / 255.0, e.Blue / 255.0, 1.0, 0);
            localList.Add(led);
            localList.Add(led);
            localList.Add(led);
            localList.Add(led);
            localList.Add(led);

            newSettings.CurrentLedStateList.Add(localList);

            // Send the settings
            await SendNewSettings(newSettings);
        }

        /// <summary>
        /// Does what is says.
        /// </summary>
        private async void GetAndSetProgramState()
        {
            m_programEnabled = m_controller.GetProgramState(GlowCommon.GlowPrograms.ManualColors);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_manualColorEnable.IsOn = m_controller.GetProgramState(GlowCommon.GlowPrograms.ManualColors);
            });
        }

        /// <summary>
        /// Fired when a user toggles the manual color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualColor_Toggled(object sender, RoutedEventArgs e)
        {
            if (m_programEnabled == ui_manualColorEnable.IsOn)
            {
                return;
            }

            m_programEnabled = ui_manualColorEnable.IsOn;
            m_controller.ToggleProgram(GlowCommon.GlowPrograms.ManualColors, m_programEnabled);
        }

        /// <summary>
        /// Sends the new settings to the pie
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private async Task SendNewSettings(ManualColorSettings settings)
        {
            Command cmd = new Command();
            cmd.Program = GlowPrograms.ManualColors;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
            try
            {
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to send update manual color settings " + e.Message);
            }
        }

        #region Live Mode
        ManualColorSettings m_liveColorSettings = new ManualColorSettings();
        bool isSending = false;
        byte[,] m_liveColor = new byte[5,3];
        int m_liveColorUpdateSpeedMs = 500;

        private void ColorPicker_OnColorChanged_0(object sender, Controls.OnColorChangedArgs e)
        {
            m_liveColor[0, 0] = e.Red;
            m_liveColor[0, 1] = e.Green;
            m_liveColor[0, 2] = e.Blue;
            FireLiveTouchChagned();
        }

        private void ColorPicker_OnColorChanged_1(object sender, Controls.OnColorChangedArgs e)
        {
            m_liveColor[1, 0] = e.Red;
            m_liveColor[1, 1] = e.Green;
            m_liveColor[1, 2] = e.Blue;
            FireLiveTouchChagned();
        }

        private void ColorPicker_OnColorChanged_2(object sender, Controls.OnColorChangedArgs e)
        {
            m_liveColor[2, 0] = e.Red;
            m_liveColor[2, 1] = e.Green;
            m_liveColor[2, 2] = e.Blue;
            FireLiveTouchChagned();
        }

        private void ColorPicker_OnColorChanged_3(object sender, Controls.OnColorChangedArgs e)
        {
            m_liveColor[3, 0] = e.Red;
            m_liveColor[3, 1] = e.Green;
            m_liveColor[3, 2] = e.Blue;
            FireLiveTouchChagned();
        }

        private void ColorPicker_OnColorChanged_4(object sender, Controls.OnColorChangedArgs e)
        {
            m_liveColor[4, 0] = e.Red;
            m_liveColor[4, 1] = e.Green;
            m_liveColor[4, 2] = e.Blue;
            FireLiveTouchChagned();
        }

        private void FireLiveTouchChagned()
        {
            // Leave if we are already working.
            lock(m_liveColorSettings)
            {
                if(isSending)
                {
                    return;
                }
                isSending = true;
            }

            MakeLiveSettingsUpdate();
        }

        private async void MakeLiveSettingsUpdate()
        {
            // Make the settings and the list.
            if(m_liveColorSettings.CurrentLedStateList == null)
            {
                m_liveColorSettings.CurrentLedStateList = new List<List<SerlizableLed>>();
            }
            if(m_liveColorSettings.CurrentLedStateList.Count != 1)
            {
                m_liveColorSettings.CurrentLedStateList.Count();
                m_liveColorSettings.CurrentLedStateList.Add(new List<SerlizableLed>());
            }

            m_liveColorSettings.CurrentLedStateList[0].Clear();
            m_liveColorSettings.CurrentLedStateList[0].Add(new SerlizableLed(m_liveColor[0, 0] / 255.0, m_liveColor[0, 1] / 255.0, m_liveColor[0, 2] / 255.0, 1.0, m_liveColorUpdateSpeedMs));
            m_liveColorSettings.CurrentLedStateList[0].Add(new SerlizableLed(m_liveColor[1, 0] / 255.0, m_liveColor[1, 1] / 255.0, m_liveColor[1, 2] / 255.0, 1.0, m_liveColorUpdateSpeedMs));
            m_liveColorSettings.CurrentLedStateList[0].Add(new SerlizableLed(m_liveColor[2, 0] / 255.0, m_liveColor[2, 1] / 255.0, m_liveColor[2, 2] / 255.0, 1.0, m_liveColorUpdateSpeedMs));
            m_liveColorSettings.CurrentLedStateList[0].Add(new SerlizableLed(m_liveColor[3, 0] / 255.0, m_liveColor[3, 1] / 255.0, m_liveColor[3, 2] / 255.0, 1.0, m_liveColorUpdateSpeedMs));
            m_liveColorSettings.CurrentLedStateList[0].Add(new SerlizableLed(m_liveColor[4, 0] / 255.0, m_liveColor[4, 1] / 255.0, m_liveColor[4, 2] / 255.0, 1.0, m_liveColorUpdateSpeedMs));

            // Send the settings
            await SendNewSettings(m_liveColorSettings);
            isSending = false;
        }

        private void LiveMode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FadeLiveMode(true);
        }

        private void Exit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FadeLiveMode(false);
        }

        private void AnimLiveMode_Completed(object sender, object e)
        {
            if(ui_liveModeHolder.Opacity ==0)
            {
                ui_liveModeHolder.Visibility = Visibility.Collapsed;
            }
        }

        private void FadeLiveMode(bool fadeIn)
        {
            ui_liveModeHolder.Visibility = Visibility.Visible;
            ui_liveModeHolder.Opacity = fadeIn ? 0 : 1;
            ui_animLiveMode.To = fadeIn ? 1 : 0;
            ui_animLiveMode.From = fadeIn ? 0 : 1;
            ui_storyLiveMode.Begin();
        }

        private void LiveColorChangeSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_liveColorUpdateSpeedMs = (int)e.NewValue * 100;
            if (ui_liveColorChangeSpeedText != null)
            {
                ui_liveColorChangeSpeedText.Text = m_liveColorUpdateSpeedMs > 1000 ? $"{m_liveColorUpdateSpeedMs / 1000}s" : $"{m_liveColorUpdateSpeedMs}ms";
            }
        }

        #endregion


    }
}
