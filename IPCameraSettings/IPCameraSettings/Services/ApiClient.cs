using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.Services
{
    internal class ApiClient
    {
        private readonly HttpClient httpClient;

        public ApiClient(string baseURL, string username, string password)
        {
            var handler = new HttpClientHandler
            {
                Credentials = new System.Net.NetworkCredential(username, password)
            };
            httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseURL)
            };
        }
    
    }
}
