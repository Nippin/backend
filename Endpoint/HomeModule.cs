﻿using Nancy;

namespace Endpoint
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get("/", args =>
            {
                System.Console.WriteLine("Visit: / on " + System.Environment.MachineName);
                return "Aby sprawdzić status płatnika VAT użyj funkcji excela: WEBSERVICE(\"http://nippin.cloudapp.net/excel/5213017228/with/description\") gdzie 5213017228 jest przykładowym szukanym numerem NIP.";
            });
        }
    }
}
