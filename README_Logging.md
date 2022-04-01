Logging in an Azure Function
============================
Simon Elms, 28 Mar 2022

References
----------
* "Use dependency injection in .NET Azure Functions - ILogger<T> and ILoggerFactory section", https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#iloggert-and-iloggerfactory

* Comment on Azure-Functions GitHub issue "ILogger not logging to console after moving to Azure Functions OOB DI #1256", https://github.com/Azure/Azure-Functions/issues/1256#issuecomment-505478565 - modifying the host.json file to enable logging for a particular class or namespace.

* Comments on azure-functions-host GitHub issue "Use scopes to direct logs to correct file #4689", https://github.com/Azure/azure-functions-host/issues/4689#issuecomment-533195224 and https://github.com/Azure/azure-functions-host/issues/4689#issuecomment-751642356 - workaround to get an ILogger logging to the streaming logs from a class other than the Azure Function class 

Notes
-----
Some articles say we need to call `IServiceCollection.AddLogging();` in the Startup class.  That isn't necessary now; logging is included in the DI container automatically, without needing to be explicitly added.

Steps
-----
1. In the class to inject the logger into, inject an ILoggerFactory rather than ILogger or ILogger<T>.

2. In the constructor of the class call `ILoggerFactory.CreateLogger(categoryName);` to generate the ILogger that will do the logging.  The Azure Portal logging system will filter out any logs that do not have category name `Function.{FunctionClassName}.User`.  One way to generate this is to use `Microsoft.Azure.WebJobs.Logging.LogCategories.CreateFunctionUserCategory("{FunctionClassName}")`.  For example:

```
using Microsoft.Extensions.Logging;

namespace BlobTrigger
{
    public interface IWorker
    {
        void DoWork();
    }

    public class Worker : IWorker
    {
        private readonly ILogger _logger;

        // Have to specify the name of the Azure Function class, in this case "BlobTrigger".
        public Worker(ILoggerFactory loggerFactory) =>
            _logger = loggerFactory.CreateLogger(Microsoft.Azure.WebJobs.Logging.LogCategories.CreateFunctionUserCategory("BlobTrigger")); 
            
        public void DoWork()
        {
            _logger.LogInformation("Worker running");
        }
    }
}
```

3. Logging levels are set in the host.json file.  By default logging is disabled for all classes apart from the Function class.  To set a logging level for another class, in the "logging" section of the host.json file add a "logLevel" sub-section.  One of the following will enable logging for classes apart from the Function class:

    a. Set a "default" logging level, for example `"default": "Information"`.  This will apply to all functions in the function app.
    b. Set a namespace-level logging level, for example `"BlobTrigger": "Information"`
    c. Set a class-level logging level, for example `"BlobTrigger.Worker": "Information"`

```
{
    "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    },
    "logLevel": {
      "BlobTrigger": "Information"
    }
  }
}
```
