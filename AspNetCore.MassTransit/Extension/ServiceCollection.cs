using AspNetCore.MassTransit.Service;
using AspNetCore.MassTransit.Utility;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace AspNetCore.MassTransit.Extension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddClientMassTransitWithRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton<IBusControl>(context =>
            {
                RabbitMQConnector connector = context.GetService<RabbitMQConnector>();

                var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(RabbitMqUriBuilder.RabbitMQConnectionStringBuilder(connector.RabbitMQConnectionString)), h =>
                    {
                        h.Username(connector.Username);
                        h.Password(connector.Password);
                    });
                });
                return bus;
            });

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());

            return services;
        }

        public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton<IBusControl>(context =>
            {

                CompleteRabbitMqConnection connector = context.GetService<CompleteRabbitMqConnection>();

                var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(RabbitMqUriBuilder.RabbitMQConnectionStringBuilder(connector.RabbitMQConnectionString)), h =>
                    {
                        h.Username(connector.Username);
                        h.Password(connector.Password);
                    });

                    cfg.ReceiveEndpoint(host, connector.SubscribeTopics, e =>
                    {
                        e.LoadFrom(context);
                    });
                });

                return bus;
            });
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            services.AddHostedService<BusService>();
            return services;
        }

        public static IServiceCollection AddMassTransitRequestResponse<TRequest, TResponse>(this IServiceCollection services, string topicName) where TRequest : class where TResponse : class
        {
            RabbitMQConnector massTransitconnector = services.BuildServiceProvider().GetService<RabbitMQConnector>();
            var serviceAddress = new Uri(RabbitMqUriBuilder.RabbitMQConnectionStringBuilder(massTransitconnector.RabbitMQConnectionString, topicName));

            services.AddScoped<IRequestClient<TRequest, TResponse>>(x =>
              new MessageRequestClient<TRequest, TResponse>(x.GetRequiredService<IBus>(), serviceAddress, TimeSpan.FromSeconds(massTransitconnector.Timeout)));
            return services;
        }

        public static IServiceCollection AddMassTransitRequest<TRequest>(this IServiceCollection services, string queueName) where TRequest : class
        {
            services.AddSingleton<IMassTransitRequest<TRequest>>(provider =>
            {
                RabbitMQConnector connector = provider.GetService<RabbitMQConnector>();
                IBusControl bsuControl = provider.GetRequiredService<IBusControl>();
                return new MassTransitRequest<TRequest>(bsuControl, connector, queueName);
            });

            return services;
        }

    }
}
