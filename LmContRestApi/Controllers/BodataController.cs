using LmContRestApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class bodataController : ControllerBase
{
    [HttpPost]
    public IActionResult bodata([FromBody] JsonElement body)
    {
        var boname = body.GetProperty("boname").GetString();
        
        var instance = DataStore.Get(boname!) ;

        return instance is not null ? Ok(instance) : NotFound($"No intance found {boname}") ;
    }
}
