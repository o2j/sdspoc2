using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sds.Dal.Repos;
using SdsLogging.Models;
using System.Threading.Tasks;

namespace SdsLogging
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration(config => config
                    .AddUserSecrets<Program>())
                .ConfigureServices(services =>
                {
                    services.AddSingleton(sp =>
                    {
                        IConfiguration configuration = sp.GetService<IConfiguration>();
                        CosmosClientBuilder clientBuilder = new CosmosClientBuilder(configuration["CosmosEndpoint"], configuration["CosmosAuthKey"]);
                        CosmosClient client = clientBuilder
                            .WithConnectionModeDirect()
                            .Build();
                        return new LoggingRepo(client);
                    }).AddTransient(sp =>
                    {
                        IConfiguration configuration = sp.GetService<IConfiguration>();
                        PdfAccessLogFunctionSettings functionSettings = new PdfAccessLogFunctionSettings();
                        configuration.Bind(functionSettings);

                        return functionSettings;
                    }).AddTransient(sp =>
                    {
                        IConfiguration configuration = sp.GetService<IConfiguration>();
                        GraphDemoSettings graphDemoSettings = new GraphDemoSettings();
                        configuration.Bind(graphDemoSettings);

                        return graphDemoSettings;
                    });
                })
                .Build();

             host.Run();
        }
    }
}