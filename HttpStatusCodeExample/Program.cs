﻿// <copyright file="Program.cs" company="Jim Evans">
// Copyright © 2018 Jim Evans
// Licensed under the MIT license, as found in the LICENSE file accompanying this source code.
// </copyright>

namespace HttpStatusCodeExample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Fiddler;
    using OpenQA.Selenium;
    using WebDriverProxyUtilities;

    /// <summary>
    /// Class containing the main application entry point and supporting methods.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args">Command line arguments of the application.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args", Justification = "Allowing main method to have arguments.")]
        internal static void Main(string[] args)
        {
            // Note that we're using a port of 0, which tells Fiddler to
            // select a random available port to listen on.
            int proxyPort = StartFiddlerProxy(0);

            // We are only proxying HTTP traffic, but could just as easily
            // proxy HTTPS or FTP traffic.
            OpenQA.Selenium.Proxy proxy = new OpenQA.Selenium.Proxy();
            proxy.HttpProxy = string.Format("127.0.0.1:{0}", proxyPort);

            // See the code of the individual methods for the details of how
            // to create the driver instance with the proxy settings properly set.
            BrowserKind browser = BrowserKind.Chrome;
            //BrowserKind browser = BrowserKind.Firefox;
            //BrowserKind browser = BrowserKind.IE;
            //BrowserKind browser = BrowserKind.Edge;
            //BrowserKind browser = BrowserKind.PhantomJS;

            IWebDriver driver = WebDriverFactory.CreateWebDriverWithProxy(browser, proxy);

            TestStatusCodes(driver);

            driver.Quit();

            StopFiddlerProxy();
            Console.WriteLine("Complete! Press <Enter> to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Starts the Fiddler proxy using the desired port.
        /// </summary>
        /// <param name="desiredPort">The port on which the proxy is to listen.
        /// Use zero (0) to have Fiddler select a port.</param>
        /// <returns>The port on which the Fiddler proxy is listening.</returns>
        private static int StartFiddlerProxy(int desiredPort)
        {
            // We explicitly do *NOT* want to register this running Fiddler
            // instance as the system proxy. This lets us keep isolation.
            Console.WriteLine("Starting Fiddler proxy");
            FiddlerCoreStartupFlags flags = FiddlerCoreStartupFlags.Default & ~FiddlerCoreStartupFlags.RegisterAsSystemProxy;
            FiddlerApplication.Startup(desiredPort, flags);
            int proxyPort = FiddlerApplication.oProxy.ListenPort;
            Console.WriteLine("Fiddler proxy listening on port {0}", proxyPort);
            return proxyPort;
        }

        /// <summary>
        /// Stops the Fiddler proxy.
        /// </summary>
        private static void StopFiddlerProxy()
        {
            Console.WriteLine("Shutting down Fiddler proxy");
            FiddlerApplication.Shutdown();
        }

        /// <summary>
        /// Executes the code to test retrieval of HTTP status codes from the browser.
        /// </summary>
        /// <param name="driver">The driver to use with the browser.</param>
        private static void TestStatusCodes(IWebDriver driver)
        {
            // Using Dave Haeffner's the-internet project http://github.com/arrgyle/the-internet,
            // which provides pages that return various HTTP status codes.
            string url = "http://the-internet.herokuapp.com/redirect";
            Console.WriteLine("Navigating to {0}", url);
            int responseCode = driver.NavigateTo(url);
            Console.WriteLine("Navigation to {0} returned response code {1}", url, responseCode);

            // Demonstrates navigating to a 404 page.
            url = "http://the-internet.herokuapp.com/redirector";
            Console.WriteLine("Navigating to {0}", url);
            responseCode = driver.NavigateTo(url);
            Console.WriteLine("Navigation to {0} returned response code {1}", url, responseCode);
            string elementId = "redirect";
            Console.WriteLine("Clicking on element with ID {0}", elementId);
            IWebElement element = driver.FindElement(By.Id(elementId));
            responseCode = element.ClickNavigate();
            Console.WriteLine("Element click returned response code {0}", responseCode);

            // Demonstrates navigating to a 404 page.
            url = "http://the-internet.herokuapp.com/status_codes/404";
            Console.WriteLine("Navigating to {0}", url);
            responseCode = driver.NavigateTo(url);
            Console.WriteLine("Navigation to {0} returned response code {1}", url, responseCode);
        }
    }
}
