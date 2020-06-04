using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrianClient
{
    public class TimeOutDelegatingHandler : DelegatingHandler
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public TimeOutDelegatingHandler(TimeSpan timeout) : base()
        {
            _timeout = timeout;
        }

        public TimeOutDelegatingHandler(HttpMessageHandler innerHandler, TimeSpan timeout) : base(
            innerHandler)
        {
            _timeout = timeout;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var linkedCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                return await base.SendAsync(request, linkedCancellationTokenSource.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("The request timed out", ex);
                }

                throw;
            }
            
        }
    }
}
