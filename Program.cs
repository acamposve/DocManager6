using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using System;
using System.Diagnostics;
using System.IO;
using WebApi.Interfaces;

namespace WebApi
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

        private readonly ILoggerManager _logger;
        public Program(ILoggerManager logger)
        {
            _logger = logger;
        }
        public static void Main(string[] args)
        {

            try
            {
               BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {

                throw ex.InnerException;
            }
 

        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .UseConfiguration(Configuration)

                   .Build();
    }
}
