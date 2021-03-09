using Newtonsoft.Json;

namespace GitHubRetriever.Models
{
    public class RepositoryData
    {
        public RepositoryData(string userName, string repository, string sha, string message, Commiter commiter)
        {
            UserName = userName;
            Repository = repository;
            Sha = sha;
            Message = message;
            Commiter = commiter;
            Id = string.Concat(UserName, Repository, Sha);
        }

        public string UserName { get; set; }
        public string Repository { get; set; }
        public string Sha { get; set; }
        public string Message { get; set; }
        public Commiter Commiter { get; set; }

        [JsonProperty(PropertyName = "id")] public string Id { get; }
    }
}