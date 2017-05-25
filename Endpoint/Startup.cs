using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nancy.Owin;
using Nippin;

namespace Endpoint
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var config = this.Configuration;
            var appConfig = new AppConfiguration();
            ConfigurationBinder.Bind(config, appConfig);

            app.UseOwin(x => x.UseNancy(new NancyOptions { Bootstrapper = new AppBootstrapper(lifetime.ApplicationStopped, 1, 1)}));
        }
    }
}
