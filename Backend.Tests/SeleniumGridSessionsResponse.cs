using System.Collections.Generic;

namespace Backend
{
    /// <summary>
    /// Classes are generated from JSON response from http://localhost:4444/wd/hub/sessions
    /// </summary>
    public sealed class SeleniumGridSessionsResponse
    {
        public string state { get; set; }
        public object sessionId { get; set; }
        public int hCode { get; set; }
        public List<Value> value { get; set; }
        public string @class { get; set; }
        public int status { get; set; }

        public class Chrome
        {
            public string chromedriverVersion { get; set; }
            public string userDataDir { get; set; }
        }

        public class Capabilities
        {
            public bool applicationCacheEnabled { get; set; }
            public bool rotatable { get; set; }
            public bool mobileEmulationEnabled { get; set; }
            public bool networkConnectionEnabled { get; set; }
            public Chrome chrome { get; set; }
            public bool takesHeapSnapshot { get; set; }
            public string pageLoadStrategy { get; set; }
            public string unhandledPromptBehavior { get; set; }
            public bool databaseEnabled { get; set; }
            public bool handlesAlerts { get; set; }
            public bool hasTouchScreen { get; set; }
            public string version { get; set; }
            public string platform { get; set; }
            public bool browserConnectionEnabled { get; set; }
            public bool nativeEvents { get; set; }
            public bool acceptSslCerts { get; set; }
            public bool locationContextEnabled { get; set; }
            public bool webStorageEnabled { get; set; }
            public string browserName { get; set; }
            public bool takesScreenshot { get; set; }
            public bool javascriptEnabled { get; set; }
            public bool cssSelectorsEnabled { get; set; }
            public string unexpectedAlertBehaviour { get; set; }
        }

        public class Value
        {
            public Capabilities capabilities { get; set; }
            public string id { get; set; }
            public int hCode { get; set; }
            public string @class { get; set; }
        }
    }
}
