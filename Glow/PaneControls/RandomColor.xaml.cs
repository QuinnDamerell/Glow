using Glow.Interfaces;
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
using GlowCommon.DataObjects;
using Windows.UI.Core;
using System.Threading.Tasks;
using Newtonsoft.Json;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Glow.PaneControls
{
    public sealed partial class RandomColor : UserControl, IProgramPane
    {
        RandomColorSettings m_settings = new RandomColorSettings();
        IProgramController m_controller;     
        bool m_isEnabled = false;

        public RandomColor(IProgramController controller)
        {
            this.InitializeComponent();
            m_controller = controller;

            // Set the program state
            GetAndSetProgramState();

            // Get the settings from the server
            GetCurrentRandomSettings();

            // Listen for reconnects
            App.GlowBack.ConnectionManager.OnClientConnected += ConnectionManager_OnClientConnected;
        }

        /// <summary>
        /// Fired when a user is reconnected.
        /// </summary>
        private void ConnectionManager_OnClientConnected()
        {
            GetCurrentRandomSettings();
        }

        /// <summary>
        /// Fired when we get a response from the server
        /// </summary>
        /// <param name="cmd"></param>
        public void OnCommand(Command cmd)
        {
            switch (cmd.MessageId)
            {
                case Command.COMMAND_RECIEVE_SETTINGS:
                    // Parse the settings
                    m_settings = Newtonsoft.Json.JsonConvert.DeserializeObject<RandomColorSettings>(cmd.Message);

                    // Update the UI
                    UpdateRandomColorSettingsUi();
                    break;
            }
        }

        /// <summary>
        /// Requests the current settings from the server
        /// </summary>
        private void GetCurrentRandomSettings()
        {
            new Task(async () =>
            {
                Command cmd = new Command();
                cmd.MessageId = Command.COMMAND_GET_SETTINGS;
                cmd.Program = GlowCommon.GlowPrograms.RandomColor;
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }).Start();
        }

        /// <summary>
        /// Updates the UI with the current settings
        /// </summary>
        private async void UpdateRandomColorSettingsUi()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if(m_settings == null)
                {
                    return;
                }

                // Update the UI
                ui_maxBetweenChange.Value = m_settings.NextUpdateTimeMaxMs / 100;
                ui_minBetweenChange.Value = m_settings.NextUpdateTimeMinMs / 100;
                ui_maxForColorChange.Value = m_settings.TransistionMaxMs / 100;
                ui_minForColorChange.Value = m_settings.TransistionMinMs / 100;
                ui_maxBetweenChangeText.Text = SetTimeText((int)ui_maxBetweenChange.Value * 100);
                ui_minBetweenChangeText.Text = SetTimeText((int)ui_minBetweenChange.Value * 100);
                ui_maxForColorChangeText.Text = SetTimeText((int)ui_maxForColorChange.Value * 100);
                ui_minForColorChangeText.Text = SetTimeText((int)ui_minForColorChange.Value * 100);
            });
        }

        /// <summary>
        /// Formats the string into a time.
        /// </summary>
        /// <param name="timeInMs"></param>
        /// <returns></returns>
        private string SetTimeText(int timeInMs)
        {
            if(timeInMs < 1000)
            {
                return $"{timeInMs} ms";
            }
            else
            {
                return $"{timeInMs / 1000.0} s";
            }
        }

        /// <summary>
        /// Sends new settings to the server
        /// </summary>
        private void SendNewSettings()
        {
            // Kick off a task to send the message.
            new Task(async () =>
            {
                Command cmd = new Command();
                cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
                cmd.Program = GlowCommon.GlowPrograms.RandomColor;
                cmd.Message = JsonConvert.SerializeObject(m_settings);
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }).Start();

            // Update the UI
            UpdateRandomColorSettingsUi();
        }

        /// <summary>
        /// Fired when a user moves a slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinTimeBetweenChange_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)(e.NewValue * 100);
            if (m_settings.NextUpdateTimeMinMs == newValue)
            {
                return;
            }
            m_settings.NextUpdateTimeMinMs = newValue;
            if(m_settings.NextUpdateTimeMinMs > m_settings.NextUpdateTimeMaxMs)
            {
                m_settings.NextUpdateTimeMaxMs = m_settings.NextUpdateTimeMinMs;
            }
            SendNewSettings();
        }

        /// <summary>
        /// Fired when a user moves a slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxBetweenChange_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)(e.NewValue * 100);
            if (m_settings.NextUpdateTimeMaxMs == newValue)
            {
                return;
            }
            m_settings.NextUpdateTimeMaxMs = newValue;
            if (m_settings.NextUpdateTimeMinMs > m_settings.NextUpdateTimeMaxMs)
            {
                m_settings.NextUpdateTimeMinMs = m_settings.NextUpdateTimeMaxMs;
            }
            SendNewSettings();
        }

        /// <summary>
        /// Fired when a user moves a slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinForColorChange_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)(e.NewValue * 100);
            if (m_settings.TransistionMinMs == newValue)
            {
                return;
            }
            m_settings.TransistionMinMs = newValue;
            if (m_settings.TransistionMinMs > m_settings.TransistionMaxMs)
            {
                m_settings.TransistionMaxMs = m_settings.TransistionMinMs;
            }
            SendNewSettings();

        }

        /// <summary>
        /// Fired when a user moves a slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxForColorChange_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            int newValue = (int)(e.NewValue * 100);
            if (m_settings.TransistionMaxMs == newValue)
            {
                return;
            }
            m_settings.TransistionMaxMs = newValue;
            if (m_settings.TransistionMinMs > m_settings.TransistionMaxMs)
            {
                m_settings.TransistionMinMs = m_settings.TransistionMaxMs;
            }
            SendNewSettings();
        }

        /// <summary>
        /// Fired when a state changes.
        /// </summary>
        public void OnProgramListChanged()
        {
            GetAndSetProgramState();
        }

        /// <summary>
        /// Updates the program state
        /// </summary>
        private async void GetAndSetProgramState()
        {
            m_isEnabled = m_controller.GetProgramState(GlowCommon.GlowPrograms.RandomColor);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_randomColorToggle.IsOn = m_controller.GetProgramState(GlowCommon.GlowPrograms.RandomColor);
            });
        }

        /// <summary>
        /// The user has updated the program state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandomColorEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            m_controller.ToggleProgram(GlowCommon.GlowPrograms.RandomColor, ui_randomColorToggle.IsOn);
        } 
    }
}
