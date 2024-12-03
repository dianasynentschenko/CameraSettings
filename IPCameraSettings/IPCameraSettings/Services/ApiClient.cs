using IPCameraSettings.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.Services
{
    public class ApiClient
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;
        private string csrfToken;

        public ApiClient(string baseURL, string username, string password)
        {                       
            baseURL = baseURL.Trim();           

            // use https, when http
            if (!baseURL.StartsWith("https://"))
            {
                baseURL = "https://" + baseURL;
            }

            if (!Uri.TryCreate(baseURL, UriKind.Absolute, out Uri uri))
            {
                throw new FormatException($"Invalid baseURL format: {baseURL}");
            }


            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false, // turn off redirect
                Credentials = new System.Net.NetworkCredential(username, password),
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,

                UseCookies = true,
                CookieContainer = cookieContainer

            };

            httpClient = new HttpClient(handler)
            {
                BaseAddress = uri
            };
            Console.WriteLine($"BaseAddress set to: {httpClient.BaseAddress}");
        }

        private async Task<string> GenerateDigestHeader(string url, string username, string password)
        {
            // first requestti receive WWW-Authenticate
            var initialRequest = new HttpRequestMessage(HttpMethod.Post, url);            
            Console.WriteLine($"Request URI: {initialRequest.RequestUri}");

            var response = await httpClient.SendAsync(initialRequest);

            if (!response.Headers.Contains("WWW-Authenticate"))
            {
                throw new Exception("Digest authentication not supported by the server.");
            }

            var authenticateHeader = response.Headers.GetValues("WWW-Authenticate").FirstOrDefault();

            // params from WWW-Authenticate
            var realm = ExtractParameter(authenticateHeader, "realm");
            var nonce = ExtractParameter(authenticateHeader, "nonce");
            var qop = ExtractParameter(authenticateHeader, "qop");

            // HA1, HA2, response
            var ha1 = MD5Hash($"{username}:{realm}:{password}");
            var ha2 = MD5Hash($"POST:{url}");
            Console.WriteLine($"HA1: {ha1}");
            Console.WriteLine($"HA2: {ha2}");
           
            var nc = "00000001"; // Nonce Count
            var cnonce = Guid.NewGuid().ToString("N"); // Client Nonce

            var responseHash = MD5Hash($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}");
            Console.WriteLine($"Response: {responseHash}");

            // Headers Authorization
            return $"Digest username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{url}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\", response=\"{responseHash}\"";
        }

        private string MD5Hash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private string ExtractParameter(string header, string paramName)
        {
            var param = $"{paramName}=\"";
            var startIndex = header.IndexOf(param) + param.Length;
            var endIndex = header.IndexOf("\"", startIndex);
            return header.Substring(startIndex, endIndex - startIndex);
        }


        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginUrl = "...login...";

            // Headers Digest Authentication
            var digestHeader = await GenerateDigestHeader(loginUrl, username, password);

            // Query with headers Authorization
            var request = new HttpRequestMessage(HttpMethod.Post, loginUrl);
            request.Headers.Add("Authorization", digestHeader);                       
            await LogRequest(request);

            var response = await httpClient.SendAsync(request);           
            LogResponse(response);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed. Status: {response.StatusCode}, Response: {errorContent}");
                return false;
            }

            if (response.Headers.TryGetValues("X-CSRFToken", out var values))
            {
                csrfToken = values.FirstOrDefault();
            }
            else
            {
                Console.WriteLine("Warning: CSRF token not found in response headers.");
            }

            var cookies = cookieContainer.GetCookies(new Uri(httpClient.BaseAddress + loginUrl));
            foreach (Cookie cookie in cookies)
            {
                Console.WriteLine($"Cookie: {cookie.Name} = {cookie.Value}");
            }

            return true;
        }

               

        public async Task<StreamSettings> GetStreamSettingsAsync()
        {            
            var request = new HttpRequestMessage(HttpMethod.Post, "...main stream...");

            AddCsrfToken(request);
             
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            // return JsonConvert.DeserializeObject<StreamSettings>(json);

            try
            {
                var jObject = JObject.Parse(json);
                                
                var ch1Data = jObject["data"]?["channel_info"]?["CH1"];
                if (ch1Data != null)
                {
                    return ch1Data.ToObject<StreamSettings>();
                }
                else
                {
                    throw new Exception("Channel data not found in JSON response.");
                }
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing StreamSettings: " + ex.Message);
            }
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
