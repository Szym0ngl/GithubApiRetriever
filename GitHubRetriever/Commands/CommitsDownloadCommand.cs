using System;
using System.Collections.Generic;
using System.Net.Http;
using GitHubRetriever.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;

namespace GitHubRetriever.Commands
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
            var httpResponseMessage =
                CreateGetRequest($"https://api.github.com/repos/{userName}/{repository}/branches");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine($"User {userName} or repository {repository} not found");
                throw new Exception("Get request failed");
            }

            var branches =
                JsonConvert.DeserializeObject<List<Branch>>(httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter()
                    .GetResult());

            var repositoryData = new List<RepositoryData>();

            foreach (var branch in branches) //TODO this should be changed to use recurrency
            {
                var content = CreateGetRequest(
                        $"https://api.github.com/repos/{userName}/{repository}/commits/{branch.Commit.Sha}").Content
                    .ReadAsStringAsync().GetAwaiter().GetResult();
                var message = JObject.Parse(content)["commit"]?["message"];
                var commiter = JObject.Parse(content)["commit"]?["committer"]?.ToObject<Commiter>();
                repositoryData.Add(new RepositoryData(userName, repository, branch.Commit.Sha, message?.ToString(),
                    commiter));
                var isParentPresent = true;
                while (isParentPresent)
                {
                    var parentSha = JObject.Parse(content)["parents"]?.First?["sha"]; //TODO should do here recurrent function to iterate all parents
                    if (parentSha == null)
                    {
                        isParentPresent = false;
                    }
                    else
                    {
                        content = CreateGetRequest(
                                $"https://api.github.com/repos/{userName}/{repository}/commits/{parentSha}").Content
                            .ReadAsStringAsync().GetAwaiter().GetResult();
                        message = JObject.Parse(content)["commit"]?["message"];
                        commiter = JObject.Parse(content)["commit"]?["committer"]?.ToObject<Commiter>();
                        repositoryData.Add(new RepositoryData(userName, repository, branch.Commit.Sha, message?.ToString(),
                            commiter));
                    }
                }
            }

            return repositoryData;
        }

        private HttpResponseMessage CreateGetRequest(string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", ApplicationJson);
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetry(MaxRetryAttempts, i => pauseBetweenFailures);

            var response = new HttpResponseMessage();
            retryPolicy.Execute(() => { response = httpClient.SendAsync(request).Result; });

            return response;
        }
    }
}