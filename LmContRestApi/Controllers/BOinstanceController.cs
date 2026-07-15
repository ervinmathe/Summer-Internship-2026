using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SharedModels;
using LmContRestApi.Models;
using LmContRestApi.Services;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    [HttpPost("store")]
    public IActionResult Store([FromBody] ScriptPayload payload)
    {
        var type = TypeRegistry.Resolve(payload.TypeName);
        if (type is null)
            return BadRequest($"Unknown type: {payload.TypeName}");

        object? instance;
        try
        {
            instance = JsonSerializer.Deserialize(payload.Data.GetRawText(), type);
        }
        catch (JsonException ex)
        {
            return BadRequest($"Deserialization failed: {ex.Message}");
        }

        if (instance is null)
            return BadRequest("Deserialized instance was null.");

        // your modification step
        if (instance is CityData city)
        {
            city.City = city.City?.Trim();
        }

        DataStore.Set(payload.TypeName, instance);
        return Ok();
    }

    [HttpGet("getData/{typeName}")]
    public IActionResult GetData(string typeName)
    {
        var instance = DataStore.Get(typeName);
        return instance is not null ? Ok(instance) : NotFound($"No stored instance for type '{typeName}'.");
    }


    [HttpDelete("clearStore")]

    public IActionResult clearStore()
    {
        DataStore.Clear() ;
        return Ok() ;
    }
}