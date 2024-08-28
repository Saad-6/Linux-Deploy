using lxd;
using lxd.Models;
using lxd.Utlilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


class Commands
{
    private static string currentDirectory = Directory.GetCurrentDirectory();
    private static string projectDirectory = null;

    private static readonly Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>
        {
            { "greet", args => Greet(args) },
            { "cd", args => ChangeDirectory(args) },
            {"clr",aegs => Console.Clear() },
            { "rootdir", args => RootDir(args) },
            { "run", args => Run(args) },
            { "help", args => Help() },
            { "exit", args => Exit() }
        };

    static void Main(string[] args)
    {
        while (true)
        {
            Console.Write($"{currentDirectory}# ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            var splitInput = input.Split(' ');
            var command = splitInput[0];
            var commandArgs = splitInput[1..];

            if (!commands.ContainsKey(command))
            {
                Console.WriteLine("Unknown command.");
                Help();
                continue;
            }

            commands[command](commandArgs);
        }
    }

    private static void Greet(string[] args)
    {
        var name = args.Length > 0 ? args[0] : "stranger";
        Console.WriteLine($"Hello, {name}!");
    }
 
    private static void RootDir(string[] args)
    {
        if (args.Length == 0 || !Directory.Exists(args[0]))
        {
            Console.WriteLine("Invalid directory path.");
            return;
        }
        projectDirectory = args[0];
        var type = ProjectIdentifier.DetermineProjectType(projectDirectory);
        var version = ProjectIdentifier.GetProjectVersion(type, projectDirectory);
        Console.WriteLine("Project Type: " + type.ToString() + "  " + version);
        currentDirectory = projectDirectory;
    }
    private static void ChangeDirectory(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a directory path.");
            return;
        }

        string newDirectory = args[0];

        try
        {
            if (Directory.Exists(newDirectory))
            {
                currentDirectory = newDirectory;
                projectDirectory = newDirectory;
                Console.WriteLine($": {currentDirectory}");
            }
            else
            {
                Console.WriteLine($"Directory does not exist: {newDirectory}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing directory: {ex.Message}");
        }
    }
    private static void Run(string[] args)
    {
        if (string.IsNullOrEmpty(projectDirectory))
        {
            Console.WriteLine("Error: You are not inside a project directory. Use the rootdir command to set a project directory.");
            return;
        }

        var type = ProjectIdentifier.DetermineProjectType(projectDirectory);
        if (type == ProjectType.NotFound)
        {
            Console.WriteLine("Error: No valid project found in the specified directory.");
            return;
        }
        var mode = args.Length > 0 ? args[0].ToLower() : "debug"; // Default to "debug" if no mode is specified

        switch (type)
        {
            case ProjectType.DotNet:
                Console.WriteLine($"Running .NET project in {mode} mode...");
                RunDotNetProject(mode);
                break;

            case ProjectType.NodeJS:
                Console.WriteLine("Running Node.js project...");
                RunNodeJSProject();
                break;

            case ProjectType.React:
                Console.WriteLine("Running React project...");
                RunReactProject();
                break;
            // Add cases for other project types like React, Django, etc.
            default:
                Console.WriteLine($"Running {type} project is not suppoeted yet.");
                break;
        }
    }
    private static void RunDotNetProject(string mode)
    {
        try
        {
            var configuration = mode == "release" ? "Release" : "Debug";
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project {projectDirectory} --configuration {configuration}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
                Console.WriteLine(process.StandardOutput.ReadToEnd());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running .NET project: {ex.Message}");
        }
    }

    private static void RunNodeJSProject()
    {
        try
        {
            string[] possibleFiles = { "index.js", "Index.js", "server.js", Path.Combine("src", "index.js") };
            string fileToRun = null;

            foreach (var file in possibleFiles)
            {
                var fullPath = Path.Combine(projectDirectory, file);
                if (File.Exists(fullPath))
                {
                    fileToRun = fullPath;
                    break;
                }
            }

            if (fileToRun == null)
            {
                Console.WriteLine("Error: No entry JavaScript file found in the project directory.");
                return;
            }

            System.Diagnostics.Process.Start("node", fileToRun);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running Node.js project: {ex.Message}");
        }
    }

    private static void RunReactProject()
    {
        try
        {
            string command = "npm start";
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd \"{projectDirectory}\" && {command}",
                WorkingDirectory = projectDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    Console.WriteLine(output);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running React project: {ex.Message}");
        }
    }
    private static void Help()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  greet [name] - Greets the user with the given name.");
        Console.WriteLine("  rootdir [path] - Sets the root directory of the project.");
        Console.WriteLine("  run - Runs the project if in a valid directory.");
        Console.WriteLine("  exit - Exits the tool.");
    }

    private static void Exit()
    {
        Console.WriteLine("Exiting...");
        Environment.Exit(0);
    }
}