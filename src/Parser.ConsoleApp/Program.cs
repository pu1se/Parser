using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using Newtonsoft.Json;
using Parser.BL;
using static System.Console;
using PuppeteerSharp;
using HtmlAgilityPack;

namespace Parser.ConsoleApp
{
    class Program
    {
        private static LaunchOptions PuppeterSharpParserConfig = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
            
        };

        public static async Task<string> ParseEmailAsync()
        {
            var url = "https://companies.devby.io/instinctools";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var emailNode = htmlDocument.DocumentNode.Descendants("a")
                .FirstOrDefault(a => a.Attributes["href"]?.Value?.StartsWith("mailto:") == true);

            var email = emailNode?.InnerHtml;
            return email;
        }

        static async Task Main(string[] args)
        {
            await CompaniesDevBy.RunLogic();

            // var emails = await ParseEmailAsync();

            /*var url = @"https://companies.devby.io/10clouds";

            using (var browser = await Puppeteer.LaunchAsync(PuppeterSharpParserConfig))
            {
                var page = await browser.NewPageAsync();

                await page.GoToAsync(url, new NavigationOptions
                {
                    WaitUntil = new[]
                    {
                        WaitUntilNavigation.Networkidle0,
                    }
                });

                await page.WaitForExpressionAsync(@"$('.sidebar-views-contacts ul li:eq(0) span').html() != '⋯'");

                var browserContent = await page.GetContentAsync();
                var parser = new HtmlParser();
                var document = parser.ParseDocument(browserContent);

                var contacts = document.QuerySelectorAll(".sidebar-views-contacts ul li").ToList();

            }*/
        }
        
    }
    
}
