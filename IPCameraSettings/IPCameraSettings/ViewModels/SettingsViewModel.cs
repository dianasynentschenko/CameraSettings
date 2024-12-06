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
        private readonly ApiClient apiClient;

        private StreamSettings streamSettings;

        public StreamSettings StreamSettings
        {
            get => streamSettings;
            set => SetProperty(ref streamSettings, value);
        }

        public ICommand LoadSettingsCommand { get; }

        public SettingsViewModel(ApiClient apiClient)
        { 
            this.apiClient = apiClient;

            _ = LoadSettingsAsync();

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
