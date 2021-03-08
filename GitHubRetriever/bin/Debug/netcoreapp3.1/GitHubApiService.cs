using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GitHubRetriever.Models;
using Microsoft.Azure.Cosmos;

namespace GitHubRetriever
{
    public class GitHubApiService
    {
        private readonly CommitsDownloadCommand commitsDownload;

        public GitHubApiService(CommitsDownloadCommand commitsDownload)
        {
            this.commitsDownload = commitsDownload;
        }

        public async Task Run(Container container, string userName, string repository)
        {
            var repositoryData = commitsDownload.Execute(userName, repository);
            await AddItemsToContainerAsync(repositoryData, container);
        }

        private async Task AddItemsToContainerAsync(List<RepositoryData> repositoryData, Container container)
        {
            foreach (var data in repositoryData)
                try
                {
                    var response =
                        await container.ReadItemAsync<RepositoryData>(data.Id,
                            new PartitionKey(data.UserName));
                    Console.WriteLine("Item in database with id: {0} already exists\n", response.Resource.Id);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var response =
                        await container.CreateItemAsync(data,
                            new PartitionKey(data.UserName));

                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n",
                        response.Resource.Id, response.RequestCharge);
                }
        }
    }
}