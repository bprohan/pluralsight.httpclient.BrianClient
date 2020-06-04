using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BrianClient
{
    public class RetryPolicyDelegatingHandler : DelegatingHandler
    {
        private readonly int _maximumAmountOfRetries = 3;

        public RetryPolicyDelegatingHandler(int maxNumberOfRetries) : base()
        {
            _maximumAmountOfRetries = maxNumberOfRetries;
        }

        public RetryPolicyDelegatingHandler(HttpMessageHandler innerHandler, int maxNumberOfRetries) : base(
            innerHandler)
        {
            _maximumAmountOfRetries = maxNumberOfRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < _maximumAmountOfRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return response;
        }
    }
}