using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Forms;
using SharedModels;

// Pull CityData from context
// hello
var City = context.TargetBo as CityData;
if (City == null) return ScriptResult.Cancel("TargetBo is not CityData");

var client = new HttpClient();
var _extras = new ConditionalWeakTable<object, Dictionary<string, object>>();
var extra = _extras.GetOrCreateValue(City);

void print(string message) {
    MessageBox.Show(message);
}

try {
    var encodedName = Uri.EscapeDataString(City.City ?? "");
    var location = await client.GetStringAsync($"https://geocoding-api.open-meteo.com/v1/search?name={encodedName}");
    var doc = JsonDocument.Parse(location);
    var results = doc.RootElement.GetProperty("results");

    if (results.GetArrayLength() > 0)
    {
        var first = results[0];
        
        double lat = first.GetProperty("latitude").GetDouble();
        double lon = first.GetProperty("longitude").GetDouble();
        extra["x"] = lon;
        extra["y"] = lat;
        extra["printFnc"] = new Action<string>(print);

        if (first.TryGetProperty("country", out var countryProp))
        {
            string? correctCountry = countryProp.GetString();
            if (correctCountry != null && !string.Equals(City.Country, correctCountry, StringComparison.OrdinalIgnoreCase))
            {
                City.Country = correctCountry;
            }
        }
            
        if (first.TryGetProperty("admin1", out var admin1Prop))
        {
            string? correctCounty = admin1Prop.GetString();
            if (correctCounty != null && !string.Equals(City.County, correctCounty, StringComparison.OrdinalIgnoreCase))
            {
                City.County = correctCounty;
            }
        }

        return ScriptResult.Success("Location updated successfully");
    } 
    else 
    {
        return ScriptResult.Cancel("Invalid location");
    }
} 
catch(Exception e) {
    return e.Message;
}