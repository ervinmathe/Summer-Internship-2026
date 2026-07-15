using SharedModels;
using System;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;

var client  = new HttpClient();

var props = typeof(CityData).GetProperties();
var dict = new Dictionary<string, object?>();

for (int i = 0; i < data.Length && i < props.Length; i++)
{
    dict[props[i].Name] = data[i];
}

var payload = new
{
    typeName = nameof(CityData),
    data = dict
};

var json = JsonSerializer.Serialize(payload);
var response = await client.PostAsync("http://localhost:5153/api/data/store",
    new StringContent(json, Encoding.UTF8, "application/json"));

return response ;