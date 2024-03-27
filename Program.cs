

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StateMachineDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection collection = new ServiceCollection();
            collection.AddSingleton<ILogger>(sp =>
            {
                return sp.GetRequiredService<ILogger<Program>>();
            });
            collection.AddLogging(configure =>
            {
                configure.AddConsole();
            });
            var serviceProvider = collection.BuildServiceProvider();
            RestartWebAppJob job = new RestartWebAppJob(serviceProvider);

            while (!job.IsComplete)
            {
                await job.Execute(null);
            }
        }
    }

}