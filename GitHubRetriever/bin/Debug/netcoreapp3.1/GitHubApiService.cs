using ABB.Ability.IoTHub.Receiver;
using Serilog;

namespace GitHubRetriever
{
    public class GitHubApiService
    {
        private readonly CommitsDownloadCommand commitsDownload;

        public GitHubApiService(CommitsDownloadCommand commitsDownload)
        {
            this.commitsDownload = commitsDownload;
        }

        public void Run(Configuration configuration)
        {
            commitsDownload.Execute();
        }
    }
}