using Glow.Interfaces;
using GlowCommon;
using GlowCommon.DataObjects;
using Microsoft.Band;
using System;
using System.Collections.Generic;
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

        DispatcherTimer m_timer;
        ManualColorSettings m_lastSettings = new ManualColorSettings();
        bool m_hasUpdates = true;
        CoreDispatcher m_dispatcher;

        public ManualColorPane(IProgramController controller)
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            m_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            m_controller = controller;

            // Get our inital state
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


        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            m_timer.Tick += Timer_Tick;
            m_timer.Start();
        }

        // Gather the values on the timer tick
        private async void Timer_Tick(object sender, object e)
        {
            // Don't update unless we have something
            if (!m_hasUpdates)
            {
                return;
            }
            m_hasUpdates = false;

            ManualColorSettings newSettings = new ManualColorSettings();
            newSettings.CurrentLedState = new List<SerlizableLed>();
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

            try
            {
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }
            catch (Exception e)
            {

            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_hasUpdates = true;
        }

        private async void GetAndSetProgramState()
        {
            m_programEnabled = m_controller.GetProgramState(GlowCommon.GlowPrograms.ManualColors);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_manualColorEnable.IsOn = m_controller.GetProgramState(GlowCommon.GlowPrograms.ManualColors);
            });
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (m_programEnabled == ui_manualColorEnable.IsOn)
            {
                return;
            }

            m_programEnabled = ui_manualColorEnable.IsOn;
            m_controller.ToggleProgram(GlowCommon.GlowPrograms.ManualColors, m_programEnabled);
        }
    }
}
