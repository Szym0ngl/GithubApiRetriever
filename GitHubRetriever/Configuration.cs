using Microsoft.Extensions.Configuration;

namespace ABB.Ability.IoTHub.Receiver
{
    public class Configuration
    {
        /// <summary>
        ///     Connection string for the Azure Storage account used for checkpointing and leasing
        /// </summary>
        public string StorageConnectionString { get; set; }

        /// <summary>
        ///     Name of the container to store checkpointing and leasing information
        /// </summary>
        public string StorageContainerName { get; set; }

        /// <summary>
        ///     Retrieves a populated core configuration object based on the contents of a file
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve the configuration from</param>
        /// <returns>A configuration object populated with the core data</returns>
        public static Configuration Retrieve(string fileName)
        {
            var builder = new ConfigurationBuilder().AddJsonFile(fileName).Build();
            var configuration = new Configuration();
            builder.Bind(configuration);

            return configuration;
        }
    }
}