using System;
using System.Collections.Generic;
using System.Net.Http ;
using System.Runtime.CompilerServices;
using System.Text;

namespace Script_runner {
    public class ApiClient {
        private static readonly HttpClient client = new HttpClient();

        //private static ConditionalWeakTable<object, Dictionary<string, object>> _extras = new();

       public string PostCity(CityData City, string endpoint , ConditionalWeakTable<object, Dictionary<string, object>> _extras = null)
       {       
            var node = System.Text.Json.Nodes.JsonNode.Parse(
                System.Text.Json.JsonSerializer.Serialize(City)
            )       !.AsObject();

            if(_extras.TryGetValue(City, out var extras))
            {
                foreach (var (key, value) in extras)
                    node[key] = System.Text.Json.Nodes.JsonValue.Create(value);
            }

            string json = node.ToJsonString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync($"http://localhost:3000/{endpoint}", content).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
}
}
