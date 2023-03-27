using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Newtonsoft.Json;
using Parser.BL;
using static System.Console;

namespace Parser.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CompaniesDevBy.RunLogic();
        }

        
    }
    
}
