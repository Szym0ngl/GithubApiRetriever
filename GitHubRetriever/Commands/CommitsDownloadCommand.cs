using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog;
using GitHubRetriever.Models;
using Newtonsoft.Json;
using Polly;
using System;
using System.Net.Http;
using Autofac;
using GitHubRetriever;
using Polly.Retry;
using Serilog.Core;
using Serilog;

namespace GitHubRetriever
{
    public class CommitsDownloadCommand
    {
        private const int MaxRetryAttempts = 3;
        private const string ApplicationJson = "application/vnd.github.v3+json";
        private readonly TimeSpan pauseBetweenFailures = TimeSpan.FromSeconds(2);
        private readonly HttpClient httpClient;

        public CommitsDownloadCommand(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public List<Branch> Execute(string userName, string repository)
        {
            var content = CreateGetRequest($"https://api.github.com/repos/{userName}/{repository}/branches");
            var branches =  JsonConvert.DeserializeObject<List<Branch>>(content);

            foreach (var branch in branches)
            {
                CreateGetRequest($"https://api.github.com/repos/{userName}/{repository}/commits/{branch.Commit.Sha}");
            }
            return branches;
        }

        private string CreateGetRequest(string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetry(MaxRetryAttempts, i => this.pauseBetweenFailures);

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