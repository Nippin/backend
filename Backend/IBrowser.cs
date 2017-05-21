using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace Backend
{
    /// <summary>
    /// Represents SeleniumWebDriver instance
    /// </summary>
    public interface IBrowser : IDisposable
    {
        /// <summary>
        /// Need to be invoked once to start browser's work.
        /// </summary>
        /// <returns></returns>
        Task Initialize();

        Task GoToUrl(string url);

        Task<TPage> Expect<TPage>() where TPage : IPage, new();

        Task<Screenshot> GetScreenshot();
    }
}
