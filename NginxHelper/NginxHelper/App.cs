using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NginxHelper
{
    class App
    {
        public string Hosts;
        public string AppName;
        public const string HomePage = "https://github.com/yanxyz/nginx-windows-helper";

        private Config Cfg;
        private Options Opts;

        public App()
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            AppName = Path.GetFileNameWithoutExtension(appPath);
            var iniFile = Path.ChangeExtension(appPath, ".ini");
            Cfg = new Config(iniFile);
            Hosts = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"System32\drivers\etc\hosts");
        }

        public void Run(string[] args)
        {
            Opts = new Options(args);
            if (Opts.Help)
            {
                ShowHelp();
                return;
            }

            var firstArg = Opts.Args[0];
            string action = null;
            string[] actions = {
                "create",
                "start",
                "restart",
                "stop",
                "edit",
                "test",
                "nginx",
                "home",
            };
            var list = new List<string>();
            foreach (var item in actions)
            {
                if (item.StartsWith(firstArg))
                {
                    if (item == firstArg)
                    {
                        action = item;
                        break;
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }

            if (list.Count > 1)
            {
                Console.WriteLine("Did you mean one of these?");
                foreach (var item in list)
                    Console.WriteLine(item);
                return;
            }

            if (list.Count == 1)
                action = list[0];

            if (string.IsNullOrEmpty(action))
            {
                Console.WriteLine("Unknown command");
                return;
            }

            string serverName;
            string serverRoot = null;
            int argsLength = Opts.Args.Count;

            if (action == "create")
            {
                if (argsLength > 1)
                {
                    serverName = args[1];
                    if (argsLength > 2)
                        serverRoot = args[2];
                }
                else
                {
                    Console.WriteLine("ServerName is required");
                    return;
                }
                CreateVhost(serverName, serverRoot);
                return;
            }

            if (action == "edit")
            {
                if (argsLength > 1)
                {
                    var arg = args[1];
                    if (arg == "hosts")
                    {
                        EditHosts();
                        return;
                    }
                    else
                        serverName = args[1];
                }
                else
                {
                    Console.WriteLine("one argument is required: <ServerName | main | hosts>");
                    return;
                }

                Edit(serverName);
                return;
            }

            if (action == "test")
            {
                TestConf();
                if (argsLength > 1)
                {
                    var name = args[1];
                    if (CheckHosts(name)) return;
                    if (Prompt($"System hosts does not contain `{name}`. Add it to hosts right now?"))
                    {
                        EditHosts();
                    }
                }
                return;
            }

            if (action == "nginx")
            {
                EnterNginxDir();
                return;
            }

            if (action == "home")
            {
                OpenHomePage();
                return;
            }

            Signal(action);
        }

        public void ShowHelp()
        {
            var usage = $@"Nginx for Windows helper

Usage: {AppName} <command> [arguments]

Commands

    create <ServerName> [ServerRoot]
    start
    restart
    stop
    edit <ServerName | main | template | hosts>
    test [ServerName]

Command create

    ServerRoot, default value is the current directory.

    {AppName} create t.com .\public
    Create a virtual host and then restart Nginx.

Command edit

    The editor is Sublime Text, make sure subl.exe is in PATH.

    argument
    main          Edit the main conf of Nginx: {Cfg.NginxDir}\conf\nginx.conf
    template      Edit the conf template: {Cfg.ConfTemplate}
    hosts         Edit system hosts in notepad.exe

    {AppName} edit t.com
    Edit the conf of t.com 

Command test

    {AppName} test t.com
    Test conf by Nginx and also check whether the system hosts contains t.com     

Readme

    {HomePage}
";
            Console.Write(usage);
        }

        public void CreateVhost(string serverName, string serverRoot)
        {
            string template;
            using (var sr = new StreamReader(Cfg.ConfTemplate))
            {
                template = sr.ReadToEnd();
            }

            var data = new Dictionary<string, string>();

            var name = serverName;
            var serverNameValue = name;
            if (String.IsNullOrEmpty(name))
                throw new Exception("ServerName is null");
            // Add www sub domain for top level domain, e.g. a.com www.a.com
            // exclude localhost 
            var count = 0;
            foreach (var c in name)
                if (c == '.') count++;
            if (count == 1)
                serverNameValue = $"{name} www.{name}";
            data.Add("ServerName", serverNameValue);

            var root = Path.Combine(Environment.CurrentDirectory, serverRoot ?? "")
                .Replace('\\', '/');
            data.Add("ServerRoot", root);

            var conf = Regex.Replace(template, @"{{(\w+)}}", ReplaceWord);
            var confFile = GetConfFile(name);
            using (var sw = new StreamWriter(confFile, false))
            {
                sw.Write(conf);
            }

            Console.WriteLine($"{serverName} => {root}");

            // restart nginx after conf modified
            Console.WriteLine("restart Nginx...");
            Signal("restart");

            string ReplaceWord(Match match)
            {
                var key = match.Groups[1].Value;
                try
                {
                    return data[key];
                }
                catch (KeyNotFoundException)
                {
                    throw new AppException($"Cannot resolve variable `{key}` in the template \"{Cfg.ConfTemplate}\"");
                }
            }
        }

        private string GetConfFile(string name)
        {
            if (name == "main")
                return Path.Combine(Cfg.NginxDir, "conf/nginx.conf");
            if (name == "template")
                return Cfg.ConfTemplate;
            return Path.Combine(Cfg.ConfSaveDir, name + ".conf");
        }

        public void Signal(string action)
        {
            // nssm.exe should run as administrator
            var startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                // keep the console window to view nssm.exe message
                Arguments = $"/d /s /k \"nssm.exe {action} \"{Cfg.ServiceName}\"\"",
                Verb = "runas",
            };
            Process.Start(startInfo);
        }

        public void Edit(string name)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "subl.exe",
                Arguments = $"--new-window \"{Cfg.NginxDir}\" \"{GetConfFile(name)}\"",
                UseShellExecute = false,
            };
            Process.Start(startInfo);
        }

        public void EditHosts()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "notepad.exe",
                Arguments = Hosts,
                Verb = "runas"
            };
            Process.Start(startInfo);
        }

        public bool CheckHosts(string name)
        {
            using (var sw = new StreamReader(Hosts))
            {
                var sep = new[] { ' ', '\t' };
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line == "") continue;
                    var c = line[0];
                    if (c == '#') continue;
                    var parts = line.Split(sep);
                    foreach (var item in parts)
                    {
                        if (item == name) return true;
                    }
                }
            }

            return false;
        }

        public bool Prompt(string prompt)
        {
            Console.WriteLine(prompt + " [y/n]");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Y || key == ConsoleKey.Enter)
                return true;
            return false;
        }

        public void TestConf()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Cfg.NginxDir, "nginx.exe"),
                Arguments = "-t",
                WorkingDirectory = Cfg.NginxDir,
                UseShellExecute = false,
                RedirectStandardError = true,
            };
            var p = Process.Start(startInfo);
            Console.WriteLine(p.StandardError.ReadToEnd().TrimEnd('\r', '\n'));
            p.WaitForExit();
        }

        public void EnterNginxDir()
        {
            // Open a new console window
            var startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/d /s /k \"cd \"{Cfg.NginxDir}\"\"",
            };
            Process.Start(startInfo);
        }

        public void OpenHomePage()
        {
            Process.Start(HomePage);
        }
    }
}
