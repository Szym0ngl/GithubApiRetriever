using Microsoft.Extensions.Configuration;

namespace GitHubRetriever
{
    public class DbConfiguration
    {
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }

        public static DbConfiguration Retrieve(string fileName)
        {
            var builder = new ConfigurationBuilder().AddJsonFile(fileName).Build();
            var configuration = new DbConfiguration();
            builder.Bind(configuration);

            return configuration;
        }
    }
}