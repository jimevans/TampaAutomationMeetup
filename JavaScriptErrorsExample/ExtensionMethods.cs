﻿// <copyright file="ExtensionMethods.cs" company="Jim Evans">
// Copyright © 2018 Jim Evans
// Licensed under the MIT license, as found in the LICENSE file accompanying this source code.
// </copyright>

namespace JavaScriptErrorsExample
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Fiddler;
    using OpenQA.Selenium;

    /// <summary>
    /// A class of extension methods for a WebDriver instance.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// The default timeout for navigation.
        /// </summary>
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Navigates to a specified URL, injecting code to capture JavaScript errors on the page.
        /// </summary>
        /// <param name="driver">The driver used to navigate to the URL.</param>
        /// <param name="targetUrl">The URL to navigate to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the driver instance or URL is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "As a test sample project, specifying strings for URLs is okay.")]
        public static void NavigateTo(this IWebDriver driver, string targetUrl)
        {
            if (driver == null)
            {
                throw new ArgumentNullException("driver", "Driver cannot be null");
            }

            if (string.IsNullOrEmpty(targetUrl))
            {
                throw new ArgumentNullException("targetUrl", "URL cannot be null or empty string");
            }

            string errorScript = "window.__webdriver_javascript_errors = []; window.onerror = function(errorMsg, url, line) { window.__webdriver_javascript_errors.push(errorMsg + ' (found at ' + url + ', line ' + line + ')'); };";
            SessionStateHandler beforeRequestHandler = delegate(Session targetSession)
            {
                // Tell Fiddler to buffer the response so that we can modify
                // it before it gets back to the browser.
                targetSession.bBufferResponse = true;
            };

            SessionStateHandler beforeResponseHandler = delegate(Session targetSession)
            {
                if (targetSession.fullUrl == targetUrl &&
                    targetSession.oResponse.headers.ExistsAndContains("Content-Type", "html"))
                {
                    targetSession.utilDecodeResponse();
                    string responseBody = targetSession.GetResponseBodyAsString();
                    string headTag = Regex.Match(
                        responseBody,
                        "<head.*>",
                        RegexOptions.IgnoreCase).ToString();
                    string addition = headTag + "<script>" + errorScript + "</script>";
                    targetSession.utilReplaceOnceInResponse(headTag, addition, false);
                }
            };

            FiddlerApplication.BeforeRequest += beforeRequestHandler;
            FiddlerApplication.BeforeResponse += beforeResponseHandler;
            driver.Url = targetUrl;
            FiddlerApplication.BeforeResponse -= beforeResponseHandler;
            FiddlerApplication.BeforeRequest -= beforeRequestHandler;
        }

        /// <summary>
        /// Gets the JavaScript errors on the current page.
        /// </summary>
        /// <param name="driver">The driver used to retrieve the errors.</param>
        /// <returns>A list of all JavaScript errors captured on the page.</returns>
        public static IList<string> GetJavaScriptErrors(this IWebDriver driver)
        {
            return GetJavaScriptErrors(driver, DefaultTimeout);
        }

        /// <summary>
        /// Gets the JavaScript errors on the current page.
        /// </summary>
        /// <param name="driver">The driver used to retrieve the errors.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> structure for the time out of the retrieval.</param>
        /// <returns>A list of all JavaScript errors captured on the page.</returns>
        public static IList<string> GetJavaScriptErrors(this IWebDriver driver, TimeSpan timeout)
        {
            string errorRetrievalScript = "var errorList = window.__webdriver_javascript_errors; window.__webdriver_javascript_errors = []; return errorList;";
            DateTime endTime = DateTime.Now.Add(timeout);
            List<string> errorList = new List<string>();
            IJavaScriptExecutor executor = driver as IJavaScriptExecutor;
            ReadOnlyCollection<object> returnedList = executor.ExecuteScript(errorRetrievalScript) as ReadOnlyCollection<object>;
            while (returnedList == null && DateTime.Now < endTime)
            {
                System.Threading.Thread.Sleep(250);
                returnedList = executor.ExecuteScript(errorRetrievalScript) as ReadOnlyCollection<object>;
            }

            if (returnedList == null)
            {
                return null;
            }
            else
            {
                foreach (object returnedError in returnedList)
                {
                    errorList.Add(returnedError.ToString());
                }
            }

            return errorList;
        }
    }
}
