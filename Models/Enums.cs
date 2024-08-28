using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lxd.Models
{
    public class Enums
    {
    }
    public enum ProjectType
    {
        DotNet,
        NodeJS,
        React,
        Laravel,
        javaspring,
        django,
        flask,
        rubyonrails,
        cpp,
        NotFound
    }
    public static class ProjectTypePatterns
    {
        public static readonly Dictionary<ProjectType, List<string>> Patterns = new()
    {
        { ProjectType.React, new List<string> { "package.json", "src", "components", "index.js","index.ts","node_modules","public","app.jsx","app.tsx" } },
        { ProjectType.NodeJS, new List<string> { "package.json", "server.js", "app.js","app.ts", "node_modules","routes","views","deploy","build",
            "middleware","middlewares","model","models","bin","repositories","repository" } },
        { ProjectType.DotNet, new List<string> { "appsettings.json", "Program.cs","controllers","models","views","wwwroot","bin","obj","properties" } },
        
    };
    }
}
