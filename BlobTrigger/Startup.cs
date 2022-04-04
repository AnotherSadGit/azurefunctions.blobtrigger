using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BlobTrigger.Startup))]

namespace BlobTrigger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            RegisterServicesWithDI(builder);
        }

        // Notice this version of RegisterServicesWithDI needs to take an IFunctionsHostBuilder 
        // parameter, instead of an IServiceCollection parameter, since we need access to the 
        // builder context to be able to bind the options object to the configuration section.
        private void RegisterServicesWithDI(IFunctionsHostBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
            IServiceCollection services = builder.Services;

            // Notice that the nested AddressOptions object is automatically bound to the Address 
            // sub-section of the configuration section.  We don't need to bind the nested 
            // options object explicitly.
            CustomerOptions customerOptions = 
                context.Configuration.GetSection("CustomerOptions").Get<CustomerOptions>();

            services
                .AddSingleton(customerOptions)
                .AddSingleton<IWorker, Worker>();
        }
    }
}
