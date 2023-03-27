using AngleSharp.Io;
using AngleSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;

namespace Parser.ConsoleApp
{
    public  static class CompaniesDevBy
    {
        const string BaseUrl = "https://companies.devby.io";
        const string FileName = "companies.json";
        static IConfiguration ParserConfig = AngleSharp.Configuration.Default.WithDefaultLoader(new LoaderOptions{IsNavigationDisabled = false, IsResourceLoadingEnabled = true}).WithCss().WithDefaultCookies().WithJs().WithRenderDevice();

        public static async Task RunLogic()
        {
            var companies = await GetCompanies();

            foreach (var company in companies)
            {
                await FillContactInformation(company);
            }
        }

        static async Task FillAdditionalInformationAboutCompany(Company company)
        {
            var document = await BrowsingContext.New(ParserConfig)
                .OpenAsync(BaseUrl + company.SubLink);
            
            /*var contacts = document.QuerySelectorAll(".sidebar-views-contacts ul li").ToList();
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
                        company.Email = email;
                        break;

                    case { } phone:
                        company.Phone = phone;
                        break;
                }
            }*/
            var contacts = document.QuerySelectorAll(".widget-companies-agents ul li").ToList();
            foreach (var htmlContact in contacts)
            {
                var contact = new Contact();
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

                company.Contacts.Add(contact);
            }

            // get company description
            var description = document.QuerySelector(".widget-companies-description .description .text");
            if (description != null)
            {
                company.Description = description.TextContent.NormolizeText();
            }


            var howManyTimesWasViewed = document.QuerySelector(".info-company-panel span");
            if (howManyTimesWasViewed != null)
            {
                // get only numbers from string
                var number = new string(howManyTimesWasViewed.TextContent.Where(char.IsDigit).ToArray());
                company.HowManyTimesWasViewedByPeople = int.Parse(number);
            }
        }

        static async Task<List<Company>> GetCompanies()
        {
            

            if (!System.IO.File.Exists(FileName))
            {
                var companyCollection = await ParseCompanies(ParserConfig, FileName);

                var counter = 0;
                foreach (var company in companyCollection)
                {
                    Console.WriteLine(++counter);
                    await FillAdditionalInformationAboutCompany(company);
                }

                // save to file as json
                var json = JsonConvert.SerializeObject(companyCollection);
                await System.IO.File.WriteAllTextAsync(FileName, json);
                
                return companyCollection;
            }
            else
            {
                var json = await System.IO.File.ReadAllTextAsync(FileName);
                var companyCollection = JsonConvert.DeserializeObject<List<Company>>(json);
                return companyCollection;
            }
        }

        static async Task FillContactInformation(Company company)
        {
            foreach (var contact in company.Contacts)
            {
                if (contact.Url == null)
                {
                    continue;
                }

                var document = await BrowsingContext.New(ParserConfig)
                    .OpenAsync(BaseUrl + contact.Url);

                var htmlProfile = document.QuerySelector(".profile__container .island__item:last-child");
                var htmlMail = htmlProfile.QuerySelector("a")?.TextContent;
                if (htmlMail != null)
                {
                    contact.Email = htmlMail;
                }

                var htmlPhone = htmlProfile.QuerySelector("div")?.TextContent;
                if (htmlPhone != null)
                {
                    contact.Phone = htmlPhone;
                }
            }
        }

        private static async Task<List<Company>> ParseCompanies(IConfiguration config, string fileName)
        {
            var baseUrl = BaseUrl;
            var document = await BrowsingContext.New(config).OpenAsync(baseUrl);
            var companyTable = document.QuerySelector("#tablesort");
            var companyRows = companyTable.QuerySelectorAll("tr").Skip(1);
            var companyCollection = new List<Company>();

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

                var company = new Company
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

    public class Company
    {
        public string Name { get; set; }
        public string SubLink { get; set; }
        public decimal Rating { get; set; }
        public int EmployeesNumber { get; set; }
        public int ReviewNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ContactInfoNotParsed { get; set; }
        public int HowManyTimesWasViewedByPeople { get; set; }
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        public string Description { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Url { get; set; }
    }

    public static class Extensions
    {
        public static string NormolizeText(this string text) => text.Trim('\n', '\r', ' ');
    }
}
