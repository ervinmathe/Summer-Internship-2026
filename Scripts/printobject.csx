using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedModels;
using ScriptRunner.Engine;

var City = context.TargetBo as CityData;
if(City == null) return ScriptResult.Cancel("TargetBo is not CityData");

async Task<CityData> modify(CityData city) {
    try {
        // If TargetBo is or contains Form controls:

        context.UpdateStatus?.Invoke("Elotte");
        await Task.Delay(2000);
        context.UpdateStatus?.Invoke("Utana");

    } catch {
        return city;
    }

    await Task.Delay(2000);
    city.County = "Budapest";
    city.City = "Budapest";
    return city;
}

var result = await modify(City);

// Execute 'getBoData.csx' directly from our loaded DLL!var conetxtForExtraScript = new ScriptContext
var context2 = new ScriptContext {
    TargetBo = result ,
    PropertyName = "" ,
    OldValue = null ,
    NewValue = null ,
    EventType = ScriptEventType.After ,
    UpdateStatus = text => context.UpdateStatus?.Invoke(text)
};

ScriptResult result2 = await Task.Run(() => ScriptModule.ExecuteScript("getBoData" , context2));

return $"Update Country: {result.Country}\nCounty: {result.County}\nCity: {result.City}\n---\n{result2.ReturnValue}";
