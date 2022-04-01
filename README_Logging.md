Logging in an Azure Function
============================
Simon Elms, 28 Mar 2022

References
----------
* "Use dependency injection in .NET Azure Functions - ILogger<T> and ILoggerFactory section", https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#iloggert-and-iloggerfactory

* Comment on Azure-Functions GitHub issue "ILogger not logging to console after moving to Azure Functions OOB DI #1256", https://github.com/Azure/Azure-Functions/issues/1256#issuecomment-505478565 - modifying the host.json file to enable logging for a particular class or namespace.

* Comments on azure-functions-host GitHub issue "Use scopes to direct logs to correct file #4689", https://github.com/Azure/azure-functions-host/issues/4689#issuecomment-533195224 and https://github.com/Azure/azure-functions-host/issues/4689#issuecomment-751642356 - workaround to get an ILogger logging to the streaming logs from a class other than the Azure Function class 

* "Enable streaming execution logs in Azure Functions", https://docs.microsoft.com/en-us/azure/azure-functions/streaming-logs - how to view Azure Function App streaming logs (as opposed to the Function Code + Test logs).

Notes
-----
Some articles say we need to call `IServiceCollection.AddLogging();` in the Startup class.  That isn't necessary now; logging is included in the DI container automatically, without needing to be explicitly added.

Viewing Logs in Azure Portal
----------------------------
The logs the Azure Function writes to can be viewed in two places:  The Function 'Code + Test' logs and the Function App Streaming Logs.

### Azure Function Code + Test Logs
These logs are more exclusive, since they only display the output from the specific Azure Function, rather than all functions in the Function App.  However, they have a foible: They have a strict filter than only displays log entries with the category name `Function.{FunctionClassName}.User`.  In this sample project the category name would have to be `Function.BlobTrigger.User`.  This can filter out log entries in classes other than the function class.  For example, they would filter out log messages written in the Worker class in this project while displaying the log messages written in the BlobTrigger class.

To view these logs in the Azure Portal go to the Azure Function in question then, in the left blade menu select Developer > Code + Test and expand the Logs section at the bottom of the page.

### Function App Streaming Logs
These logs have a less strict filter than the Azure Function Code + Test logs; they will show log messages regardless of the category name.  However, they include log messages from all functions in the Function App, not just one function.

To view these logs in the Azure Portal go to the Function App in question then, in the left blade menu select Monitoring > Log stream.

For log messages written in an injected class to be visible in the Function Code + Test Logs
--------------------------------------------------------------------------------------------
We need to set the logger category name to `Function.{FunctionClassName}.User`.  To do that:

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

If you just want log messages visible in the Function App Streaming Logs
------------------------------------------------------------------------
In other words, if you don't mind that the log messages written in an injected class will NOT be visible in the Function Code + Test Logs.

In this case we can inject an ILogger<T> instead of an ILoggerFactory.

For example:

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
        private readonly ILogger<IWorker> _logger;

        public Worker(ILogger<IWorker> logger) => _logger = logger; 
            
        public void DoWork()
        {
            _logger.LogInformation("Worker running");
        }
    }
}
```

### Comparing the outputs of the Function App Streaming Logs and the Function Code + Test Logs
Here are the same log entries displayed in the Function App Streaming Logs and the Function Code + Test Logs.

Function App Streaming Logs:

```
2022-04-01T07:11:03.202 [Information] Trigger Details: MessageId: 205a09b3-3ade-42ec-ab46-e2e21f2973f6, DequeueCount: 1, InsertionTime: 2022-04-01T07:11:02.000+00:00, BlobCreated: 2022-03-28T02:15:30.000+00:00, BlobLastModified: 2022-04-01T07:10:59.000+00:00
2022-04-01T07:11:03.217 [Information] C# Blob trigger function Processed blobName:Test.txtSize: 15 Bytes
2022-04-01T07:11:03.220 [Information] Value of BlobPath setting: samples-workitems/data/input-files
2022-04-01T07:11:03.220 [Information] Worker running
2022-04-01T07:11:03.233 [Information] Executed 'BlobTrigger' (Succeeded, Id=6b5add3e-d1c8-4ca3-9886-00059419a6ec, Duration=97ms)
2022-04-01T07:11:34.176 [Information] Host Status: {"id": "blobstoragetriggerlocaldev","state": "Running","version": "4.1.3.17473","versionDetails": "4.1.3+3ed9ce8ebeef5b156badaf203a016a56e819a852","platformVersion": "97.0.7.624","instanceId": "1bb4d45ac106a35172331037aba09b93bd4669d8cec6a85ef0d9ab91054743f1","computerName": "10-30-2-3","processUptime": 1152255,"functionAppContentEditingState": "Unknown"}
```

Function Code + Test Logs: 

```
2022-04-01T07:11:03.198 [Information] Executing 'BlobTrigger' (Reason='New blob detected: samples-workitems/data/input-files/Test.txt', Id=6b5add3e-d1c8-4ca3-9886-00059419a6ec)
2022-04-01T07:11:03.202 [Information] Trigger Details: MessageId: 205a09b3-3ade-42ec-ab46-e2e21f2973f6, DequeueCount: 1, InsertionTime: 2022-04-01T07:11:02.000+00:00, BlobCreated: 2022-03-28T02:15:30.000+00:00, BlobLastModified: 2022-04-01T07:10:59.000+00:00
2022-04-01T07:11:03.217 [Information] C# Blob trigger function Processed blobName:Test.txtSize: 15 Bytes
2022-04-01T07:11:03.220 [Information] Value of BlobPath setting: samples-workitems/data/input-files
2022-04-01T07:11:03.233 [Information] Executed 'BlobTrigger' (Succeeded, Id=6b5add3e-d1c8-4ca3-9886-00059419a6ec, Duration=97ms)
```

Notice that the "Worker running" log entry, written by the Worker class, only appears in the Function App Streaming Logs.


Setting Logging Levels
----------------------
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
