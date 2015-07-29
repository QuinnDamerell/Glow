using Glow.Interfaces;
using Glow.PageControls;
using Glow.PaneControls;
using GlowCommon;
using GlowCommon.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Glow
{
#pragma warning disable CS4014
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainLanding : Page, IProgramController, IGlowControlListener
    {
        /// <summary>
        /// Used to hold references to the controls
        /// </summary>
        Dictionary<GlowPrograms, IProgramPane> m_paneControls = new Dictionary<GlowPrograms, IProgramPane>();

        /// <summary>
        /// Used to hold the relation ship of program to nav pos
        /// </summary>
        Dictionary<GlowPrograms, int> m_glowProgramNavIndex = new Dictionary<GlowPrograms, int>();
       
        /// <summary>
        /// Holds which program we want to switch to
        /// </summary>
        GlowPrograms m_programToSwtich = GlowPrograms.None;

        public MainLanding()
        {
            this.InitializeComponent();
            this.Loaded += MainLanding_Loaded;

            // Hide the UI
            ui_pageRoot.Opacity = 0;

            // Register for commands from the connection manager
            App.GlowBack.ConnectionManager.OnCommandRecieved += ConnectionManager_OnCommandRecieved;
            App.GlowBack.ConnectionManager.OnClientConnected += ConnectionManager_OnClientConnected;
            App.GlowBack.ConnectionManager.OnClientDisconnected += ConnectionManager_OnClientDisconnected;

            m_paneControls[GlowPrograms.GlowControl] = new GlowControlPane(this);
            m_paneControls[GlowPrograms.ManualColors] = new ManualColorPane(this);
            m_paneControls[GlowPrograms.Clock] = new ClockPane(this);
            m_paneControls[GlowPrograms.Weather] = new WeatherPane(this);
            m_paneControls[GlowPrograms.WeatherCam] = new WeatherCamPane(this);
            m_paneControls[GlowPrograms.RandomColor] = new RandomColor(this);

            m_glowProgramNavIndex.Add(GlowPrograms.GlowControl, 0);
            m_glowProgramNavIndex.Add(GlowPrograms.ManualColors, 1);
            m_glowProgramNavIndex.Add(GlowPrograms.Clock, 2);
            m_glowProgramNavIndex.Add(GlowPrograms.Weather, 3);
            m_glowProgramNavIndex.Add(GlowPrograms.WeatherCam, 4);
            m_glowProgramNavIndex.Add(GlowPrograms.RandomColor, 5);


            Storyboard.SetTarget(ui_animContentGrid, ui_contentGrid);
            SetPane(App.GlowBack.AppSetting.LastShownProgram);           
        }


        private void MainLanding_Loaded(object sender, RoutedEventArgs e)
        {
            // When we are loaded animate in
            ui_storyPageRoot.Begin();
        }

        private void SetPane(GlowPrograms program)
        {
            if (!m_paneControls.ContainsKey(program))
            {
                throw new Exception("That program doesn't have a user control!");
            }

            // If we are already going get out of here.
            if (m_programToSwtich != GlowPrograms.None)
            {
                return;
            }

            // Cache the program
            m_programToSwtich = program;
            App.GlowBack.AppSetting.LastShownProgram = m_programToSwtich;

            // Fade out
            ui_animContentGrid.From = 1;
            ui_animContentGrid.To = 0;            
            ui_storyContentGrid.Begin();
            ui_spitView.IsPaneOpen = false;
        }

        private void AnimContentGrid_Completed(object sender, object e)
        {
            if(m_programToSwtich != GlowPrograms.None)
            {
                // Switch and fade in
                ui_contentGrid.Children.Clear();
                ui_contentGrid.Children.Add((UIElement)m_paneControls[m_programToSwtich]);
                m_programToSwtich = GlowPrograms.None;

                ui_animContentGrid.From = 0;
                ui_animContentGrid.To = 1;
                ui_storyContentGrid.Begin();

                // Side the side bar color
                UpdateProgramNavUI();
            }
        }

        private void GlowButton_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.GlowControl);
        }

        private void ManualColor_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.ManualColors);
        }

        private void Clock_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.Clock);
        }

        private void Weather_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.Weather);
        }

        private void WeatherCam_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.WeatherCam);
        }

        private void RandomColor_Click(object sender, RoutedEventArgs e)
        {
            SetPane(GlowPrograms.RandomColor);
        }

        /// <summary>
        /// Called by the programs to get their current state.
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public bool GetProgramState(GlowPrograms program)
        {
            return ((GlowControlPane)m_paneControls[GlowPrograms.GlowControl]).IsProgramEnabled(program);
        }

        /// <summary>
        /// Called by a program when they want to toggle their state.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="enable"></param>
        public void ToggleProgram(GlowPrograms program, bool enable)
        {
            ((GlowControlPane)m_paneControls[GlowPrograms.GlowControl]).ToggleProgram(program, enable);
            UpdateProgramNavUI();
        }

        /// <summary>
        /// Called by the glow controller when the program enabled list has changed.
        /// </summary>
        public void OnEnabledProgramsChanged()
        {
            foreach(KeyValuePair<GlowPrograms,IProgramPane> program in m_paneControls)
            {
                program.Value.OnProgramListChanged();

                UpdateProgramNavUI();
            }
        }

        private void UpdateProgramNavUI()
        {
            // Update the UI
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (KeyValuePair<GlowPrograms, IProgramPane> program in m_paneControls)
                {
                    SolidColorBrush transparent = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    SolidColorBrush active = new SolidColorBrush(Color.FromArgb(100, 0, 120, 215));

                    if(App.GlowBack.AppSetting.LastShownProgram == program.Key)
                    {
                        transparent = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
                        active = new SolidColorBrush(Color.FromArgb(200, 0, 120, 215));
                    }

                    if (m_glowProgramNavIndex.ContainsKey(program.Key))
                    {
                        switch (m_glowProgramNavIndex[program.Key])
                        {
                            case 0:
                                ui_glowButton.Background = GetProgramState(program.Key) ? active : transparent;
                                break;
                            case 1:
                                ui_manualColorButton.Background = GetProgramState(program.Key) ? active : transparent;
                                break;
                            case 2:
                                ui_clockButton.Background = GetProgramState(program.Key) ? active : transparent;
                                break;
                            case 3:
                                ui_weatherButton.Background = GetProgramState(program.Key) ? active : transparent;
                                break;
                            case 4:
                                ui_weatherCamButton.Background = GetProgramState(program.Key) ? active : transparent;
                                break;
                            case 5:
                                ui_randomColor.Background = GetProgramState(program.Key) ? active : transparent;
                                break;
                        }
                    }
                }                
            });
        }

        /// <summary>
        /// Called by the connection manager when we get a command
        /// </summary>
        /// <param name="command"></param>
        private void ConnectionManager_OnCommandRecieved(Command command)
        {
            if(!m_paneControls.ContainsKey(command.Program))
            {
                return;
            }

            // Send the command
            m_paneControls[command.Program].OnCommand(command);
        }

        #region Connection Status UI

        private void AnimConnectionState_Completed(object sender, object e)
        {
            if(ui_connectionLostHolder.Height == 0)
            {
                ui_connectionLostHolder.Visibility = Visibility.Collapsed;
            }
        }

        private void ConnectionManager_OnClientDisconnected()
        {
            ToggleConnectionState(true);
        }

        private void ConnectionManager_OnClientConnected()
        {
            ToggleConnectionState(false);
        }

        private async void ToggleConnectionState(bool isShowing)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ui_connectionLostHolder.Visibility = Visibility.Visible;
                ui_animConnectionState.To = isShowing ? 25 : 0;
                ui_animConnectionState.From = ui_connectionLostHolder.Height;

                ui_storyConnectionState.Begin();
            });
        }

        #endregion
    }
}
