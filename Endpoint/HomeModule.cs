using Nancy;

namespace Endpoint
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get("/", args =>
            {
                System.Console.WriteLine("Visit: / on " + System.Environment.MachineName);
                return "Run excel and check ZUS status with function WEBSERVICE(\"http://nippin.cloudapp.net/excel/5213017228/with/description\")";
            });
        }
    }
}
