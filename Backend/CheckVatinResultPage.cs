using Nippin;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static Backend.PageActor;

namespace Backend
{
    public sealed class CheckVatinResultPage : IPage
    {
        public Action Clear { get; private set; }
        public CheckVatinReply.VatinPayerStatus Status { get; private set; }

        private RemoteWebDriver driver;

        public void Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;
            Clear = OnClear;
        }

        /// <summary>
        /// The page contains 
        /// * button titled Wyczyść
        /// * {span} starting with 'Data sprawdzenia:'
        /// </summary>
        /// <param name="deadline">Maksimum waiting time for page characteristic elements.</param>
        /// <returns>Task completed if elements have been found.</returns>
        public Task Identified(CancellationToken deadline)
        {
            var clearElementExists = driver.Repeat(() =>
            {
                // button 'Wyczyść'
                var element = driver.FindElements(By.Id("b-9")).FirstOrDefault();
                if (element == null) return false;
                if (!element.Displayed) return false;

                return true;
            }, deadline);

            var checkingDateExists = driver.Repeat(() =>
            {
                var element = driver.FindElements(By.Id("caption2_b-b")).FirstOrDefault();
                if (element == null) return false;
                if (!element.Displayed) return false;
                var content = element.Text;
                if (!content.StartsWith("Data sprawdzenia:")) return false;

                return true;
            }, deadline);

            var descriptionExists = driver.Repeat(() =>
            {
                var element = driver.FindElements(By.Id("caption2_b-3")).FirstOrDefault();
                if (element == null) return false;
                if (!element.Displayed) return false;
                var content = element.Text;
                Status = content.StartsWith("Podmiot o podanym identyfikatorze podatkowym NIP jest zarejestrowany jako podatnik VAT czynny")
                    ? CheckVatinReply.VatinPayerStatus.IsTaxPayer
                    : CheckVatinReply.VatinPayerStatus.Unknown;

                return true;
            }, deadline);


            return Task.WhenAll(clearElementExists, checkingDateExists, descriptionExists);
        }

        public string Print()
        {
            return driver.GetScreenshot().AsBase64EncodedString;
        }

        private void OnClear()
        {
            repeat:
            try
            {
                var element = default(IWebElement);
                var now = DateTime.Now;
                while (DateTime.Now - now < TimeSpan.FromSeconds(10))
                {
                    // try to recognize page base on string: Data sprawdzenia:
                    var found = driver.FindElements(By.Id("b-9")).FirstOrDefault();
                    if (found == null) continue;
                    if (!found.Displayed) continue;

                    element = found;
                    break;
                }

                if (element == null) throw new NoSuchElementException();

                element.Click();
            }
            catch (StaleElementReferenceException repeat)
            {
                goto repeat;
            }

        }
    }
}
