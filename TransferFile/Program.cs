using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace TransferFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(builder.Build())
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .CreateLogger();

            Log.Logger.Information("Esperando archivo .DAT para traslado");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IMonitorFile, MonitorFile>();
                })
                .UseSerilog()
                .Build();


            var svc = ActivatorUtilities.CreateInstance<MonitorFile>(host.Services);
            svc.Run();
        }

        //crear una conexion manual al appsettings
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();

        }

    }
}
