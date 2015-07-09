using Glow.PageControls;
using Glow.PaneControls;
using GlowCommon;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Glow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainLanding : Page
    {
        Dictionary<GlowPrograms, UserControl> m_userControls = new Dictionary<GlowPrograms, UserControl>();

        public MainLanding()
        {
            this.InitializeComponent();

            m_userControls[GlowPrograms.ManualColors] = new ManualColorPane();
            m_userControls[GlowPrograms.Clock] = new ClockPane();

            SetPane(GlowPrograms.ManualColors);
        }

        private void SetPane(GlowPrograms program)
        {
            if (!m_userControls.ContainsKey(program))
            {
                throw new Exception("That program doesn't have a user control!");
            }
            ui_contentGrid.Children.Clear();
            ui_contentGrid.Children.Add(m_userControls[program]);
            ui_spitView.IsPaneOpen = false;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ui_spitView.IsPaneOpen = !ui_spitView.IsPaneOpen;
        }

        private void MenuText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ui_spitView.IsPaneOpen = !ui_spitView.IsPaneOpen;
        }

        private void ManualColor_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.ManualColors);
        }

        private void ManualColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetPane(GlowPrograms.ManualColors);
        }

        private void Clock_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.Clock);
        }

        private void Clock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetPane(GlowPrograms.Clock);            
        }
    }
}
