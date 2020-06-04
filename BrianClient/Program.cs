using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BrianClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            CreateHostBuilder(args).Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
           .ConfigureServices((hostContext, services) =>
           {
               //services.AddHostedService<CrudService>();
               //services.AddHostedService<PartialUpdateService>();
               //services.AddHostedService<StreamService>();
               //services.AddHostedService<CancellationService>();
               services.AddHttpClient();


               //NamedClient
               services.AddHttpClient("MoviesClient", client =>
                   {
                       client.BaseAddress = new Uri("https://localhost:5001");
                       client.Timeout = new TimeSpan(0, 0, 30);
                       client.DefaultRequestHeaders.Clear();
                   })
                   .ConfigurePrimaryHttpMessageHandler(h =>
                       new HttpClientHandler()
                       {
                           AutomaticDecompression = System.Net.DecompressionMethods.GZip
                       });


               //TypedClient.Registered with a transient scope
               ////services.AddHttpClient<MoviesClient>(client =>
               ////    {
               ////        client.BaseAddress = new Uri("https://localhost:5001");
               ////        client.Timeout = new TimeSpan(0, 0, 30);
               ////        client.DefaultRequestHeaders.Clear();
               ////    })
               ////    .ConfigurePrimaryHttpMessageHandler(h =>
               ////        new HttpClientHandler()
               ////        {
               ////            AutomaticDecompression = System.Net.DecompressionMethods.GZip
               ////        });


               //TypedClient.Registered with a transient scope .. Default Configuration is moved to the MoviesClient constructor
               services.AddHttpClient<MoviesClient>()
                   .ConfigurePrimaryHttpMessageHandler(h =>
                       new HttpClientHandler()
                       {
                           AutomaticDecompression = System.Net.DecompressionMethods.GZip
                       });


               //services.AddScoped<IIntegrationService, TestIntegrationService>();
               services.AddScoped<IIntegrationService, HttpClientFactoryInstanceManagementService>();
               var sp = services.BuildServiceProvider();
               sp.GetService<IIntegrationService>().Run();
           });
    }



}
