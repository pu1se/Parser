using AngleSharp.Io;
using AngleSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using Parser.BL.Entities;
using Parser.ConsoleApp;
using PuppeteerSharp;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using SeleniumExtras.WaitHelpers;

namespace Parser.BL
{
    public  static class CompaniesDevBy
    {
        const string BaseUrl = "https://companies.devby.io";
        const string FileName = "companies.json";
        static IConfiguration AngleSharpParserConfig = AngleSharp.Configuration.Default.WithDefaultLoader(new LoaderOptions{IsNavigationDisabled = false, IsResourceLoadingEnabled = true}).WithCss().WithDefaultCookies().WithJs().WithRenderDevice();
        private static Storage Storage = new Storage();
        private static LaunchOptions PuppeterSharpParserConfig = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
            
        };

        public static async Task RunLogic()
        {
            Console.ForegroundColor = ConsoleColor.Green;

            var companies = await FillOrganizations();
            var counter = 0;

            foreach (var organization in companies.Where(x => x.Email == null).Skip(10))
            {
                Console.WriteLine(++counter + " company");
                await FillAdditionalInformationAboutCompany(organization);
                await Storage.SaveChangesAsync();
            }

            // await FillContactInformation();
        }

        static async Task FillAdditionalInformationAboutCompany(OrganizationEntity organization)
        {
            var url = BaseUrl + organization.SubLink;

            var driver = new EdgeDriver();
            driver.Navigate().GoToUrl(url);

            await Task.Delay(15000);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            ReadOnlyCollection<IWebElement> contacts = null;
            try
            {
                contacts = wait.Until(
                    ExpectedConditions.VisibilityOfAllElementsLocatedBy(
                        By.CssSelector(".sidebar-views-contacts ul li")));
            }
            catch(Exception ex)
            {
                // ignored
            }

            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    IWebElement span = null;
                    try
                    {
                        span = contact.FindElement(By.CssSelector("span"));
                    }
                    catch(Exception ex)
                    {
                        // ignored
                    }

                    if (span == null)
                    {
                        continue;
                    }

                    var text = span.Text;

                    if (text == null || text.Contains("..."))
                    {
                        continue;
                    }

                    switch (text)
                    {
                        case { } email when email.Contains("@"):
                            organization.Email = email;
                            break;

                        case { } phone:
                            organization.Phone = phone;
                            break;
                    }
                }
            }

            

            driver.Quit();

            /*
            var document = await BrowsingContext.New(AngleSharpParserConfig)
                .OpenAsync(url); 
            using (var browser = await Puppeteer.LaunchAsync(PuppeterSharpParserConfig))
            {
                var page = await browser.NewPageAsync();

                await page.GoToAsync(url, new NavigationOptions
                {
                    WaitUntil = new[]
                    {
                        WaitUntilNavigation.Networkidle2,
                    }
                });

                var browserContent = await page.GetContentAsync();
                var parser = new HtmlParser();
                var document = parser.ParseDocument(browserContent);

                var contacts = document.QuerySelectorAll(".sidebar-views-contacts ul li").ToList();
                foreach (var contact in contacts)
                {
                    var span = contact.QuerySelector("span");
                    if (span == null)
                    {
                        continue;
                    }

                    var text = span.TextContent;
                    switch (text)
                    {
                        case { } email when email.Contains("@"):
                            organization.Email = email;
                            break;

                        case { } phone:
                            organization.Phone = phone;
                            break;
                    }
                }
            }*/

            /*var personContacts = document.QuerySelectorAll(".widget-companies-agents ul li").ToList();
            foreach (var htmlContact in personContacts)
            {
                var contact = new ContactEntity();
                var span = htmlContact.QuerySelector("span");
                if (span != null)
                {
                    contact.Position = span.TextContent.NormolizeText();
                }

                var a = htmlContact.QuerySelector("a");
                if (a != null)
                {
                    contact.Name = a.TextContent.NormolizeText();
                    contact.Url = a.GetAttribute("href");
                }

                organization.Contacts.Add(contact);
            }

            // get organization description
            var description = document.QuerySelector(".widget-companies-description .description .text");
            if (description != null)
            {
                organization.Description = description.TextContent.NormolizeText();
            }


            var howManyTimesWasViewed = document.QuerySelector(".info-company-panel span");
            if (howManyTimesWasViewed != null)
            {
                // get only numbers from string
                var number = new string(howManyTimesWasViewed.TextContent.Where(char.IsDigit).ToArray());
                organization.HowManyTimesWasViewedByPeople = int.Parse(number);
            }*/
        }

        static async Task<List<OrganizationEntity>> FillOrganizations()
        {
            var organizationsCount = await Storage.Organizations.CountAsync();
            if (organizationsCount < 100)
            {
                var companyCollection = await ParseCompanies(AngleSharpParserConfig, FileName);
                await Storage.Organizations.AddRangeAsync(companyCollection);
                await Storage.SaveChangesAsync();
                

                return companyCollection;
            }
            else
            {
                return await Storage.Organizations.Include(x => x.Contacts).ToListAsync();
            }
        }

        static async Task FillContactInformation()
        {
            var count = 0;
            var contacts = await Storage.Contacts.Where(e => e.Email == null).ToListAsync();

            foreach (var contact in contacts)
            {
                Console.WriteLine(count++ + " contact");

                if (contact.Url == null)
                {
                    continue;
                }

                using (var browser = await Puppeteer.LaunchAsync(PuppeterSharpParserConfig))
                {
                    var page = await browser.NewPageAsync();

                    await page.GoToAsync(contact.Url,
                        new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

                    var browserContent = await page.GetContentAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(browserContent);

                    var htmlProfile = document.QuerySelectorAll(".profile__container").Skip(1).FirstOrDefault()
                        ?.QuerySelectorAll(".island__item").Skip(1).FirstOrDefault();
                    var htmlMail = htmlProfile?.QuerySelector("a")?.TextContent;
                    if (htmlMail != null)
                    {
                        contact.Email = htmlMail;
                    }

                    var htmlPhone = htmlProfile?.QuerySelector("div")?.TextContent;
                    if (htmlPhone != null)
                    {
                        contact.Phone = htmlPhone;
                    }
                }

                if (count % 10 == 0)
                {
                    Console.WriteLine("Save");
                    await Storage.SaveChangesAsync();
                }
            }
        }

        private static async Task<List<OrganizationEntity>> ParseCompanies(IConfiguration config, string fileName)
        {
            var baseUrl = BaseUrl;
            var document = await BrowsingContext.New(config).OpenAsync(baseUrl);
            var companyTable = document.QuerySelector("#tablesort");
            var companyRows = companyTable.QuerySelectorAll("tr").Skip(1);
            var companyCollection = new List<OrganizationEntity>();

            foreach (var companyHtmlRow in companyRows)
            {
                // parse this html structure:
                /*<tr class="even">
                        <td data="Finstek" class=" ">
                        <a href="/finstek">Finstek</a>
                        </td>
                    
                        <td class="plus t-left " data="0.0">
                        <strong>
                        0.0
                        </strong>
                        <span class="rating">
                        0.0
                        </span>
                        </td>
                    
                        <td class="t-left " data="100">
                                                 100
                                                 </td>
                    
                        <td data="0" class=" ">
                        <a href="https://jobs.devby.io/?filter%5Bcompanies_ids%5D%5B%5D=76956" style="text-decoration:none">↗</a>
                        </td>
                    
                        <td data="5" class=" ">
                        <a href="/finstek/reviews">5</a>
                        </td>
                    
                    </tr>*/

                var name = companyHtmlRow.QuerySelector("td:nth-child(1) a").TextContent;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var subLink = companyHtmlRow.QuerySelector("td:nth-child(1) a").GetAttribute("href");
                var rating = companyHtmlRow.QuerySelector("td:nth-child(2) strong").TextContent.NormolizeText();
                var employeesNumber = companyHtmlRow.QuerySelector("td:nth-child(3)").TextContent.NormolizeText();
                var reviewNumber = companyHtmlRow.QuerySelector("td:nth-child(5) a")?.TextContent ?? "0";

                var company = new OrganizationEntity()
                {
                    Name = name,
                    SubLink = subLink,
                    Rating = decimal.Parse(rating.Replace('.', ',')),
                    EmployeesNumber = int.Parse(employeesNumber),
                    ReviewNumber = int.Parse(reviewNumber)
                };
                companyCollection.Add(company);
            }

            return companyCollection;
        }
    }
    

    public static class Extensions
    {
        public static string NormolizeText(this string text) => text.Trim('\n', '\r', ' ');
    }
}
