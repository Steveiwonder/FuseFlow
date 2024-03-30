using System.Text.Json;

namespace FuseFlow.Core;

public class JsonJobParameterSerializer : IJobParamSerializer
{
    public Task<T> Deserialize<T>(string param)
    {
        return Task.FromResult(JsonSerializer.Deserialize<T>(param));
    }

    public Task<string> Serialize(object param)
    {
        return Task.FromResult(JsonSerializer.Serialize(param));
    }
}
