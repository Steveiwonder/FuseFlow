namespace FuseFlow.Core;

public interface IDataSerializer
{
    Task<string> Serialize(object param);
    Task<T> Deserialize<T>(string param);
}
