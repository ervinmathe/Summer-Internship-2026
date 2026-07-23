using SharedModels;
using System;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;

var client = new HttpClient();

// Get the Business Object passed in the context
var cityData = context.TargetBo as CityData;
if (cityData == null) return ScriptResult.Cancel("TargetBo is not CityData");

var props = typeof(CityData).GetProperties();
var dict = new Dictionary<string, object?>();

foreach (var prop in props)
{
    dict[prop.Name] = prop.GetValue(cityData);
}

var payload = new
{
    typeName = nameof(CityData),
    data = dict
};

var json = JsonSerializer.Serialize(payload);
var response = await client.PostAsync("http://localhost:5153/api/data/store",
    new StringContent(json, Encoding.UTF8, "application/json"));

return response;