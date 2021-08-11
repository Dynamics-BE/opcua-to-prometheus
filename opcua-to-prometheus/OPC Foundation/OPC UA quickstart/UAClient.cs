/* ========================================================================
 * Copyright (c) 2005-2020 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using opcua_to_prometheus.Models;

namespace opcua_to_prometheus.OPC_Foundation
{
    public class UAClient
    {
        private Subscription _subscription;
        /// <summary>
        /// OPC UA Client with examples of basic functionality.
        /// </summary>
        /// 
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the UAClient class.
        /// </summary>
        public UAClient(ApplicationConfiguration configuration, Action<IList, IList> validateResponse)
        {
            m_configuration = configuration;
            //m_configuration.CertificateValidator.CertificateValidation += CertificateValidation;
            //m_validateResponse = validateResponse;

        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the client session.
        /// </summary>
        public Session Session => m_session;

        /// <summary>
        /// Gets or sets the server URL.
        /// </summary>
        public string ServerUrl { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a session with the UA server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (m_session != null && m_session.Connected == true)
                {
                    Console.WriteLine("Session already connected!");
                }
                else
                {
                    Console.WriteLine("Connecting...");

                    // Get the endpoint by connecting to server's discovery endpoint.
                    // Try to find the first endopint without security.
                    EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(ServerUrl, false);

                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);
                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    // Create the session
                    Session session = await Session.Create(
                        m_configuration,
                        endpoint,
                        false,
                        false,
                        m_configuration.ApplicationName,
                        30 * 60 * 1000,
                        new UserIdentity(),
                        null
                    );

                    // Assign the created session
                    if (session != null && session.Connected)
                    {
                        m_session = session;
                    }

                    // Session created successfully.
                    Console.WriteLine($"New Session Created with SessionName = {m_session.SessionName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log Error
                Console.WriteLine($"Create Session Error : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects the session.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (m_session != null)
                {
                    Console.WriteLine("Disconnecting...");

                    m_session.Close();
                    m_session.Dispose();
                    m_session = null;

                    // Log Session Disconnected event
                    Console.WriteLine("Session Disconnected.");
                }
                else
                {
                    Console.WriteLine("Session not created!");
                }
            }
            catch (Exception ex)
            {
                // Log Error
                Console.WriteLine($"Disconnect Error : {ex.Message}");
            }
        }


        /// <summary>
        /// Create Subscription and MonitoredItems for DataChanges
        /// </summary>
        public void SubscribeToDataChanges(List<Tag> tags) //List<MonitoredItem> monitoredItems
        {
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return;
            }

            try
            {
                // Create a subscription for receiving data change notifications


                if (m_session.Subscriptions.Contains(_subscription))
                {
                    m_session.RemoveSubscription(_subscription);
                }

                // Define Subscription parameters
                _subscription = new Subscription(m_session.DefaultSubscription);

                _subscription.DisplayName = "Console ReferenceClient Subscription";
                _subscription.PublishingEnabled = true;
                _subscription.PublishingInterval = tags.Min(t => t.SubscriptionInterval) ;


                m_session.AddSubscription(_subscription);


                // Create the subscription on Server side
                _subscription.Create();
                Console.WriteLine("New Subscription created with SubscriptionId = {0}.", _subscription.Id);

                // Create MonitoredItems for data changes
                foreach (Tag tag in tags)
                {
                    MonitoredItem TempItem = new MonitoredItem(_subscription.DefaultItem);

                    TempItem.StartNodeId = new NodeId(tag.NodeID);
                    TempItem.AttributeId = Attributes.Value;
                    TempItem.DisplayName = tag.NodeID;
                    TempItem.SamplingInterval = 1000;
                    TempItem.Notification += OnMonitoredItemNotification;

                    _subscription.AddItem(TempItem);
                }

                // Create the monitored items on Server side
                _subscription.ApplyChanges();
                
                Console.WriteLine("MonitoredItems created for SubscriptionId = {0}.", _subscription.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Subscribe error: {0}", ex.Message);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Handle DataChange notifications from Server
        /// </summary>
        private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                // Log MonitoredItem Notification event
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                Console.WriteLine("Notification Received for Variable \"{0}\" and Value = {1}.", monitoredItem.DisplayName, notification.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Handles the certificate validation event.
        /// This event is triggered every time an untrusted certificate is received from the server.
        /// </summary>
        private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            bool certificateAccepted = true;

            // ****
            // Implement a custom logic to decide if the certificate should be
            // accepted or not and set certificateAccepted flag accordingly.
            // The certificate can be retrieved from the e.Certificate field
            // ***

            ServiceResult error = e.Error;
            while (error != null)
            {
                Console.WriteLine(error);
                error = error.InnerResult;
            }

            if (certificateAccepted)
            {
                Console.WriteLine("Untrusted Certificate accepted. SubjectName = {0}", e.Certificate.SubjectName);
            }

            e.AcceptAll = certificateAccepted;
        }
        #endregion

        #region Private Fields

        private ApplicationConfiguration m_configuration;

        private Session m_session;

        private readonly Action<IList, IList> m_validateResponse;

        #endregion
    }
}
