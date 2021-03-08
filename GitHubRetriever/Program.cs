using System;
using System.Net.Http;
using Autofac;
using GitHubRetriever;
using Serilog.Core;
using Serilog;

namespace ABB.Ability.IoTHub.Receiver
{
    internal class Program
    {
        private static IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<GitHubApiService>();
            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<HttpClientHandler>().SingleInstance();
            builder.RegisterType<CommitsDownloadCommand>().SingleInstance();
            return builder.Build();
        }

        public static void Main()
        {
            Console.WriteLine("Reading configuration");
            var coreConfiguration = Configuration.Retrieve("appSettings.json");
            CompositionRoot().Resolve<GitHubApiService>().Run(coreConfiguration);
        }
    }
}