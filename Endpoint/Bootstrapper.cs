using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endpoint
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public Bootstrapper() { }

        public Bootstrapper(AppConfiguration appConfig)
        {
            Console.WriteLine("Bootstrapping...");
        }

        public override void Configure(Nancy.Configuration.INancyEnvironment environment)
        {
            environment.Tracing(enabled: false, displayErrorTraces: true);
        }
    }
}
