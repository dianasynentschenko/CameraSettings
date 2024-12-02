using IPCameraSettings.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.Services
{
    public class ApiClient
    {
        private readonly HttpClient httpClient;
        private string csrfToken;

        public ApiClient(string baseURL, string username, string password)
        {

           
            baseURL = baseURL.Trim();

           
            if (!baseURL.StartsWith("http://"))
            {
                baseURL = "http://" + baseURL;
            }

           
            if (!Uri.TryCreate(baseURL, UriKind.Absolute, out Uri uri))
            {
                throw new FormatException($"Invalid baseURL format: {baseURL}");
            }

            var handler = new HttpClientHandler
            {
                Credentials = new System.Net.NetworkCredential(username, password),
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true


            };

            httpClient = new HttpClient(handler)
            {
                BaseAddress = uri
            };
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginData = new
            {
                username = username,
                password = password
            };

            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("...login...", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed. Status: {response.StatusCode}, Response: {errorContent}");
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                if (response.Headers.TryGetValues("X-CSRFToken", out var values))
                {
                    csrfToken = values.FirstOrDefault();
                }
                else
                {
                    Console.WriteLine("Warning: CSRF token not found in response headers.");
                }

                return true;
            }

            return false;
        }

        public async Task<StreamSettings> GetStreamSettingsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "...Main stream...");
            AddCsrfToken(request);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StreamSettings>(json);
        }

        private void AddCsrfToken(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(csrfToken))
            {
                request.Headers.Add("X-CSRFToken", csrfToken);
            }

        }
    }
}
