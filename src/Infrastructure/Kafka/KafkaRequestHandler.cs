using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Clients.Base;
using OMP.Connector.Application.Services;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Device.Connector.Kafka
{
    public class KafkaRequestHandler : RequestHandler, IKafkaRequestHandler
    {
        public KafkaRequestHandler(
            ILogger<KafkaRequestHandler> logger,
            IMapper mapper,
            IMessageSender messageSender,
            ICommandService commandService,
            ISubscriptionServiceStateManager subscriptionServiceStateManager,
            IDiscoveryService discoveryService,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            CommandRequestValidator commandRequestValidator)
            : base(
                  logger,
                  messageSender,
                  mapper,
                  commandService,
                  subscriptionServiceStateManager,
                  discoveryService,
                  connectorConfiguration,
                  commandRequestValidator)
        {
        }

        public async Task OnMessageReceivedAsync(ConsumeResult<string, CommandRequest> consumeResult)
        {
            try
            {
                if (consumeResult.Message is null)
                {
                    Logger.LogWarning($"Received message was null. Topic: {consumeResult.Topic}, Partition: {consumeResult.Partition}, Offset: {consumeResult.Offset}");
                    return;
                }

                await base.OnMessageReceivedAsync(consumeResult.Message.Value);
            }
            catch (Exception e)
            {
                var error = e.Demystify();
                Logger.LogWarning($"Received message was not processed. Topic: {consumeResult.Topic}, Partition: {consumeResult.Partition}, Offset: {consumeResult.Offset}, Error: {error.Message}");
            }
        }

        public void OnMessageReceived(ConsumeResult<string, CommandRequest> consumeResult)
           => OnMessageReceivedAsync(consumeResult).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}