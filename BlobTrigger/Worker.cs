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
