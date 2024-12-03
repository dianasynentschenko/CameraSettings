using IPCameraSettings.Models;
using IPCameraSettings.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IPCameraSettings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ApiClient apiClient;
        private StreamSettings streamSettings;        

        public SettingsWindow(ApiClient apiClient)
        {
            InitializeComponent();
            
            this.apiClient = apiClient;

            LoadSettings();
            
        }

        private async void LoadSettings()
        {
            try
            {
                streamSettings = await apiClient.GetStreamSettingsAsync();

                    SettingsBox.Text = $"Resolution: {streamSettings.Resolution}\n"+
                           $"FPS: {streamSettings.FPS}\n" +
                           $"Bitrate: {streamSettings.Bitrate}\n" +
                           $"Bitrate mode: {streamSettings.BitrateMode}\n"+
                           $"Custom bitrate: {streamSettings.CustomBitrate}\n" +
                           $"Video encode type: {streamSettings.VideoEncodeType}\n" +
                           $"Video encode level: {streamSettings.VideoEncodeLevel}\n" +
                           $"Bitrate control: {streamSettings.BitrateControl}\n" +
                           $"Video quality: {streamSettings.VideoQuality}\n" +
                           $"Audio: {streamSettings.Audio}\n" +
                           $"I frame interval: {streamSettings.IFrameInterval}\n";
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Error loading settings: {ex.Message}");

            }
        }

    }
}
