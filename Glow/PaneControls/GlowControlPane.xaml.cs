using Glow.Interfaces;
using GlowCommon;
using GlowCommon.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Glow.PaneControls
{
    public sealed partial class GlowControlPane : UserControl, IProgramPane
    {
        IGlowControlListener m_listener;

        GlowControlSettings m_settings;

        public GlowControlPane(IGlowControlListener listener)
        {
            this.InitializeComponent();

            m_listener = listener;

            // Register for callbacks when we get new connections.
            App.GlowBack.ConnectionManager.OnClientConnected += ConnectionManager_OnClientConnected;

            // We we are created kick off a thread to get the current state.
            QueryCurrentState();
        }

        public void OnCommand(Command cmd)
        {
            switch (cmd.MessageId)
            {
                case Command.COMMAND_RECIEVE_SETTINGS:
                    ProcessSettingsRecieved(cmd.Message);
                    break;
            }
        }

        /// <summary>
        /// Called by the main page when a program asks if it is enabled
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public bool IsProgramEnabled(GlowPrograms program)
        {
            if(m_settings == null)
            {
                return false;
            }

            if(m_settings.ProgramStateList.ContainsKey(program) && m_settings.ProgramStateList[program] == GlowControlSettings.ProgramState.Eligible)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called by the control when a program is toggled.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="enabled"></param>
        public void ToggleProgram(GlowPrograms program, bool enabled)
        {
            if(m_settings == null)
            {
                // Since we can't do it, fire this so it will be toggled back.
                m_listener.OnEnabledProgramsChanged();
                return;
            }

            // Update the program
            m_settings.ProgramStateList[program] = enabled ? GlowControlSettings.ProgramState.Eligible : GlowControlSettings.ProgramState.Disabled;

            // Send updated settings
            SendUpdatedSettings();
        }

        private void ConnectionManager_OnClientConnected()
        {
            // When we get a new connection update our state.
            QueryCurrentState();
        }

        private void ProcessSettingsRecieved(string json)
        {
            // Parse the new settings.
            m_settings = Newtonsoft.Json.JsonConvert.DeserializeObject<GlowControlSettings>(json);

            // Update the UI
            UpdateUI();

            // Inform the controller that the program list has changed.
            m_listener.OnEnabledProgramsChanged();
        }

        private void QueryCurrentState()
        {
            new Task(async () =>
            {
                Command cmd = new Command();
                cmd.MessageId = Command.COMMAND_GET_SETTINGS;
                cmd.Program = GlowCommon.GlowPrograms.GlowControl;
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }).Start();
        }

        private void SendUpdatedSettings()
        {
            new Task(async () =>
            {
                Command cmd = new Command();
                cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(m_settings);
                cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
                cmd.Program = GlowCommon.GlowPrograms.GlowControl;
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }).Start();
        }

        private async void UpdateUI()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_masterSlider.Value = m_settings.MasterIntensity * 100;
                ui_sleepyTimeBrightness.Value = m_settings.SleepyTimeIntensity * 100;
                ui_sleepyOnTime.Time = m_settings.SleepyTimeStart;
                ui_sleepyOffTime.Time = m_settings.SleepyTimeEnd;
            });
        }

        public void OnProgramListChanged()
        {
            // Ignore this.
        }

        private void MasterSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_settings.MasterIntensity = ui_masterSlider.Value / 100;

            // Send an update to the settings
            SendUpdatedSettings(); 
        }

        private void SleepyTimeBrightness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_settings.SleepyTimeIntensity = ui_sleepyTimeBrightness.Value / 100;

            // Send an update to the settings
            SendUpdatedSettings();
        }

        private void SleepyOnTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            m_settings.SleepyTimeStart = ui_sleepyOnTime.Time;

            // Send an update to the settings
            SendUpdatedSettings();
        }

        private void SleepyOffTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            m_settings.SleepyTimeEnd = ui_sleepyOffTime.Time;

            // Send an update to the settings
            SendUpdatedSettings();
        }
    }
}
