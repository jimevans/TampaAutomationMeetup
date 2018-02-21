using Fiddler;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDriverProxyUtilities;

namespace BasicAuthExample
{
    class Program
    {
        static void Main(string[] args)
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

            TestBasicAuth(driver);

            driver.Quit();

            StopFiddlerProxy();
            Console.WriteLine("Complete! Press <Enter> to exit.");
            Console.ReadLine();
        }

        private static void TestBasicAuth(IWebDriver driver)
        {
            // Using Dave Haeffner's the-internet project http://github.com/arrgyle/the-internet,
            // which provides pages that return various HTTP status codes.
            string url = "http://the-internet.herokuapp.com/basic_auth";
            Console.WriteLine("Navigating to {0}", url);
            NavigateTo(driver, url, "admin", "admin", TimeSpan.FromSeconds(10), true);
            Console.WriteLine("Navigation to {0} returned response code {1}", url, "");
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
        /// Navigates to a specified URL, returning the HTTP status code of the navigation.
        /// </summary>
        /// <param name="driver">The driver used to navigate to the URL.</param>
        /// <param name="targetUrl">The URL to navigate to.</param>
        /// <param name="userName">The user name with which to login.</param>
        /// <param name="password">The password with which to login.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> structure for the time out of the navigation.</param>
        /// <param name="printDebugInfo"><see langword="true"/> to print debugging information to the console;
        /// otherwise, <see langword="false"/>.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "As a test sample project, specifying strings for URLs is okay.")]
        public static void NavigateTo(IWebDriver driver, string targetUrl, string userName, string password, TimeSpan timeout, bool printDebugInfo)
        {
            if (driver == null)
            {
                throw new ArgumentNullException("driver", "Driver cannot be null");
            }

            if (string.IsNullOrEmpty(targetUrl))
            {
                throw new ArgumentException("URL cannot be null or the empty string.", "targetUrl");
            }

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("User name cannot be null or the empty string.", "userName");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or the empty string.", "password");
            }

            DateTime endTime = DateTime.Now.Add(timeout);
            SessionStateHandler responseHandler = delegate (Session targetSession)
            {
                if (printDebugInfo)
                {
                    Console.WriteLine("DEBUG: Received response for resource with URL {0}", targetSession.fullUrl);
                }

                byte[] credentialsArray = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userName, password));
                string encodedCredentials = Convert.ToBase64String(credentialsArray);
                targetSession.oRequest.headers["Authorization"] = string.Format("Basic {0}", encodedCredentials);
            };

            // Attach the event handler, perform the navigation, and wait for
            // the status code to be non-zero, or to timeout. Then detach the
            // event handler and return the response code.
            FiddlerApplication.BeforeRequest += responseHandler;
            driver.Url = targetUrl;
            FiddlerApplication.BeforeRequest -= responseHandler;
        }
    }
}
