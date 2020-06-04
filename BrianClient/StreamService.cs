using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BrianClient
{
    public class StreamService : BackgroundService
    {
        public ILogger _logger { get; }
        private static HttpClient _httpClient = new HttpClient(
            new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.None }
            );
        public StreamService(ILogger<StreamService> logger)
        {
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://localhost:5001");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await GetPosterTests();
            //await PostPosterTests();
            await GetPosterWithGZipCompression();
            await Task.CompletedTask;
        }

        private async Task PostPosterTests()
        {
            await PostPosterWithStreamAndReadWithStream();
        }

        private async Task PostPosterWithStreamAndReadWithStream()
        {
            // generate a movie poster of 500KB
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            var ms = new MemoryStream();
            ms.SerializeToJsonAndWrite(posterForCreation);

            ms.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var streamContent = new StreamContent(ms))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        var createdContentStream = await response.Content.ReadAsStreamAsync();
                        var createdPoster = createdContentStream.ReadAndDeserializeFromJson<Poster>();
                    }
                }
            }
        }

        private async Task PostPosterWithStream()
        {
            // generate a movie poster of 500KB
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            // we dont need a single in-memory string since we will use a stream instead.
            //var json = JsonConvert.SerializeObject(posterForCreation);

            // IN fact, this can be replaced by an extension method
            //using (var ms = new MemoryStream())
            //using (var sw = new StreamWriter(ms, new UTF8Encoding(), 1024, true))
            //using (var jsonTxtWriter = new JsonTextWriter(sw))
            //{
            //    var jsonSerializer = new JsonSerializer();
            //    jsonSerializer.Serialize(jsonTxtWriter, posterForCreation);
            //    jsonTxtWriter.Flush();
            //}

            var ms = new MemoryStream();
            ms.SerializeToJsonAndWrite(posterForCreation);

            ms.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var streamContent = new StreamContent(ms))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var createdContent = await response.Content.ReadAsStringAsync();
                    var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);
                }
            }
        }

        private async Task GetPosterTests()
        {
            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 500; i++)
                await GetPosterWithoutStream();

            var a = stopWatch.ElapsedMilliseconds;
            stopWatch.Restart();

            for (int i = 0; i < 500; i++)
                await GetPosterWithStreamAndCompletionMode();

            var b = stopWatch.ElapsedMilliseconds;
        }

        private async Task GetPosterWithGZipCompression()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();
                _logger.LogInformation($"Got poster {poster.Name}");
            }
        }

        private async Task GetPosterWithStreamAndCompletionMode()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();

                //using (var streamReader = new StreamReader(stream))
                //{
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                //    }
                //}
            }
        }

        private async Task GetPosterWithoutStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var poster = JsonConvert.DeserializeObject<Poster>(content);
        }

        private async Task GetPosterWithStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();
                //using (var streamReader = new StreamReader(stream))
                //{
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                //    }
                //}
            }
        }
    }
}
