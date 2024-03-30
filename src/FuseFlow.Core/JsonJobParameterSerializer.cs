using System.Runtime.InteropServices;
using System.Text.Json;

namespace FuseFlow.Core;

public class JsonJobParameterSerializer : IDataSerializer
{
    public Task<T> Deserialize<T>(string param)
    {
        return Task.FromResult(JsonSerializer.Deserialize<T>(param));
    }

    public Task<string> Serialize(object param)
    {
        if (param is null)
        {
            return Task.FromResult<string>(null);
        }
        return Task.FromResult(JsonSerializer.Serialize(param));
    }
}
