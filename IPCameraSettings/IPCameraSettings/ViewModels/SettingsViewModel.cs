using IPCameraSettings.Models;
using IPCameraSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IPCameraSettings.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool isInitialized = false;
        private readonly ApiClient apiClient;

        private StreamSettings streamSettings;

        public HeartbeatViewModel Heartbeat { get; }

        public DeviceInfoViewModel DeviceInfo { get; }

        public ChannelInfoViewModel ChannelInfo { get; }

        public StreamSettings StreamSettings
        {
            get => streamSettings;
            set => SetProperty(ref streamSettings, value);
        }

        public ICommand LoadSettingsCommand { get; }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel(ApiClient apiClient)
        { 
            this.apiClient = apiClient;
            Heartbeat = new HeartbeatViewModel(apiClient);
            DeviceInfo = new DeviceInfoViewModel(apiClient);
            ChannelInfo = new ChannelInfoViewModel(apiClient);
            SaveSettingsCommand = new RelayCommand(async (param) => await SaveSettingsAsync());
            
            _ = LoadSettingsAsync();        

        }

        public async Task InitializeAsync()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            try
            {   
                await Heartbeat.StartAsync();
                await Task.WhenAll(                   
                    DeviceInfo.InitializeAsync(),
                    ChannelInfo.InitializeAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task SaveSettingsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                bool success = await apiClient.UpdateStreamSettingsAsync(StreamSettings);
                if (success)
                {
                    MessageBox.Show("Settings saved successfully.");
                }
                else
                {
                    MessageBox.Show("Failed to save settings.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadSettingsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                StreamSettings = await apiClient.GetStreamSettingsAsync();
                MessageBox.Show("Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
