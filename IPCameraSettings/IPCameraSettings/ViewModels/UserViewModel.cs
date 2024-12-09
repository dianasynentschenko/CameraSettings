using IPCameraSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IPCameraSettings.ViewModels
{
    public class UserViewModel : ViewModelBase
    {   
        private string ipAddress;
        public string IPAddress
        {
            get => ipAddress;
            set => SetProperty(ref ipAddress, value);
        }

        private string username;
        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        private string password;
        public string Password
        { 
            get => password;
            set => SetProperty(ref password, value);
        }

        private ApiClient apiClient;

        public ICommand LoginCommand { get; }

        public UserViewModel()
        {            
            LoginCommand = new RelayCommand(async _ => await LoginAsync());    
            
        }

        public async Task<bool> LoginAsync()
        {
            if (IsBusy)
                return false;

            IsBusy = true;
            ErrorMessage = null;

            try
            {                
                apiClient = new ApiClient(IPAddress, Username, Password);
                
                bool isLoggedIn = await apiClient.LoginAsync(Username, Password);

                if (isLoggedIn)
                {   
                    MessageBox.Show("Login successful!");

                    await InitializeSettingsWindow();                  

                    return true;
                }
                else
                {
                    MessageBox.Show("Login failed. Please check your username, password, and IP address.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show($"Error: {ErrorMessage}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task InitializeSettingsWindow()
        {            
            var settingsViewModel = new SettingsViewModel(apiClient);
                        
            await settingsViewModel.InitializeAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var settingsWindow = new SettingsWindow(settingsViewModel);
                settingsWindow.Show();
                Application.Current.MainWindow.Close();
            });
        }


    }
}
