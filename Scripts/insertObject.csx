using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
var client = new HttpClient();


static ConditionalWeakTable<object, Dictionary<string, object>> _extras = new();

var extra = _extras.GetOrCreateValue(City);

try {
    var encodedName = Uri.EscapeDataString(City.City);
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


        if (first.TryGetProperty("country", out var countryProp))
        {
            string correctCountry = countryProp.GetString();
            if (!string.Equals(City.Country, correctCountry, StringComparison.OrdinalIgnoreCase))
            {
                City.Country = correctCountry;
            }
        }
            
        if (first.TryGetProperty("admin1", out var admin1Prop))
        {
            string correctCounty = admin1Prop.GetString();
            if (!string.Equals(City.County, correctCounty, StringComparison.OrdinalIgnoreCase))
            {
                City.County = correctCounty;
            }
        }
        var response = api.PostCity(City ,  "insertcity" , _extras);

        return response;
    } else {
        return "Invalid location" ;
    }

    

   

} catch(Exception e) {
    return e.Message;
}


