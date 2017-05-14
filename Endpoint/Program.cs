using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Endpoint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                // Open Owin application for the internet 
                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel
                .UseUrls("http://*:5000/")
                .Build();

            host.Run();

            Actors.ActorSystem.Terminate().Wait();
        }
    }
}
