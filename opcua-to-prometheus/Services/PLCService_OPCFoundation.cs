using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using opcua_to_prometheus.Models;
using opcua_to_prometheus.OPC_Foundation;

namespace opcua_to_prometheus.Services
{
    public class PLCService_OPCFoundation : IDisposable
    {
        private UAClient client;
        private readonly ConfigService configService;
        private bool clientConnected;

        public PLCService_OPCFoundation(ConfigService configService)
        {
            this.configService = configService;
            configService.ConfigChangedEvent += ConfigService_ConfigChangedEvent;
        }
        public async Task<PLCService_OPCFoundation> InitializeAsync()
        { 

            Console.WriteLine("PLC Service starting...");

            clientConnected = await ConnectingClient();

            if (clientConnected)
            {
                Console.Write("Connected");
                client.SubscribeToDataChanges(configService.ActiveConfig.Tags);
            }
            else
            {
                Console.WriteLine("Could not connect to server!");
            }
            return this;
        }

        public async Task<bool> ConnectingClient()
        {
            try
            {
                // Define the UA Client application
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationName = "OPCUA-to-prometheus";
                application.ApplicationType = ApplicationType.Client;

                // load the application configuration.
                application.ApplicationConfiguration = new ApplicationConfiguration();
                application.ApplicationConfiguration.SecurityConfiguration = new SecurityConfiguration();
                application.ApplicationConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;
                application.ApplicationConfiguration.ClientConfiguration = new ClientConfiguration();

                client = new UAClient(application.ApplicationConfiguration, null);
                client.ServerUrl = configService.ActiveConfig.OPCUAEndpoint;

                bool connected = await client.ConnectAsync();
                return connected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        private void ConfigService_ConfigChangedEvent(object sender, EventArgs e)
        {
            // Recreate subscriptions
            if (clientConnected)
            {
                client.SubscribeToDataChanges(configService.ActiveConfig.Tags);
            }
            else
            {
                Console.WriteLine("Not connected to the server");
            }

        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Disconnect();
            }
        }

    }

        

}
