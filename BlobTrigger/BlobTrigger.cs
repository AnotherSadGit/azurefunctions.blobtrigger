using System;
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
        // Original, out of the box, code:
        //  public void Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureStorageConnectionString")] Stream myBlob, string name, ILogger log)
        public void Run([BlobTrigger("%BlobPath%/{name}", Connection = "AzureStorageConnectionString")]Stream myBlob, 
            string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var configRoot = GetConfigurationRoot(context);
            var blobPath = configRoot.GetValue<string>("BlobPath");
            log.LogInformation($"Value of BlobPath setting: {blobPath}");
            this._worker.DoWork();

            // Output in the logs:
            /*
            2022-03-28T04:22:48.097 [Information] Executing 'BlobTrigger' (Reason='New blob detected: samples-workitems/data/input-files/Test.txt', Id=4a1fef9e-7641-4b6f-8d83-b2096b1d010c)
            2022-03-28T04:22:48.111 [Information] Trigger Details: MessageId: 2bf3b45b-92c6-4f30-aa76-591635be3e15, DequeueCount: 1, InsertionTime: 2022-03-28T04:22:47.000+00:00, BlobCreated: 2022-03-28T02:15:30.000+00:00, BlobLastModified: 2022-03-28T04:22:40.000+00:00
            2022-03-28T04:22:48.129 [Information] C# Blob trigger function Processed blobName:Test.txtSize: 15 Bytes
            2022-03-28T04:22:48.132 [Information] Value of BlobPath setting: samples-workitems/data/input-files
            2022-03-28T04:22:48.133 [Information] Worker running
            2022-03-28T04:22:48.153 [Information] Executed 'BlobTrigger' (Succeeded, Id=4a1fef9e-7641-4b6f-8d83-b2096b1d010c, Duration=145ms)              
             */

            // Note the two Information messages written explicitly by the function, and the one written by the Worker class.
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
