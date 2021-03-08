using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubRetriever.Models
{
    public class Branch
    {
        public string Name { get; set; }
        public Commit Commit { get; set; }
    }
}
