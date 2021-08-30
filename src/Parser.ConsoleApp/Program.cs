using System;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io;
using Parser.BL;
using static System.Console;

namespace Parser.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader(new LoaderOptions{IsNavigationDisabled = false, IsResourceLoadingEnabled = true}).WithCss().WithDefaultCookies().WithJs();

            var document = await BrowsingContext.New(config).OpenAsync("https://tatatravel.by/rezultaty-poiska-turov/?ct=1863&co=12&alr=0&nf=9&nt=10&df=23.10.2021&dt=02.11.2021&ti=-1&ad=2&ch=0&cur=1&resolution=big&re=any&sp=1");
            WriteLine(document.ToHtml());
            foreach (var element in document.QuerySelectorAll(".search-results"))
            {
                
            }
        }
    }
}
