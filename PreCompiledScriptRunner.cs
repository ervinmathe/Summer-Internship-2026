using System.IO;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.Loader;

namespace Script_runner
{
    public static class PreCompiledScriptRunner
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<object> RunFromApiAsync(string scriptName, object globals)
{
    // 1. Fetch the payload from the API (Your network code stays the same)
    var responseMessage = await _client.GetAsync($"http://localhost:5153/api/update/{scriptName}");
    responseMessage.EnsureSuccessStatusCode();
    
    var response = await responseMessage.Content.ReadFromJsonAsync<ScriptResponse>();
    byte[] assemblyBytes = Convert.FromBase64String(response.CompiledAssembly);

    // 2. Load the downloaded bytecode into memory
    var assembly = System.Reflection.Assembly.Load(assemblyBytes);

    // 3. Locate the auto-generated Roslyn script class
    // Roslyn names the generated class something like "Submission#0"
    var type = assembly.GetTypes().FirstOrDefault(t => t.Name.StartsWith("Submission#"));
    if (type == null) throw new Exception("Could not locate script entry class.");

    // 4. Locate the static entry point
    var factoryMethod = type.GetMethod("<Factory>");
    if (factoryMethod == null) throw new Exception("Could not locate the <Factory> entry point.");

    // 5. Prepare the arguments
    // Roslyn expects an object array where the FIRST element is your Globals object!
    var submissionState = new object[2];
    submissionState[0] = globals;

    // 6. Invoke the script! 
    // We pass 'null' as the first argument because <Factory> is a static method.
    var resultTask = factoryMethod.Invoke(null, new object[] { submissionState });

    // 7. Unwrap the Task result safely
    if (resultTask is Task<object> taskWithResult)
    {
        return await taskWithResult;
    }
    else if (resultTask is Task voidTask)
    {
        await voidTask;
        return "Script executed successfully (No return value).";
    }

    return resultTask;

        }

        // DTO matching the API JSON payload structure
        private class ScriptResponse
        {
            public string ScriptName { get; set; } = string.Empty;
            public string CompiledAssembly { get; set; } = string.Empty;
        }
    }
}