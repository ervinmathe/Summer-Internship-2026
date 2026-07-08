var city = new CityData();

city.Country = valueArray[0];
city.County = valueArray[1];
city.City = valueArray[2];

var endpoint = valueArray[3].toString() ;
var fetchresponse;

try {
    fetchresponse = api.PostCity(city.Country, city.County, city.City , endpoint);
} catch(e) {
    fetchresponse = e.toString();
}