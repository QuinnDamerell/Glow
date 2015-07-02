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
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using GlowCommon.DataObjects;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Glow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            StreamSocket socket = new StreamSocket();
            HostName name = new HostName("localhost");
            await socket.ConnectAsync(name, "22112");
            DataWriter writer = new DataWriter(socket.OutputStream);
            Command cmd = new Command();
            cmd.test = "hahahahaha";
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(cmd);
            writer.WriteUInt32((uint)json.Length);
            writer.WriteString(json);
        }
    }
}
