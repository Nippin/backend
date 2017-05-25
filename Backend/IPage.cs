using OpenQA.Selenium.Remote;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public interface IPage
    {
        /// <summary>
        /// First method invoked once before any other method on page.
        /// </summary>
        /// <param name="driver"></param>
        void Initialize(RemoteWebDriver driver);

        /// <summary>
        /// Identification of the current page.
        /// 
        /// The result can be 
        /// * completed (means page is identified)
        /// * exception (can't proceed)
        /// * cancelled (can't identify the page)
        /// 
        /// Method can't have side effects and can be invoked many times to check if the current browser
        /// content is the same as expected by the page.
        /// </summary>
        /// <param name="deadline">Decides how much time is to identify the page.</param>
        /// <returns></returns>
        Task Identified(CancellationToken deadline);
    }
}
