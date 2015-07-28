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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Glow.PaneControls
{
    #pragma warning disable CS4014
    public sealed partial class ClockPane : UserControl, IProgramPane
    {
        bool m_programEnabled = false;
        IProgramController m_controller;
        ClockSettings m_settings;

        public ClockPane(IProgramController controller)
        {
            this.InitializeComponent();
            m_controller = controller;

            // Disable the combo box since we don't have it yet
            ui_colorType.IsEnabled = false;

            // Subscribe for connection callbacks
            App.GlowBack.ConnectionManager.OnClientConnected += ConnectionManager_OnClientConnected;

            // Get the current state of the program and settings.
            GetAndSetProgramState();
            GetCurrentClockSettings();
        }

        private void ClockEnable_Toggled(object sender, RoutedEventArgs e)
        {
            if(m_programEnabled == ui_clockEnable.IsOn)
            {
                return;
            }

            m_programEnabled = ui_clockEnable.IsOn;
            m_controller.ToggleProgram(GlowCommon.GlowPrograms.Clock, m_programEnabled);
        }

        public void OnCommand(Command cmd)
        {
            switch(cmd.MessageId)
            {
                case Command.COMMAND_RECIEVE_SETTINGS:
                    // Parse the settings
                    m_settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ClockSettings>(cmd.Message);

                    // Update the UI
                    UpdateClockSettingsUI();
                    break;
            }
        }

        public void OnProgramListChanged()
        {
            // A program has changed, check out state.
            GetAndSetProgramState();
        }

        private async void GetAndSetProgramState()
        {
            m_programEnabled = m_controller.GetProgramState(GlowCommon.GlowPrograms.Clock);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>
            {
                ui_clockEnable.IsOn = m_controller.GetProgramState(GlowCommon.GlowPrograms.Clock);
            });            
        }

        private void ConnectionManager_OnClientConnected()
        {
            // We we get a new connection get the new settings
            GetCurrentClockSettings();
        }

        private void GetCurrentClockSettings()
        {
            new Task(() =>
            {
                Command cmd = new Command();
                cmd.MessageId = Command.COMMAND_GET_SETTINGS;
                cmd.Program = GlowCommon.GlowPrograms.Clock;
                App.GlowBack.ConnectionManager.SendCommand(cmd);
            }).Start();            
        }

        private async void UpdateClockSettingsUI()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_colorType.IsEnabled = true;
                ui_colorType.SelectedIndex = (int)m_settings.ColorType;
            });            
        }

        private void ColorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Return if we don't have the settings yet
            if(m_settings == null)
            {               
                return;
            }

            if((int)ui_colorType.SelectedIndex == (int)m_settings.ColorType)
            {
                return;
            }

            // Set the value
            m_settings.ColorType = (ClockSettings.ColorTypes)ui_colorType.SelectedIndex;

            // Send the command
            new Task(() =>
            {
                Command cmd = new Command();
                cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
                cmd.Program = GlowCommon.GlowPrograms.Clock;
                cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(m_settings);
                App.GlowBack.ConnectionManager.SendCommand(cmd);
            }).Start();
        }
    }
}
