
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Movies.Client.Models;
using Newtonsoft.Json;

namespace BrianClient
{
    public class DealingWithErrorsAndFaultsService : IIntegrationService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient1;

        public HttpClient _httpClient => _httpClient1;

        public DealingWithErrorsAndFaultsService(IHttpClientFactory httpClientFactory, ILogger<DealingWithErrorsAndFaultsService> logger)
        {
            _logger = logger;
            _httpClient1 = httpClientFactory.CreateClient("MoviesClient");
        }

        public async Task Run()
        {
            var stoppingToken = new CancellationTokenSource();
            //await GetMovieWithInvalidResponses(stoppingToken.Token);
            await PostMovieAndHandleValidationError(stoppingToken.Token);
        }


        private async Task PostMovieAndHandleValidationError(CancellationToken cancellationToken)
        {
            var movie = new MovieForCreation()
            {
                Title = "Pulp Fiction",
                Description = "Too short",   // Invalid length
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/movies/");

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Content = new StringContent(JsonConvert.SerializeObject(movie));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var errorStream = await response.Content.ReadAsStreamAsync();
                    var validationErrors = errorStream.ReadAndDeSerializeFromJson();
                    _logger.LogInformation($"Validation Error: {validationErrors}");
                }

                _logger.LogInformation("oops e doopsy");

                response.EnsureSuccessStatusCode();
            }

        }

        private async Task GetMovieWithInvalidResponses(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Response code was {(int)response.StatusCode} : {response.StatusCode}");
                return;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();

        }


    }
}
