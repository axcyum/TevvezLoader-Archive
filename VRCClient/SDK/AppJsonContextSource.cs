using System.Text.Json.Serialization;
using VRClient;

namespace Helper.Sdk
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigDto))]
    [JsonSerializable(typeof(Dictionary<string, float[]>))] // für ColorModule
    internal partial class AppJsonContext : JsonSerializerContext { }
}
