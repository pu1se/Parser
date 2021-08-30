using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace Parser.BL
{
    public class TataTravelParser
    {
        public void GetBestPrice()
        {
            var html = @"
  <html>
      <head></head>
      <body>
        <div>
          <p class='content'>Fizzler</p>
          <p>CSS Selector Engine</p></div>
      </body>
  </html>";
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);

            foreach (var element in document.QuerySelectorAll("p"))
            {
            }
        }
    }
}
