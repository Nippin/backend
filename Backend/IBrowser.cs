using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace Backend
{
    /// <summary>
    /// Represents SeleniumWebDriver instance.
    /// 
    /// Disposing the instance is very important because non-disposed instances will keep memory
    /// in selenium grid and will be disposed by the grid in some time later.
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
