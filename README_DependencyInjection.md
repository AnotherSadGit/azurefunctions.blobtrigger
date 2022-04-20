Dependency Injection in an Azure Function
=========================================
Simon Elms, 27 Mar 2022

References
----------
* "Use dependency injection in .NET Azure Functions", https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection

Required NuGet Packages
-----------------------
* Microsoft.Azure.Functions.Extensions
* Microsoft.NET.Sdk.Functions, version 1.0.28 or later
* Microsoft.Extensions.DependencyInjection, version 3.x or *earlier*

1. Create a Startup class that inherits from FunctionsStartup.  Note the using directives and the FunctionsStartup assembly attribute that specifies the type name used during startup:

```
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
```

2. Inject the interfaces into the Azure Function class (or into whichever other class they're needed):

```
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlobTrigger
{
    public class BlobTrigger
    {
        private readonly IWorker _worker;
        public BlobTrigger(IWorker worker)
        {
            this._worker = worker;
        }

        [FunctionName("BlobTrigger")]
        public void Run([BlobTrigger("%BlobPath%/{name}", Connection = "AzureStorageConnectionString")]Stream myBlob, 
            string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var configRoot = GetConfigurationRoot(context);
            var blobPath = configRoot.GetValue<string>("BlobPath");
            log.LogInformation($"Value of BlobPath setting: {blobPath}");
			
			// Use the injected service.
            this._worker.DoWork();
        }

        public IConfigurationRoot GetConfigurationRoot(ExecutionContext context)
        {
            var configRoot = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return configRoot;
        }
    }
}
```

The Options Pattern with Dependency Injection
---------------------------------------------
The Options pattern involves binding an options object to a configuration section, then registering the options object with the DI container.  The options object can then be used anywhere in code, rather than having to read settings from the configuration.

The AddOptions and Configure methods can be used to bind an options object to a configuration section then register it with the DI container in a single step.  However, they wrap the options object in an IOptions<TOptions> wrapper (where TOptions is the options type that will be bound) and register that IOptions object with the DI container.

To avoid the IOptions wrapper bind the options object and register it with the DI container in two separate steps.

This all occurs in the Startup class.  For example, the Startup class above modified to bind and register a CustomerOptions object:

```
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

            CustomerOptions customerOptions = 
                context.Configuration.GetSection("CustomerOptions").Get<CustomerOptions>();

            services
                .AddSingleton(customerOptions)
                .AddSingleton<IWorker, Worker>();
        }
    }
}
```

Notes
-----
1. The DI container only holds explicitly registered types.  Function-specific types like BindingContext and ExecutionContext aren't available for injection.

2. You cannot use services registered with the DI container in the startup class, they are only available once startup is complete.  For example, don't try to use a logger registered with the DI container during startup.