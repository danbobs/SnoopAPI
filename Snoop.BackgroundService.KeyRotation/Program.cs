using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Snoop.Background.KeyRotation.Interfaces;
using Snoop.Background.KeyRotation.Services;

namespace Snoop.Background.KeyRotation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<KeyRotator>();
                    services.AddSingleton<IEncryptionServiceWrapper, EncryptionServiceWrapper>();
                });
    }
}
