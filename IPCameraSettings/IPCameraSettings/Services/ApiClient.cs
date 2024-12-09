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
using System.Windows;


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
            HttpClientHandler handler = new HttpClientHandler           
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
            HttpRequestMessage initialRequest = new HttpRequestMessage(HttpMethod.Post, url);            
            Console.WriteLine($"Request URI: {initialRequest.RequestUri}");

            HttpResponseMessage response = await httpClient.SendAsync(initialRequest);

            if (!response.Headers.Contains("WWW-Authenticate"))
            {
                throw new Exception("Digest authentication not supported by the server.");
            }

            string authenticateHeader = response.Headers.GetValues("WWW-Authenticate").FirstOrDefault();

            // params from WWW-Authenticate
            string realm = ExtractParameter(authenticateHeader, "realm");
            string nonce = ExtractParameter(authenticateHeader, "nonce");
            string qop = ExtractParameter(authenticateHeader, "qop");

            // HA1, HA2, response
            string ha1 = MD5Hash($"{username}:{realm}:{password}");
            string ha2 = MD5Hash($"POST:{url}");
            Console.WriteLine($"HA1: {ha1}");
            Console.WriteLine($"HA2: {ha2}");
           
            string nc = "00000001"; // Nonce Count
            string cnonce = Guid.NewGuid().ToString("N"); // Client Nonce

            string responseHash = MD5Hash($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}");
            Console.WriteLine($"Response: {responseHash}");

            // Headers Authorization
            return $"Digest username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{url}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\", response=\"{responseHash}\"";
        }


        private string MD5Hash(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


        private string ExtractParameter(string header, string paramName)
        {
            string param = $"{paramName}=\"";
            int startIndex = header.IndexOf(param) + param.Length;
            int endIndex = header.IndexOf("\"", startIndex);
            return header.Substring(startIndex, endIndex - startIndex);
        }


        public async Task<bool> LoginAsync(string username, string password)
        {
            string loginUrl = "...Login...";

            // Headers Digest Authentication
            string digestHeader = await GenerateDigestHeader(loginUrl, username, password);

            // Query with headers Authorization
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, loginUrl);
            request.Headers.Add("Authorization", digestHeader);                       
            await LogRequest(request);

            HttpResponseMessage response = await httpClient.SendAsync(request);           
            LogResponse(response);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed. Status: {response.StatusCode}, Response: {errorContent}");
                return false;
            }

            if (response.Headers.TryGetValues("X-CSRFToken", out IEnumerable<string> values))
            {
                csrfToken = values.FirstOrDefault();
            }
            else
            {
                Console.WriteLine("Warning: CSRF token not found in response headers.");
            }

            CookieCollection cookies = cookieContainer.GetCookies(new Uri(httpClient.BaseAddress + loginUrl));
            foreach (Cookie cookie in cookies)
            {
                Console.WriteLine($"Cookie: {cookie.Name} = {cookie.Value}");
            }

            return true;
        }

        public async Task<bool> SendHeartbeatAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "...Heartbeat...");
                await LogRequest(request);

                AddCsrfToken(request);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Heartbeat successful.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Heartbeat failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending heartbeat: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> GetDeviceInfoAsync()
        {
            try
            {                
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "...DeviceInfo Get...");
                await LogRequest(request);

                AddCsrfToken(request);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Device info successful.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Device info failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending device info: {ex.Message}");
                return false;
            }
        }

        public async Task<StreamSettings> GetStreamSettingsAsync()
        {            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "...MainStream Get...");         
            AddCsrfToken(request);
             
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            // return JsonConvert.DeserializeObject<StreamSettings>(json);

            try
            {              
                var payload = JsonConvert.DeserializeObject<RequestPayload>(json);
                           
                if (payload?.Data?.ChannelInfo?.CH1 != null)
                {
                    return payload.Data.ChannelInfo.CH1;
                }
                else
                {
                    throw new Exception("Channel data (CH1) not found in JSON response.");
                }
            }
            catch (JsonSerializationException ex)
            {              
                throw new Exception("Error deserializing StreamSettings: " + ex.Message);
            }
            catch (Exception ex)
            {             
                throw new Exception("Error fetching StreamSettings: " + ex.Message);
            }
        }


        public async Task<bool> UpdateStreamSettingsAsync(StreamSettings settings)
        {
            if (settings == null)
            {
                Console.WriteLine("Stream settings are missing.");
                return false;
            }

            string url = "...MainStream Set...";
                        
            var payload = new RequestPayload
            {
                Data = new DataWrapper
                {
                    ChannelInfo = new ChannelInfo
                    {
                        CH1 = settings
                    }
                }
            };
           
            string json = JsonConvert.SerializeObject(payload, Formatting.Indented);

            
            int insertIndex = json.IndexOf('{') + 1;
            json = json.Insert(insertIndex, "\n    "); 

            Console.WriteLine($"Serialized JSON:\n{json}");

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                     
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            await LogRequest(request);

            AddCsrfToken(request); 

            CookieCollection cookies = cookieContainer.GetCookies(httpClient.BaseAddress);
            foreach (Cookie cookie in cookies)
            {
                Console.WriteLine($"Cookie: {cookie.Name} = {cookie.Value}");
            }

            string sessionCookie = cookies["session"]?.Value;
            if (!string.IsNullOrEmpty(sessionCookie))
            {
                request.Headers.Add("Cookie", $"session={sessionCookie}");
            }
            else
            {
                Console.WriteLine("Warning: Session cookie is missing.");
            }

            Console.WriteLine("Request headers:");
            foreach (var header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            try
            {
                Console.WriteLine($"Sending request to {url}");
             
                HttpResponseMessage response = await httpClient.SendAsync(request);

                Console.WriteLine($"Response received. Status Code: {response.StatusCode}");
                            
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content:\n{responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Stream settings updated successfully!");
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to update stream settings.");
                                       
                    try
                    {
                        JObject errorDetails = JObject.Parse(responseContent);
                        Console.WriteLine($"Error Code: {errorDetails["error_code"]}");
                        Console.WriteLine($"Error Message: {errorDetails["message"] ?? "No additional error message provided"}");
                        if (errorDetails["invalid_params"] != null)
                        {
                            Console.WriteLine($"Invalid Parameters: {errorDetails["invalid_params"]}");
                        }
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Error response is not a valid JSON.");
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating stream settings: {ex.Message}");
                return false;
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

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (request.Content != null)
            {
                string content = await request.Content.ReadAsStringAsync();
                Console.WriteLine($"Content: {content}");
            }

            Console.WriteLine("================");
        }

        private void LogResponse(HttpResponseMessage response)
        {
            Console.WriteLine("=== Response ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");

            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (response.Content != null)
            {
                string content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Content: {content}");
            }

            Console.WriteLine("================");
        }

    }
}
