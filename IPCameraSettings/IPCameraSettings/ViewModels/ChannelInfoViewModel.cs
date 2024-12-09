using IPCameraSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.ViewModels
{
    public class ChannelInfoViewModel : ViewModelBase
    {
        private ApiClient apiClient;

        private string channelInfo;
        public string ChannelInfo
        {
            get => channelInfo;
            set => SetProperty(ref channelInfo, value);
        }

        public ChannelInfoViewModel(ApiClient apiClient)
        {
            this.apiClient = apiClient;
          
        }

        public async Task InitializeAsync()
        {
            await LoadChannelInfoAsync();
        }

        public async Task LoadChannelInfoAsync()
        {
            try
            {
                var channelInfoModel = await apiClient.GetChannelInfoAsync();

                if (channelInfoModel != null)
                {
                }
                else
                {
                    ChannelInfo = "Failed to load channel information.";
                }
            }
            catch (Exception ex)
            {
                ChannelInfo = $"Error: {ex.Message}";
            }
        }
    }
}

