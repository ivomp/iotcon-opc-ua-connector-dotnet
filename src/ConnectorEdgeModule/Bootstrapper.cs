﻿using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Factories;
using OMP.Connector.Application.Providers;
using OMP.Connector.Application.Providers.Subscription;
using OMP.Connector.Application.Services;
using OMP.Connector.Application.Validators;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Providers;
using OMP.Connector.EdgeModule.DI;
using OMP.Connector.EdgeModule.Jobs;
using OMP.Connector.EdgeModule.MapperProfiles;
using OMP.Connector.Infrastructure.OpcUa.ConfigBuilders;
using OMP.Connector.Infrastructure.OpcUa.Reconnect;
using OMP.Connector.Infrastructure.OpcUa.States;
using Quartz;

namespace OMP.Connector.EdgeModule
{
    public static class Bootstrapper
    {
        public static IServiceProvider Bootstrap(HostBuilderContext hostingContext, IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<ConnectorConfiguration>(hostingContext.Configuration);

            serviceCollection.AddSingleton<IConfigureOptions<ConnectorConfiguration>, ConfigureConnectorSettings>();

            serviceCollection.AddSingleton<IAppConfigBuilder, AppConfigBuilder>();
            serviceCollection.AddSingleton(provider =>
            {
                var builder = provider.GetService<IAppConfigBuilder>();
                return builder!.Build();
            });

            serviceCollection.AddSingleton<ISubscriptionServiceStateManager, SubscriptionServiceStateManager>();
            serviceCollection.AddSingleton<ISubscriptionProviderFactory, SubscriptionProviderFactory>();

            serviceCollection.AddSingleton<ISessionPoolStateManager, SessionPoolStateManager>();

            serviceCollection.AddTransient<CommandRequestValidator>();
            serviceCollection.AddTransient<MonitoredItemValidator>();
            serviceCollection.AddTransient<RoutingSettingsValidator>();

            //if (settings.KafkaBootstrapServers.Any())
            //{
            serviceCollection.AddKafkaIntegration();
            //}

            serviceCollection.AddSingleton(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EdgeProfile(provider.GetRequiredService<ILoggerFactory>()));
            }).CreateMapper());

            serviceCollection.AddTransient<IDiscoveryService, DiscoveryService>();
            serviceCollection.AddTransient<ICommandService, CommandService>();
            serviceCollection.AddTransient<ISubscriptionServiceFactory, SubscriptionServiceFactory>();
            serviceCollection.AddTransient<ISubscriptionRestoreService, SubscriptionRestoreService>();
            serviceCollection.AddSingleton<ICommandProviderFactory, CommandProviderFactory>();
            serviceCollection.AddTransient<IDiscoveryProvider, DiscoveryProvider>();
            serviceCollection.AddTransient<IOpcSessionReconnectHandlerFactory, OpcSessionReconnectHandlerFactory>();
            serviceCollection.AddTransient<IOpcMonitoredItemService, OpcMonitoredItemService>();

            serviceCollection.AddSingleton<MonitoredItemServiceInitializerFactory>();
            serviceCollection.RegisterDelegateFromService<MonitoredItemServiceInitializerFactory, MonitoredItemServiceInitializerFactoryDelegate>
                (factory => factory.Initialize);

            serviceCollection.AddTransient<ConfigRestoreService>();
            serviceCollection.AddTransient<ConfigurationRestoreJob>();

            serviceCollection.AddQuartz(q =>
            {
                q.SchedulerId = "Connector Scheduler Id";
                q.SchedulerName = "Connector Scheduler Id";
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.ScheduleJob<ConfigurationRestoreJob>(trigger =>
                        trigger
                            .StartAt(DateBuilder.EvenMinuteDate(DateTimeOffset.UtcNow.AddSeconds(15)))
                            .WithSimpleSchedule(x =>
                                x.WithInterval(TimeSpan.FromSeconds(30))
                                 .RepeatForever()
                             )
                            .WithDescription("Trigger for restoring/applying missing or un-applied configuration")
                            .WithIdentity(nameof(ConfigurationRestoreJob))
                    );
            });

            serviceCollection.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = false;
            });


            return serviceCollection.BuildServiceProvider();
        }
    }
}