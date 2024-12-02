using IPCameraSettings.Models;
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

        public SettingsWindow(string ipAddress, string username, string password)
        {
            InitializeComponent();

            var baseURL = $"http://{ipAddress}/API";
            apiClient = new ApiClient(baseURL, username, password);
            
            
        }
    }
}
