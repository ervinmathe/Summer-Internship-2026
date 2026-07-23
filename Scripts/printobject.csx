//City.Country = "Hu" ;

using System.Threading.Tasks;
using SharedModels;
using Script_runner;

async Task<CityData> modify(CityData City) {
    try {
        Form.resultLabel.Text = "Elotte";

        await Task.Delay(2000);
        Form.resultLabel.Text = "Utana";

    } catch(Exception ex) {
        Form.resultLabel.Text = ex.Message;
        return City;
    }

    await Task.Delay(2000);
    City.County = "Budapest";
    City.City = "Budapest";
    return City;
}

var result = await modify(City);
Form.resultLabel.Text = $"Country: {result.Country} | County: {result.County} | City: {result.City}";

// Run the other precompiled script from the DB
var otherGlobals = new getBoData
{
    boName = "CityData"
};

object extraResult = await PreCompiledScriptRunner.RunFromApiAsync("getBoData", otherGlobals);

return $"Country: {result.Country}\nCounty: {result.County}\nCity: {result.City}\n---\n{extraResult}";