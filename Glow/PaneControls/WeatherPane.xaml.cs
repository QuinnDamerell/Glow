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
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using GlowCommon;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Glow.PaneControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WeatherPane : Page, IProgramPane
    {
        IProgramController m_controller;
        bool m_isEnabled = false;
        WeatherSettings m_settings = new WeatherSettings();

        public WeatherPane(IProgramController controller)
        {
            this.InitializeComponent();
            m_controller = controller;
            GetAndSetProgramState();

            GetGpsLocation();
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
            m_isEnabled = m_controller.GetProgramState(GlowCommon.GlowPrograms.Weather);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_weatherEnabled.IsOn = m_controller.GetProgramState(GlowCommon.GlowPrograms.Weather);
            });
        }

        private void WeatherEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            m_controller.ToggleProgram(GlowCommon.GlowPrograms.Weather, ui_weatherEnabled.IsOn);
        }

        private async void GetGpsLocation()
        {
            // Get the location
            try
            {
                // Get the point   
                Geolocator m_geoLocator = new Geolocator();
                await Geolocator.RequestAccessAsync();
                Geoposition position = await m_geoLocator.GetGeopositionAsync();

                // Get the address
                MapLocationFinderResult mapLocationFinderResult = await MapLocationFinder.FindLocationsAtAsync(position.Coordinate.Point);
                if (mapLocationFinderResult.Status != MapLocationFinderStatus.Success)
                {
                    throw new Exception();
                }

                WeatherSettings.Location loc = new WeatherSettings.Location
                {
                    City = mapLocationFinderResult.Locations[0].Address.Town,
                    State = mapLocationFinderResult.Locations[0].Address.Region
                };
                m_settings.CurrentLocation = loc;
                ui_locationText.Text = "Your Location: " + mapLocationFinderResult.Locations[0].Address.Town;

                // Send the location to the pie
                await SendNewSettings(m_settings);
            }
            catch(Exception)
            {
                ui_locationText.Text = "Failed to get your location!";
            }
        }

        private async Task SendNewSettings(WeatherSettings settings)
        {
            Command cmd = new Command();
            cmd.Program = GlowPrograms.Weather;
            cmd.MessageId = Command.COMMAND_RECIEVE_SETTINGS;
            cmd.Message = Newtonsoft.Json.JsonConvert.SerializeObject(settings);

            try
            {
                await App.GlowBack.ConnectionManager.SendCommand(cmd);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to send weather settings " + e.Message);
            }
        }
    }
}
