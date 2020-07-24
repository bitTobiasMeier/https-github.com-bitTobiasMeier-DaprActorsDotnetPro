namespace OrderActor
{
    using System;
    using Dapr.Actors.AspNetCore;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        private const int AppChannelHttpPort = 5300;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseActors(actorRuntime =>
                {
                    // Demo settings
                    actorRuntime.ConfigureActorSettings(a =>
                    {
                        a.ActorIdleTimeout = TimeSpan.FromMinutes(70);
                        a.ActorScanInterval = TimeSpan.FromSeconds(35);
                        a.DrainOngoingCallTimeout = TimeSpan.FromSeconds(35);
                        a.DrainRebalancedActors = true;
                    });
                    actorRuntime.RegisterActor<OrderActor>();
                    actorRuntime.RegisterActor<CustomerActor>();
                }).UseUrls($"http://localhost:{AppChannelHttpPort}/");
    }
}
