using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using opcua_to_prometheus.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace opcua_to_prometheus.Services
{
    public class ConfigService
    {
        //Reads configuration file and watches it for changes

        public Configuration ActiveConfig { get; set; }
        private FileSystemWatcher watcher;

        public ConfigService()
        {
            ReadConfiguration();
            StartFileWatch();
            //DebugAddVariables();
            //dfg
        }

        private void DebugAddVariables()
        {
            Configuration readConfig = new Configuration();
            readConfig.OPCUAEndpoint = @"opc.tcp://localhost:4897/Softing/dataFEED/Server";
            readConfig.SubscriptionInterval = 1000;
            readConfig.Tags = new List<Tag>();

            Tag tag = new Tag();
            tag.MetricsName = $"Metric{0}";
            tag.NodeID = $"ns=4;s=DB.DB_5.Timer_10ms";
            readConfig.Tags.Add(tag);
            

            ActiveConfig = readConfig;
        }

        public void ReadConfiguration()
        {
            Console.WriteLine($"Reading config file...");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            Configuration readConfig = null;
            try
            {
                using (var sr = new StreamReader("../config.yml"))
                {
                    readConfig = deserializer.Deserialize<Configuration>(sr);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The config file could not be read");
                Console.WriteLine(e.Message);
            }

            Console.WriteLine($"Configuration file read succesfully");

            ActiveConfig = readConfig;
        }

        private void StartFileWatch()
        {
            watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), "config.yml");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnChanged;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine($"Current directory {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Watching config file for changes.");
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"Config file changed, rereading.");
            ReadConfiguration();
        }

    }
}
