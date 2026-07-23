using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ScriptRunner.Core;


namespace ScriptRunner.Engine
{
    public static class ScriptModule
    {
        private const string DllPath = "GeneratedScripts.dll";
        private const string GeneratedAssemblyName = "GeneratedScripts";
        private static readonly Dictionary<string, IScript> _scripts = new(StringComparer.OrdinalIgnoreCase);

        public static void InitializeScripts(string scriptsFolder)
        {
            var scriptFiles = Directory.GetFiles(scriptsFolder, "*.csx");

            if (scriptFiles.Length == 0)
                return;

            if (IsDllUpToDate(DllPath, scriptFiles))
            {
                byte[] existingBytes = File.ReadAllBytes(DllPath);
                LoadAssembly(existingBytes);
                return;
            }

            var scripts = scriptFiles.ToDictionary(
                filePath => Path.GetFileName(filePath),
                filePath => File.ReadAllText(filePath)
            );

            byte[] compiledBytes = CompileScriptsToDll(scripts);
            File.WriteAllBytes(DllPath, compiledBytes);
            LoadAssembly(compiledBytes);
        }

        public static ScriptResult ExecuteScript(string scriptName, ScriptContext context)
        {
            if (!_scripts.TryGetValue(scriptName, out var script))
            {
                throw new KeyNotFoundException($"Compiled script '{scriptName}' was not found in assembly.");
            }

            return script.Execute(context);
        }

        private static void LoadAssembly(byte[] dllBytes)
        {
            var assembly = Assembly.Load(dllBytes);
            var scriptTypes = assembly.GetTypes()
                .Where(t => typeof(IScript).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in scriptTypes)
            {
                if (Activator.CreateInstance(type) is IScript instance)
                {
                    _scripts[type.Name] = instance;
                }
            }
        }

        private static byte[] CompileScriptsToDll(IDictionary<string, string> scriptFiles)
        {
            var syntaxTrees = new List<SyntaxTree>();

            // Dynamically retrieve exact shared models namespace
            string sharedModelsNamespace = typeof(SharedModels.CityData).Namespace ?? "SharedModels";

            foreach (var (scriptName, scriptCode) in scriptFiles)
            {
                string className = Path.GetFileNameWithoutExtension(scriptName);
                var (scriptUsings, scriptBody) = ExtractUsings(scriptCode);

                string generatedSource = $$"""
                    using System;
                    using System.IO;
                    using System.Text;
                    using System.Linq;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;
                    using System.Net.Http;
                    using System.Text.Json;
                    using System.Runtime.CompilerServices;
                    using System.Windows.Forms;

                    // Core scripting contracts & shared domain models
                    using ScriptRunner.Core;
                    using {{sharedModelsNamespace}};

                    {{scriptUsings}}

                    namespace GeneratedScripts
                    {
                        public class {{className}} : IScript
                        {
                            public ScriptResult Execute(ScriptContext context)
                            {
                                return ExecuteAsync(context).GetAwaiter().GetResult();
                            }

                            private async Task<ScriptResult> ExecuteAsync(ScriptContext context)
                            {
                                {{scriptBody}}
                                return ScriptResult.Success();
                            }
                        }
                    }
                    """;

                syntaxTrees.Add(CSharpSyntaxTree.ParseText(generatedSource));
            }

            var references = GetReferences();

            var compilation = CSharpCompilation.Create(
                GeneratedAssemblyName,
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);
                var errors = string.Join("\n", failures.Select(f => f.GetMessage()));
                throw new InvalidOperationException($"Roslyn Script Compilation Failed:\n{errors}");
            }

            return ms.ToArray();
        }

        private static List<MetadataReference> GetReferences()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Framework / Trusted platform assemblies
            if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trusted)
            {
                foreach (var path in trusted.Split(Path.PathSeparator))
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        paths.Add(path);
                }
            }

            // 2. Currently loaded assemblies in AppDomain
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                {
                    paths.Add(assembly.Location);
                }
            }

            // 3. Explicitly guarantee ScriptRunner.Core and Shared.Models assemblies are included
            paths.Add(typeof(IScript).Assembly.Location);
            paths.Add(typeof(SharedModels.CityData).Assembly.Location);

            // 4. Only exclude a previously-generated scripts dll
            paths.RemoveWhere(p =>
                Path.GetFileNameWithoutExtension(p).Equals(GeneratedAssemblyName, StringComparison.OrdinalIgnoreCase));

            return paths
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                .ToList();
        }

        private static (string Usings, string Body) ExtractUsings(string scriptCode)
        {
            var lines = scriptCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var usings = new List<string>();
            var body = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (IsUsingDirective(trimmed))
                {
                    usings.Add(line);
                }
                else
                {
                    body.Add(line);
                }
            }

            return (string.Join("\n", usings), string.Join("\n", body));
        }

        private static bool IsUsingDirective(string line)
        {
            if (!line.StartsWith("using ") || !line.EndsWith(";"))
                return false;

            // Filter out method-level resource statements like "using var client = ..." or "using (var x = ...)"
            if (line.StartsWith("using var ") || line.Contains("(") || line.Contains(" = new "))
                return false;

            return true;
        }

        private static bool IsDllUpToDate(string dllPath, string[] scriptFiles)
        {
            if (!File.Exists(dllPath))
                return false;

            DateTime dllLastModified = File.GetLastWriteTimeUtc(dllPath);

            foreach (var scriptFile in scriptFiles)
            {
                if (File.GetLastWriteTimeUtc(scriptFile) > dllLastModified)
                {
                    return false;
                }
            }

            return true;
        }
    }
}