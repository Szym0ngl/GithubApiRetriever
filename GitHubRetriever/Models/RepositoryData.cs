using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubRetriever.Models
{
    public class RepositoryData
    {
        public string UserName { get; set; }
        public string Repository { get; set; }
        public string Sha { get; set; }
        public string Message { get; set; }
        public string Commiter { get; set; }
    }
}
