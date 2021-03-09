using System;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using GitHubRetriever.Commands;
using Microsoft.Azure.Cosmos;

namespace GitHubRetriever
{
    internal class Program
    {
        private static Container container;
        private CosmosClient cosmosClient;
        private Database database;

        private static IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<GitHubApiService>();
            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<HttpClientHandler>().SingleInstance();
            builder.RegisterType<CommitsDownloadCommand>().SingleInstance();
            return builder.Build();
        }

        public static async Task Main()
        {
            try
            {
                var dbConfiguration = DbConfiguration.Retrieve("appSettings.json");
                await new Program().CreateCosmosDb(dbConfiguration);
                Console.WriteLine("Enter user name: ");
                var userName = Console.ReadLine();
                Console.WriteLine("Enter repository name: ");
                var repository = Console.ReadLine();
                await CompositionRoot().Resolve<GitHubApiService>().Run(container, userName, repository);
            }
            catch (CosmosException de)
            {
                var baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of program, press any key to exit.");
                Console.ReadKey();
            }
        }

        public async Task CreateCosmosDb(DbConfiguration dbConfiguration)
        {
            cosmosClient = new CosmosClient(dbConfiguration.EndpointUri, dbConfiguration.PrimaryKey);
            await CreateDatabaseAsync(dbConfiguration.DatabaseId);
            await CreateContainerAsync(dbConfiguration.ContainerId);
        }

        private async Task CreateDatabaseAsync(string databaseId)
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        private async Task CreateContainerAsync(string containerId)
        {
            container = await database.DefineContainer(containerId, "/UserName")
                .WithUniqueKey()
                .Path("/UserName")
                .Path("/Repository")
                .Path("/Sha")
                .Attach()
                .CreateIfNotExistsAsync();
        }
    }
}