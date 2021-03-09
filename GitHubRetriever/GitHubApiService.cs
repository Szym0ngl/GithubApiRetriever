using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GitHubRetriever.Commands;
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

        private static async Task AddItemsToContainerAsync(IEnumerable<RepositoryData> repositoryData,
            Container container)
        {
            foreach (var data in repositoryData)
            {
                Console.WriteLine($"[{data.Repository}]/[{data.Sha}]: {data.Message}[{data.Commiter}]");
                try
                {
                    await container.ReadItemAsync<RepositoryData>(data.Id, new PartitionKey(data.UserName));
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await container.CreateItemAsync(data, new PartitionKey(data.UserName));
                }
            }
        }
    }
}