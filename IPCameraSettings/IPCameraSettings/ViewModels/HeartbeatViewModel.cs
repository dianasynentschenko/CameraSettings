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

        private HeartbeatState heartbeatState;
        public HeartbeatState HeartbeatState
        {
            get => heartbeatState;
            set => SetProperty(ref heartbeatState, value);
        }

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

            timer.Tick += async (s, e) => await PerformHeartbeatAsync();
            HeartbeatState = HeartbeatState.Idle;           
        }


        public async Task StartAsync()
        {
            if (heartbeatState == HeartbeatState.Idle)
            { 
                timer.Start();
                await PerformHeartbeatAsync();
            }
        }

        private async Task PerformHeartbeatAsync()
        {
            if (heartbeatState == HeartbeatState.Checking)
                return;

            try
            {
                HeartbeatState = HeartbeatState.Checking;
                HeartbeatStatus = "Checking...";
                bool isAlive = await apiClient.SendHeartbeatAsync();
                if (isAlive)
                {
                    HeartbeatState = HeartbeatState.Alive;
                    HeartbeatStatus = "Server is alive";
                }
                else
                {
                    HeartbeatState = HeartbeatState.Error;
                    HeartbeatStatus = "Server is down";
                }
            }
            catch (Exception ex)
            {
                HeartbeatState = HeartbeatState.Error;
                HeartbeatStatus = $"Error: {ex.Message}";
            }
        }

        public void StopTimer()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                heartbeatState = HeartbeatState.Idle;
            }
        }
    }
}

