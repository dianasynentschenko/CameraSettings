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
        private SettingsState settingsState;
        public SettingsState SettingsState
        {
            get => settingsState;
            set => SetProperty(ref settingsState, value);
        }

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
            SettingsState = SettingsState.LoadingSettings;

            try
            {   
                await Heartbeat.StartAsync();
                await Task.WhenAll(                   
                    DeviceInfo.InitializeAsync(),
                    ChannelInfo.InitializeAsync()
                );
                SettingsState = SettingsState.Loaded;
            }
            catch (Exception ex)
            {
                SettingsState = SettingsState.Error;
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task SaveSettingsAsync()
        {
            if (IsBusy || SettingsState == SettingsState.SavingSettings)
                return;

            IsBusy = true;
            SettingsState = SettingsState.SavingSettings;

            try
            {
                bool success = await apiClient.UpdateStreamSettingsAsync(StreamSettings);
                if (success)
                {
                    SettingsState = SettingsState.LoadingSettings;
                    await Task.Delay(500);
                    Console.WriteLine($"State changed: {SettingsState}");
                    await LoadSettingsAsync();
                    SettingsState = SettingsState.Loaded;                 

                }
                else
                {
                    SettingsState = SettingsState.Error;
                    MessageBox.Show("Failed to save settings.");
                }
            }
            catch (Exception ex)
            {
                SettingsState = SettingsState.Error;
                MessageBox.Show($"Failed to save settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadSettingsAsync()
        {
            if (IsBusy || SettingsState == SettingsState.LoadingSettings)
                return;

            IsBusy = true;
            SettingsState = SettingsState.LoadingSettings;

            try
            {               
                StreamSettings = await apiClient.GetStreamSettingsAsync();
                SettingsState = SettingsState.Loaded;             
            }
            catch (Exception ex)
            {
                SettingsState = SettingsState.Error;
                MessageBox.Show($"Failed to load settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
