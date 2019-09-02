using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace TestServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var createIndividualHosts = new Dictionary<string, (IWebHost host, string basePath)>
            {
                ["Client authentication"] = (BuildWebHost<AuthenticationStartup>(CreateAdditionalArgs(args)), "/subdir"),
                ["Server authentication"] = (BuildWebHost<ServerAuthenticationStartup>(CreateAdditionalArgs(args)), "/subdir"),
                ["CORS (WASM)"] = (BuildWebHost<CorsStartup>(CreateAdditionalArgs(args)), "/subdir"),
                ["Prerendering (Server-side)"] = (BuildWebHost<PrerenderedStartup>(CreateAdditionalArgs(args)), "/prerendered"),
                ["Globalization + Localization (Server-side)"] = (BuildWebHost<InternationalizationStartup>(CreateAdditionalArgs(args)), "/subdir"),
                ["Server-side blazor"] = (BuildWebHost<ServerStartup>(CreateAdditionalArgs(args)), "/subdir"),
                ["Hosted client-side blazor"] = (BuildWebHost<ClientStartup>(CreateAdditionalArgs(args)), "/subdir")
            };

            var mainHost = BuildWebHost(args);

            Task.WhenAll(createIndividualHosts.Select(s => s.Value.host.StartAsync())).GetAwaiter().GetResult();

            var testAppInfo = mainHost.Services.GetRequiredService<TestAppInfo>();
            testAppInfo.Scenarios = createIndividualHosts
                .ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value.host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault()
                    .Replace("127.0.0.1", "localhost") + kvp.Value.basePath);

            mainHost.Run();
        }

        private static string[] CreateAdditionalArgs(string[] args) =>
            args.Concat(new[] { "--urls", "http://127.0.0.1:0" }).ToArray();

        public static IWebHost BuildWebHost(string[] args) => BuildWebHost<Startup>(args);

        public static IWebHost BuildWebHost<TStartup>(string[] args) where TStartup : class =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((ctx, lb) =>
                {
                    TestSink sink = new TestSink();
                    lb.AddProvider(new TestLoggerProvider(sink));
                    lb.Services.Add(ServiceDescriptor.Singleton(sink));
                })
                .UseConfiguration(new ConfigurationBuilder()
                        .AddCommandLine(args)
                        .Build())
                .UseStartup<TStartup>()
                .UseStaticWebAssets()
                .Build();
    }
}
