using System;
using System.Timers;
using System.Collections.Generic;
using OpcLabs.EasyOpc.UA.PubSub;
using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.OperationModel;
using opcua_to_prometheus.Models;
using OpcLabs.BaseLib.ComponentModel;
using System.Reflection;

namespace opcua_to_prometheus.Services
{
    public class PLCService : IDisposable
    {
        private readonly ConfigService configService;
        private EasyUAClient client;
        private int countTagsChanged;
        private Timer timer;
        public PLCService(ConfigService configService)
        {
            #region license
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            LicensingManagement.Instance.RegisterManagedResource("QuickOPC", "Multipurpose", Assembly.GetExecutingAssembly(), "replace me with license file");
            long serialNumber = (uint)new EasyUAClient().LicenseInfo["Multipurpose.SerialNumber"];
            if ((1111110000 <= serialNumber) && (serialNumber <= 1111119999))
            {
                Console.WriteLine("QuickOPC license did not get applied, this is a trial license. License is only valid for versions 5.58.451 and lower.");
                throw new ApplicationException("QuickOPC license did not get applied, this is a trial license. License is only valid for versions 5.58.451 and lower.");
            }
            else
            {
                Console.WriteLine($"Valid QuickOPC license with serial number: {serialNumber}");
            }

            EasyUAClient.SharedParameters.EngineParameters.CertificateAcceptancePolicy.AcceptAnyCertificate = true;
            #endregion

            Console.WriteLine("PLCService: starting");

            this.configService = configService;

            UAEndpointDescriptor endpointDescriptor = this.configService.ActiveConfig.OPCUAEndpoint;
            client = new EasyUAClient();
            client.DataChangeNotification += client_DataChangeNotification;

            List<EasyUAMonitoredItemArguments> easyUAMonitoredItemArguments = new List<EasyUAMonitoredItemArguments>();


            foreach (Tag tag in configService.ActiveConfig.Tags)
            {
                //take 100ms as default
                int subscriptionInterval = 100;

               
                if (configService.ActiveConfig.SubscriptionInterval != 0)
                {
                    subscriptionInterval = configService.ActiveConfig.SubscriptionInterval;
                }
                
                if (tag.SubscriptionInterval != 0)
                {
                    subscriptionInterval = tag.SubscriptionInterval;
                }

                easyUAMonitoredItemArguments.Add(new EasyUAMonitoredItemArguments(null, endpointDescriptor, tag.NodeID,
                       new UAMonitoringParameters(subscriptionInterval)));


            }

            client.SubscribeMultipleMonitoredItems(easyUAMonitoredItemArguments.ToArray());

            
            var subscriber = new EasyUASubscriber();

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"{countTagsChanged} tags per second");

            countTagsChanged = 0;
        }

        private void client_DataChangeNotification(object sender, EasyUADataChangeNotificationEventArgs e)
        { 

            if (e.Succeeded)
            {
                countTagsChanged++;
                /*Console.WriteLine("{0}: {1}", e.Arguments.NodeDescriptor, e.AttributeData.Value);*/
                Tag foundTag = configService.ActiveConfig.Tags.Find(tag => tag.NodeID == e.Arguments.NodeDescriptor.NodeId);
                if (foundTag != null)
                {
                    foundTag.Value = e.AttributeData.Value;
                }
            }
            else
                Console.WriteLine("{0} *** Failure: {1}", e.Arguments.NodeDescriptor, e.ErrorMessageBrief);
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.UnsubscribeAllMonitoredItems();
            }
        }
    }
}
