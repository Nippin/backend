using OpenQA.Selenium;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    /// <summary>
    /// Represents SeleniumWebDriver instance.
    /// 
    /// To operate on browser instance in a general way, we need:
    /// * Initialize it to allow navbigate to home page and do required self-checks
    /// * Navigate to given url
    /// * try to interprete current page as TPage instance
    /// * Get browser screenshot
    /// * Dispose when we would liek to destroy remote browser instance.
    /// Disposing the instance is very important because non-disposed instances will keep memory
    /// in selenium grid and will be disposed by the grid in some time later.
    /// </summary>
    public interface IBrowser : IDisposable
    {
        /// <summary>
        /// Need to be invoked once to start browser's work.
        /// </summary>
        /// <returns></returns>
        void Initialize();

        /// <summary>
        /// Allows to navigate
        /// </summary>
        /// <param name="url">Navigation target</param>
        /// <returns>Task finished when browser has navigated to target location</returns>
        Task GoToUrl(string url);

        /// <summary>
        /// Creates given type of page, initializes it with nuderlying selenium driver and returns.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        /// <param name="deadline"></param>
        /// <returns></returns>
        Task<TPage> Expect<TPage>(CancellationToken deadline) where TPage : IPage, new();

        /// <summary>
        /// Returns current screenshot fro mthe browser instance.
        /// </summary>
        /// <returns>Screenshot</returns>
        Task<Screenshot> GetScreenshot();
    }
}
