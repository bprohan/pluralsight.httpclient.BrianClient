using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BrianClient
{
    public interface IIntegrationService
    {
        Task Run();
    }


    public class TestIntegrationService : IIntegrationService
    {
        private readonly ILogger<TestIntegrationService> _logger;
        public TestIntegrationService(ILogger<TestIntegrationService> logger)
        {
            _logger = logger;
        }

        public Task Run()
        {
            _logger.LogInformation("In TestIntegrationService");
            return Task.CompletedTask;
        }
    }
}