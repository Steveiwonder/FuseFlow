namespace FuseFlow.Core;

public interface IJobParamSerializer
{
    Task<string> Serialize(object param);
    Task<T> Deserialize<T>(string param);
}
