﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using AspNetCore.MassTransit;
using Common;

namespace AspNetCore.Worker
{
    class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /// Dependency Injection Implementation Here Or Inject Servie Collection
            /// services.AddTransit<>();
            /// 

            services.AddAspNetCoreMassTransit(config =>
            {
                config.SetupConnection(connection =>
                {
                    connection.RabbitMQConnectionString = "18.223.252.79";
                    connection.Username = "admin";
                    connection.Password = "MousumI01011996";
                    connection.Timeout = 60;
                });

                config.AddRequestMessage<Request>("Test.Receiver");
            });

            // This is service provider for create instance of DI object
            ServiceProvider provider = services.BuildServiceProvider();

            IMassTransitRequest<Request> massTransitRequest = provider.GetService<IMassTransitRequest<Request>>();

            massTransitRequest.Send(new Request()
            {
                TestMessage = "This is test message"

            }).GetAwaiter().GetResult();

            // This is an example of how to create instance of DI object
            //objectName obj = provider.GetService<objectName>();

            // Now you will able to execute your desired process

        }
    }
}
