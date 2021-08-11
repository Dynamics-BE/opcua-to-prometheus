using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
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
        public event EventHandler ConfigChangedEvent;

        public ConfigService()
        {
            ReadConfiguration();
            StartFileWatch();
            //DebugAddVariables();
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
            int i = 1;
            while (i < 5){
              
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
                if (readConfig != null)
                {
                    Console.WriteLine($"Configuration file read succesfully");
                    ActiveConfig = readConfig;
                    i = 5;
                }
                else
                {
                    Console.WriteLine($"Retrying to read the config file: attempt {i}");
                }

                Thread.Sleep(1000);
                i++;
            }
        }

        private void StartFileWatch()
        {
            watcher = new FileSystemWatcher(Path.Combine(Directory.GetCurrentDirectory(), @"..\"), "config.yml");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnChanged;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine($"Current directory {Path.Combine(Directory.GetCurrentDirectory(), @"..\")}");
            Console.WriteLine($"Watching config file for changes.");
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"Config file changed, rereading. {e.ChangeType}");
            ReadConfiguration();
            ConfigChangedEvent(this, new EventArgs());
        }
    }
}
