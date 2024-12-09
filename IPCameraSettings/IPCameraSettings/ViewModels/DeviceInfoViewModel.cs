using IPCameraSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.ViewModels
{
    public class DeviceInfoViewModel : ViewModelBase
    {
        private ApiClient apiClient;

        private string deviceInfo;
        public string DeviceInfo
        {
            get => deviceInfo;
            set => SetProperty(ref deviceInfo, value);
        }

        public DeviceInfoViewModel(ApiClient apiClient)
        {
            this.apiClient = apiClient;
          
        }

        public async Task InitializeAsync()
        {
            await LoadDeviceInfoAsync();
        }

        public async Task LoadDeviceInfoAsync()
        {
            try
            {
                var deviceInfoModel = await apiClient.GetDeviceInfoAsync();

                if (deviceInfoModel != null)
                {
                    //DeviceInfo = $"Name: {deviceInfoModel.Device.Name}, IP: {deviceInfoModel.Device.Ip}, Status: {deviceInfoModel.Device.Status}";
                }
                else
                {
                    DeviceInfo = "Failed to load device information.";
                }
            }
            catch (Exception ex)
            {
                DeviceInfo = $"Error: {ex.Message}";
            }
        }
    }
}
