using System;

namespace ScriptRunner.Core
{
    public enum ScriptEventType
    {
        Before,
        On,
        After
    }

    public class ScriptContext
    {
        public required object TargetBo { get; init; }
        public required string? PropertyName { get; init; }
        public object? OldValue { get; init; }
        public object? NewValue { get; init; }
        public ScriptEventType EventType { get; init; }

        public Action<string>? UpdateStatus { get; init; }
    }

    public class ScriptResult
    {
        public bool IsCancelled { get; set; }
        public object? ReturnValue { get; set; }
        public Exception? Exception { get; set; }

        public static ScriptResult Success(object? value = null) => new() { ReturnValue = value };
        public static ScriptResult Cancel(string? reason = null) => new() { IsCancelled = true, ReturnValue = reason };

        // Implicit conversions
        public static implicit operator ScriptResult(string? value) => Success(value);
        public static implicit operator ScriptResult(bool isSuccess) => isSuccess ? Success() : Cancel("Operation cancelled.");
        
        // Allows returning HttpResponseMessage directly from scripts
        public static implicit operator ScriptResult(System.Net.Http.HttpResponseMessage response) 
            => response.IsSuccessStatusCode ? Success(response) : Cancel($"HTTP Error {(int)response.StatusCode}");
    }

    public interface IScript
    {
        ScriptResult Execute(ScriptContext context);
    }
}
