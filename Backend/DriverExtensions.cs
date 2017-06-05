using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nippin
{
    public static class DriverExtensions
    {
        public static Task Repeat(this RemoteWebDriver driver, Func<bool> operation, CancellationToken deadline)
        {
            return Repeat(driver, operation, deadline, 10, 300);
        }

        public static Task Repeat(this RemoteWebDriver driver, Func<bool> operation, CancellationToken deadline, int repeats, int operationDelay)
        {
            var tcs = new TaskCompletionSource<object>();

            Task.Run(async () =>
            {
                try
                {
                    int tries = repeats;
                    while (--tries > 0)
                    {
                        if (deadline.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                            return;
                        }

                        await Task.Delay(operationDelay);

                        try
                        {
                            if (!operation()) continue;

                            tcs.SetResult(null);
                            break;
                        }
                        catch (StaleElementReferenceException)
                        {
                            continue;
                        }
                    }

                    if (tries == 0)
                    {
                        tcs.SetException(new PageUnavailableException());
                        return;
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }
            });

            return tcs.Task;

        }
    }
}
