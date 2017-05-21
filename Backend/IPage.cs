using OpenQA.Selenium.Remote;
using System.Threading.Tasks;

namespace Backend
{
    public interface IPage
    {
        Task Initialize(RemoteWebDriver driver);
    }
}
