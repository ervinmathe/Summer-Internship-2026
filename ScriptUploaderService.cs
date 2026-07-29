using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Script_runner
{
    public class ScriptUploaderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public ScriptUploaderService(string apiUrl)
        {
            _httpClient = new HttpClient();
            _apiUrl = apiUrl;
        }

        public async Task<string> UploadScriptAsync(string scriptName, string scriptContent)
        {
            // Build payload matching DB columns for raw content uploads
            var payload = new
            {
                ScriptName = scriptName,
                ScriptContent = scriptContent
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload), 
                Encoding.UTF8, 
                "application/json"
            );

            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Error ({response.StatusCode}): {responseBody}");
            }

            return responseBody;
        }
    }
}