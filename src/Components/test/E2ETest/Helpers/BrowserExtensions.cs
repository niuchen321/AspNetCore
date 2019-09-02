using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.Components.E2ETest
{
    internal static class BrowserExtensions
    {
        public static void Navigate(this IWebDriver browser, Uri baseUri, string relativeUrl, bool noReload)
        {
            var absoluteUrl = new Uri(baseUri, relativeUrl);

            if (noReload)
            {
                var existingUrl = browser.Url;
                if (string.Equals(existingUrl, absoluteUrl.AbsoluteUri, StringComparison.Ordinal))
                {
                    return;
                }
            }

            browser.Navigate().GoToUrl("about:blank");
            browser.Navigate().GoToUrl(absoluteUrl);
        }

        public static IWebElement WaitUntilExists(this IWebDriver browser, By findBy, int timeoutSeconds = 10, bool throwOnError = false)
        {
            IReadOnlyList<LogEntry> errors = null;
            IWebElement result = null;
            new WebDriverWait(browser, TimeSpan.FromSeconds(timeoutSeconds)).Until(driver =>
            {
                if (throwOnError && browser.Manage().Logs.AvailableLogTypes.Contains(LogType.Browser))
                {
                    // Fail-fast if any errors were logged to the console.
                    errors = browser.GetBrowserLogs(LogLevel.Severe);
                    if (errors.Count > 0)
                    {
                        return true;
                    }
                }

                return (result = driver.FindElement(findBy)) != null;
            });

            if (errors?.Count > 0)
            {
                var message =
                    $"Encountered errors while looking for '{findBy}'." + Environment.NewLine +
                    string.Join(Environment.NewLine, errors);
                throw new XunitException(message);
            }

            return result;
        }
    }
}
