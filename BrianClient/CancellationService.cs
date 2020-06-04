using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movies.Client.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BrianClient
{
    public class CancellationService : BackgroundService
    {
        public ILogger _logger { get; }
        private static HttpClient _httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.None });
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationService(ILogger<CancellationService> logger)
        {
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://localhost:5001");
            _httpClient.Timeout = new TimeSpan(0, 0, 1);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation($"GetTrailerAndCancel");
            _cancellationTokenSource.CancelAfter(2000);
            //await GetTrailerAndCancel(_cancellationTokenSource);
            await GetTrailerAndHandleTimeout(_cancellationTokenSource);
        }

        private async Task GetTrailerAndCancel(CancellationTokenSource cancellationTokenSource)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");
            _logger.LogInformation($"SendAsync http message");

            try
            {
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
                    _logger.LogInformation($"Trailer is {trailer.Name} with size {trailer.Bytes.Length / 1024} kb");
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation($"The operation was cancelled: {e.Message}");
            }
        }

        private async Task GetTrailerAndHandleTimeout(CancellationTokenSource cancellationTokenSource)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");
            _logger.LogInformation($"SendAsync http message");

            try
            {
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
                    _logger.LogInformation($"Trailer is {trailer.Name} with size {trailer.Bytes.Length / 1024} kb");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"The operation was cancelled: {e.Message}");
            }
        }
    }
}
