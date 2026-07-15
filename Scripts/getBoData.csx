using System.Net.Http;
using System.Text;
using System.Text.Json;

var client = new HttpClient();

var payload = new { boname = boName };
string json = JsonSerializer.Serialize(payload);

var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = client.PostAsync($"http://localhost:5153/api/bodata" , content).Result ;

return response.Content.ReadAsStringAsync().Result;