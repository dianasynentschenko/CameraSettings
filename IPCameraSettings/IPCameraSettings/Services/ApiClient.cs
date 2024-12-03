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
                BaseAddress = new Uri(baseURL)
            };
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginData = new
            {
                Username = username,
                Password = password
            };

            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
                        
            var request = new HttpRequestMessage(HttpMethod.Post, "...login...")
            {
                Content = content
            };

            
            await LogRequest(request);

            var response = await httpClient.SendAsync(request);

            
            LogResponse(response);



            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

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
            var request = new HttpRequestMessage(HttpMethod.Get, "....");
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


        private async Task LogRequest(HttpRequestMessage request)
        {
            Console.WriteLine("=== Request ===");
            Console.WriteLine($"{request.Method} {request.RequestUri}");

            foreach (var header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();
                Console.WriteLine($"Content: {content}");
            }

            Console.WriteLine("================");
        }

        private void LogResponse(HttpResponseMessage response)
        {
            Console.WriteLine("=== Response ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");

            foreach (var header in response.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (response.Content != null)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Content: {content}");
            }

            Console.WriteLine("================");
        }

    }
}
