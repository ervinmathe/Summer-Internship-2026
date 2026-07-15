using System.Text.Json;

namespace LmContRestApi.Models;

public class ScriptPayload
{
    public string TypeName { get; set; } = default!;
    public JsonElement Data { get; set; }
}