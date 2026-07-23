using System.Net.Http;
using System.Text;
using System.Text.Json;

var client = new HttpClient();

// Get boName from context property or object name
string boName = context.TargetBo.GetType().Name;

var payload = new { boname = boName };
string json = JsonSerializer.Serialize(payload);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("http://localhost:5153/api/bodata", content);
string responseData = await response.Content.ReadAsStringAsync();

return responseData;