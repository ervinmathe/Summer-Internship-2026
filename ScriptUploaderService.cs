using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Script_runner
{
    public class ScriptUploaderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public ScriptUploaderService(string apiUrl)
        {
            _httpClient = new HttpClient();
            _apiUrl = apiUrl;
        }

        public async Task<string> UploadScriptAsync(string scriptName, string scriptPath, Type globalsType)
        {
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Script not found at {scriptPath}");

            string code = File.ReadAllText(scriptPath);

            // 1. Setup the exact same options you use for execution
            var options = ScriptOptions.Default
                .WithReferences(
                    typeof(Form1).Assembly, 
                    typeof(Control).Assembly,
                    typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly // <-- ADD THIS LINE
                )
                .WithImports("System", "System.Windows.Forms", "Script_runner");

            // 2. Create the script and force compilation
            var script = CSharpScript.Create(code, options, globalsType: globalsType);
            var compilation = script.GetCompilation();

            // 3. Emit to memory stream
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                var errors = string.Join(Environment.NewLine, emitResult.Diagnostics.Select(d => d.GetMessage()));
                throw new InvalidOperationException($"Compilation failed:{Environment.NewLine}{errors}");
            }

            if (emitResult.Success)
            {
                long sizeInBytes = ms.Length;
                System.Diagnostics.Debug.WriteLine($"{sizeInBytes / 1024.0:F2} KB") ;
            }
            else
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
            }
            // 4. Convert bytecode to Base64 string for JSON transmission
            byte[] compiledBytes = ms.ToArray();
            string base64Assembly = Convert.ToBase64String(compiledBytes);

            // 5. Build payload matching the DB columns exactly (ignoring the Id column)
            var payload = new
            {
                ScriptName = scriptName,
                CompiledAssembly = base64Assembly,
                ScriptContent = code ,
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // 6. Send to your dynamic API
            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Error ({response.StatusCode}): {responseBody}");
            }

            return responseBody;
        }
    }
}