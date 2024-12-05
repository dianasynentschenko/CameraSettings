using IPCameraSettings.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IPCameraSettings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddressBox.Text;
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            string baseURL = $"http://{ipAddress}";
            ApiClient apiClient = new ApiClient(ipAddress, username, password);

            bool isLoggedIn = await apiClient.LoginAsync(username, password);
            if (isLoggedIn)
            {
                SettingsWindow settingsWindow = new SettingsWindow(apiClient);
                settingsWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Login failed. Please check your username, password, and IP address.");
            }
        }


    }
    
}
