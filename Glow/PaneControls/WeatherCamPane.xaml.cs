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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Glow.PaneControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WeatherCamPane : Page, IProgramPane
    {
        IProgramController m_controller;
        bool m_isEnabled = false;

        public WeatherCamPane(IProgramController controller)
        {
            this.InitializeComponent();
            m_controller = controller;

            GetAndSetProgramState();
        }

        public void OnCommand(Command cmd)
        {

        }

        public void OnProgramListChanged()
        {
            GetAndSetProgramState();
        }

        private async void GetAndSetProgramState()
        {
            m_isEnabled = m_controller.GetProgramState(GlowCommon.GlowPrograms.WeatherCam);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_weatherCamEnabled.IsOn = m_controller.GetProgramState(GlowCommon.GlowPrograms.WeatherCam);
            });
        }

        private void WeatherCamEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            m_controller.ToggleProgram(GlowCommon.GlowPrograms.WeatherCam, ui_weatherCamEnabled.IsOn);
        }
    }
}
