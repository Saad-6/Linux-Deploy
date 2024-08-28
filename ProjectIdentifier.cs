using lxd.Models;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace lxd
{
    public static class ProjectIdentifier
    {
        //  public Dictionary<string,string> Files = new Dictionary<string,string>();
        public static ProjectType DetermineProjectType(string rootFolderPath)
        {
            if (!Directory.Exists(rootFolderPath))
            {
                return ProjectType.NotFound;
            }
            var filesAndDirectories = Directory.GetFileSystemEntries(rootFolderPath, "*", SearchOption.TopDirectoryOnly);
            var patternMatches = new Dictionary<ProjectType, int>
            {
                { ProjectType.React, 0 },
                { ProjectType.NodeJS, 0 },
                {ProjectType.DotNet,0 },
                // Add other project types as needed
            };
            foreach (var item in filesAndDirectories)
            {
                
                    var filename = Path.GetFileName(item);
                    foreach (var pattern in ProjectTypePatterns.Patterns)
                    {
                        foreach(var match in pattern.Value)
                        {
                          
                            if (match == filename)
                            {
                                patternMatches[pattern.Key]++;
                            }
                        }                    
                    } 
            }
            var highestMatch = patternMatches.OrderByDescending(p => p.Value).FirstOrDefault();

            if (highestMatch.Value > 0)
            {
                return highestMatch.Key;
            }
            return ProjectType.NotFound;
        }
        public static string GetProjectVersion(ProjectType projectType, string rootFolderPath)
        {
            switch (projectType)
            {
                case ProjectType.React:
                    var packageJsonPath = Path.Combine(rootFolderPath, "package.json");
                    if (File.Exists(packageJsonPath))
                    {
                        var packageJsonContentReact = File.ReadAllText(packageJsonPath);
                        var packageJsonReact = JsonConvert.DeserializeObject<dynamic>(packageJsonContentReact);
                        return  packageJsonReact?.dependencies.react?.ToString();
                    }
                    break;
                case ProjectType.NodeJS:
                    string whichOneExist; ;
                    var packageJsonPathnode = Path.Combine(rootFolderPath, "package.json");
                    var packageJsonPathnode2 = Path.Combine(rootFolderPath, "src","package.json");
                    if (File.Exists(packageJsonPathnode))
                    {
                        whichOneExist = packageJsonPathnode;
                    }
                    else
                    {
                        whichOneExist = packageJsonPathnode2;
                    }
                    string version;
                    var packageJsonContent = File.ReadAllText(whichOneExist);
                    var packageJson = JsonConvert.DeserializeObject<dynamic>(packageJsonContent);
                    version = packageJson?.dependencies.express?.ToString();
                    if (version != "")
                    {
                        version = "Express" + version;
                        return version;
                    }
                    packageJson?.dependencies?["@nestjs/common"]?.ToString();        
                    version =  "NestJS" + version ;
                    return version;

                case ProjectType.DotNet:
                    var csprojPath = Directory.GetFiles(rootFolderPath, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
                    if (!string.IsNullOrEmpty(csprojPath))
                    {
                        var csprojContent = File.ReadAllText(csprojPath);
                        var targetFrameworkRegex = new Regex("<TargetFramework>(.*?)</TargetFramework>");
                        var targetFrameworksRegex = new Regex("<TargetFrameworks>(.*?)</TargetFrameworks>");

                        var match = targetFrameworkRegex.Match(csprojContent);
                        if (match.Success)
                        {
                            return match.Groups[1].Value; // This will return the framework version, e.g., net6.0
                        }

                        match = targetFrameworksRegex.Match(csprojContent);
                        if (match.Success)
                        {
                            return match.Groups[1].Value.Split(';').First(); // If multiple frameworks are specified, return the first one
                        }
                    }
                    break;
            }

            return "Unknown";
        }
    }
    }
