﻿// <copyright file="WebDriverFactory.cs" company="Jim Evans">
// Copyright © 2018 Jim Evans
// Licensed under the MIT license, as found in the LICENSE file accompanying this source code.
// </copyright>

namespace WebDriverProxyUtilities
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.IE;
    using OpenQA.Selenium.PhantomJS;
    using OpenQA.Selenium.Edge;

    /// <summary>
    /// A static factory class for creating WebDriver instances with proxies.
    /// </summary>
    public static class WebDriverFactory
    {
        /// <summary>
        /// Creates a WebDriver instance for the desired browser using the specified proxy settings.
        /// </summary>
        /// <param name="kind">The browser to launch.</param>
        /// <param name="proxy">The WebDriver Proxy object containing the proxy settings.</param>
        /// <returns>A WebDriver instance using the specified proxy settings.</returns>
        public static IWebDriver CreateWebDriverWithProxy(BrowserKind kind, Proxy proxy)
        {
            IWebDriver driver = null;
            switch (kind)
            {
                case BrowserKind.InternetExplorer:
                    driver = CreateInternetExplorerDriverWithProxy(proxy);
                    break;

                case BrowserKind.Firefox:
                    driver = CreateFirefoxDriverWithProxy(proxy);
                    break;

                case BrowserKind.Chrome:
                    driver = CreateChromeDriverWithProxy(proxy);
                    break;

                case BrowserKind.Edge:
                    driver = CreateEdgeDriverWithProxy(proxy);
                    break;

                default:
                    driver = CreatePhantomJSDriverWithProxy(proxy);
                    break;
            }

            return driver;
        }

        /// <summary>
        /// Creates an InternetExplorerDriver instance using the specified proxy settings.
        /// </summary>
        /// <param name="proxy">The WebDriver Proxy object containing the proxy settings.</param>
        /// <returns>An InternetExplorerDriver instance using the specified proxy settings</returns>
        private static IWebDriver CreateInternetExplorerDriverWithProxy(Proxy proxy)
        {
            InternetExplorerOptions options = new InternetExplorerOptions();
            options.Proxy = proxy;

            // Make IE not use the system proxy, and clear its cache before
            // launch. This makes the behavior of IE consistent with other
            // browsers' behavior.
            options.UsePerProcessProxy = true;
            options.EnsureCleanSession = true;

            IWebDriver driver = new InternetExplorerDriver(options);
            return driver;
        }

        /// <summary>
        /// Creates an FirefoxDriver instance using the specified proxy settings.
        /// </summary>
        /// <param name="proxy">The WebDriver Proxy object containing the proxy settings.</param>
        /// <returns>An FirefoxDriver instance using the specified proxy settings</returns>
        private static IWebDriver CreateFirefoxDriverWithProxy(Proxy proxy)
        {
            FirefoxOptions firefoxOptions = new FirefoxOptions();
            firefoxOptions.Proxy = proxy;
            firefoxOptions.BrowserExecutableLocation = @"C:\Program Files (x86)\Nightly\firefox.exe";

            IWebDriver driver = new FirefoxDriver(firefoxOptions);
            return driver;
        }

        /// <summary>
        /// Creates an ChromeDriver instance using the specified proxy settings.
        /// </summary>
        /// <param name="proxy">The WebDriver Proxy object containing the proxy settings.</param>
        /// <returns>An ChromeDriver instance using the specified proxy settings</returns>
        private static IWebDriver CreateChromeDriverWithProxy(Proxy proxy)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.Proxy = proxy;

            IWebDriver driver = new ChromeDriver(chromeOptions);
            return driver;
        }

        private static IWebDriver CreateEdgeDriverWithProxy(Proxy proxy)
        {
            EdgeOptions edgeOptions = new EdgeOptions();

            IWebDriver driver = new EdgeDriver(edgeOptions);
            return driver;
        }

        /// <summary>
        /// Creates a PhantomJSDriver instance using the specified proxy settings.
        /// </summary>
        /// <param name="proxy">The WebDriver Proxy object containing the proxy settings.</param>
        /// <returns>An InternetExplorerDriver instance using the specified proxy settings</returns>
        private static IWebDriver CreatePhantomJSDriverWithProxy(Proxy proxy)
        {
            // This is an egregiously inconsistent API. Expect this to change
            // so that an actual Proxy object can be passed in.
            PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
            service.ProxyType = "http";
            service.Proxy = proxy.HttpProxy;

            IWebDriver driver = new PhantomJSDriver(service);
            return driver;
        }
    }
}
