using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Microsoft.AspNetCore.Components.E2ETest
{
    internal static class AppHelpers
    {
        public static IWebElement MountTestComponent<TComponent>(this IWebDriver browser) where TComponent : IComponent
        {
            var componentTypeName = typeof(TComponent).FullName;
            var testSelector = browser.WaitUntilTestSelectorReady();
            testSelector.SelectByValue("none");
            testSelector.SelectByValue(componentTypeName);
            return browser.FindElement(By.TagName("app"));
        }

        public static SelectElement WaitUntilTestSelectorReady(this IWebDriver browser)
        {
            var elemToFind = By.CssSelector("#test-selector > select");
            browser.WaitUntilExists(elemToFind, timeoutSeconds: 30, throwOnError: true);
            return new SelectElement(browser.FindElement(elemToFind));
        }
    }
}
