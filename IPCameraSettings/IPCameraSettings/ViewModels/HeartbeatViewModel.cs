using IPCameraSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IPCameraSettings.ViewModels
{
    public class HeartbeatViewModel : ViewModelBase
    {
        private ApiClient apiClient;
        private readonly DispatcherTimer timer;

        private string heartbeatStatus;
        public string HeartbeatStatus
        {
            get => heartbeatStatus;
            set => SetProperty(ref heartbeatStatus, value);
        }

        public HeartbeatViewModel(ApiClient apiClient)
        {
            this.apiClient = apiClient;
                        
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };

            timer.Tick += async (s, e) => await SendHeartbeatAsync();
            timer.Start();
        }

        private async Task SendHeartbeatAsync()
        {
            try
            {
                bool isAlive = await apiClient.SendHeartbeatAsync();
                HeartbeatStatus = isAlive ? "Server is alive" : "Server is down";
            }
            catch (Exception ex)
            {
                HeartbeatStatus = $"Error: {ex.Message}";
            }
        }

        public void StopTimer()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
        }
    }
}

