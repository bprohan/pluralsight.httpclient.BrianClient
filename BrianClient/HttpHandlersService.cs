using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Movies.Client.Models;

namespace BrianClient
{
    public class HttpHandlersService : BackgroundService
    {
        private HttpClient _httpClient;

        public HttpHandlersService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MoviesClient");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            await GetMoviesWithRetryPolicy(cancellationTokenSource.Token);
        }

        private async Task GetMoviesWithRetryPolicy(CancellationToken cancellationToken)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using var response =
                await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
        }
    }
}
