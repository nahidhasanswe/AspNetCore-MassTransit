using AspNetCore.MassTransit.Extension;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.MassTransit
{
    public class MassTransitConfiguration : IMassTransitConfiguration
    {
        private readonly IServiceCollection _services;


        public MassTransitConfiguration(IServiceCollection services)
        {
            _services = services;
        }

        public void AddRequestMessage<T>(string queueName) where T : class
        {
            _services.AddMassTransitRequest<T>(queueName);
        }

        public void AddRequestResponse<TRequest, TResponse>(string queueName)
            where TRequest : class
            where TResponse : class
        {
            _services.AddMassTransitRequestResponse<TRequest, TRequest>(queueName);
        }

        public void SetupConnection(Action<RabbitMQConnector> connection , string SubscribeQueueName = null)
        {
            RabbitMQConnector connector = new RabbitMQConnector();
            connection?.Invoke(connector);

            if(string.IsNullOrEmpty(SubscribeQueueName))
            {
                _services.AddSingleton(connector);
                _services.AddClientMassTransitWithRabbitMQ();

            }else
            {
                CompleteRabbitMqConnection completeConnection = new CompleteRabbitMqConnection();

                connection?.Invoke(completeConnection);
                completeConnection.SubscribeTopics = SubscribeQueueName;

                _services.AddSingleton(completeConnection);

                _services.AddMassTransitWithRabbitMQ();
            }
        }

        void IMassTransitConfiguration.AddCosumers(Action<IServiceCollectionConfigurator> configure)
        {
            _services.AddMassTransit(configure);
        }
    }
}
