using System.Text.Json.Serialization;
using FileServer.Models.Auth;
using FileServer.Models.Files;

namespace FileServer;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    RespectNullableAnnotations = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(GetFilesListResponse))]
[JsonSerializable(typeof(UploadFileResponse))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(Token))]
[JsonSerializable(typeof(Claim))]
internal sealed partial class Jsc : JsonSerializerContext;
