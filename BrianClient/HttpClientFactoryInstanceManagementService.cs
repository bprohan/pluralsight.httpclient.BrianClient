using Microsoft.Extensions.Logging;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BrianClient
{
    public class HttpClientFactoryInstanceManagementService : IIntegrationService
    {
        private readonly ILogger<HttpClientFactoryInstanceManagementService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MoviesClient _moviesClient;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        // The HttpFactory, or any named HTTPClient can be injected
        public HttpClientFactoryInstanceManagementService(ILogger<HttpClientFactoryInstanceManagementService> logger,
            IHttpClientFactory httpClientFactory, MoviesClient moviesClient)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _moviesClient = moviesClient;
        }

        public async Task Run()
        {
            _logger.LogInformation("HttpClientFactoryInstanceManagementService Running");
            await GetMoviesWithTypedHttpClientFactory(_cancellationTokenSource.Token);
        }


        private async Task GetMoviesWithTypedHttpClientFactory(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            //using var response =
            //    await _moviesClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var movies = await _moviesClient.GetMovies(cancellationToken);
        }

        private async Task GetMoviesWithNamedHttpClientFactory(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using var response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
        }


        private async Task GetMoviesWithHttpClientFactory(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
        }

        // simple example where httpclient is created for every request, leaves many sockets open for some minutes
        private async Task TestDisposeClient(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com");

                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();

                Console.WriteLine($"Request completed with status code {response.StatusCode}");
            }

        }

        // simple example where httpclient is re-used, leaves single sockets open.. but may have problems with dns changes etc.
        private async Task TestReuseHttpClient(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            for (int i = 0; i < 10; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com");
                var response = await httpClient.SendAsync(request, cancellationToken);

                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();

                Console.WriteLine($"Request completed with status code {response.StatusCode}");

            }

        }
    }
}
