using Microsoft.ClearScript.V8;

namespace FileServer.Tests.Util;

internal static class JavaScriptEngineExtensions
{
    public static TRes? Eval<TRes>(this V8ScriptEngine engine, string code) where TRes : struct
    {
        object res = engine.Evaluate(code);
        if (res is null)
            return null;
        if (res is TRes typedRes)
            return typedRes;
        throw new EvalException($"Wrong eval result type: expected '{typeof(TRes)}', but got '{res.GetType().FullName}'.");
    }

    public static void Eval(this V8ScriptEngine engine, string code)
    {
        engine.Evaluate(code);
    }

    public static void EvalFiles(this V8ScriptEngine engine, string[] files)
    {
        foreach (string filePath in files)
            engine.Evaluate(File.ReadAllText(filePath));
    }
}

#pragma warning disable MA0048 // File name must match type name
internal sealed class EvalException(string? message, Exception? innerException) : Exception(message, innerException)
{
    public EvalException(string? message) : this(message, innerException: null) { }
    public EvalException() : this(message: null, innerException: null) { }
}
