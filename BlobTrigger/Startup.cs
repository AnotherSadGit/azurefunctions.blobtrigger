using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BlobTrigger.Startup))]

namespace BlobTrigger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            RegisterServicesWithDI(builder.Services);
        }

        private void RegisterServicesWithDI(IServiceCollection services)
        {
            services
                .AddSingleton<IWorker, Worker>();
        }
    }
}
