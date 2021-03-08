using System;
using System.Collections.Generic;
using System.Net.Http;
using GitHubRetriever.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;

namespace GitHubRetriever
{
    public class CommitsDownloadCommand
    {
        private const int MaxRetryAttempts = 3;
        private const string ApplicationJson = "application/vnd.github.v3+json";
        private readonly HttpClient httpClient;
        private readonly TimeSpan pauseBetweenFailures = TimeSpan.FromSeconds(2);

        public CommitsDownloadCommand(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public List<RepositoryData> Execute(string userName, string repository)
        {
            var content = CreateGetRequest($"https://api.github.com/repos/{userName}/{repository}/branches");
            var branches = JsonConvert.DeserializeObject<List<Branch>>(content);

            var repositoryData = new List<RepositoryData>();

            foreach (var branch in branches)
            {
                content = CreateGetRequest(
                    $"https://api.github.com/repos/{userName}/{repository}/commits/{branch.Commit.Sha}");
                var message = JObject.Parse(content)["commit"]?["message"];
                var commiter = JObject.Parse(content)["commit"]?["committer"]?.ToObject<Commiter>();
                repositoryData.Add(new RepositoryData(userName, repository, branch.Commit.Sha, message?.ToString(),
                    commiter));
            }

            return repositoryData;
        }

        private string CreateGetRequest(string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", ApplicationJson);
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetry(MaxRetryAttempts, i => pauseBetweenFailures);

            var response = new HttpResponseMessage();
            retryPolicy.Execute(() =>
            {
                response = httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
            });

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
    }
}