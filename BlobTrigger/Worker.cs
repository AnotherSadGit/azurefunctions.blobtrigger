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
        private readonly CustomerOptions _customerOptions;

        public Worker(ILoggerFactory loggerFactory, CustomerOptions customerOptions)
        {
            _logger = 
                loggerFactory.CreateLogger(Microsoft.Azure.WebJobs.Logging.LogCategories.CreateFunctionUserCategory("BlobTrigger")); 
            _customerOptions = customerOptions;
        }
            
        public void DoWork()
        {
            _logger.LogInformation("Worker running");
            _logger.LogInformation($"Customer Name: '{_customerOptions.Name}'");
            _logger.LogInformation($"Customer Address Street: '{_customerOptions.Address.Street}'");
        }
    }
}
