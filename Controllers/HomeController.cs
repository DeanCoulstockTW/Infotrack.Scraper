using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlineTitleSearch.Models;
namespace OnlineTitleSearch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        [HttpPost]
        public ActionResult Index(SearchModel searchModel)
        {
            string pages = null;
            System.IO.File.Delete("numberedResults.txt"); // reset for a fresh file every run
            if (searchModel.GoogleSelected)
            {
                pages = "staticGooglePages.txt";
            }
            var iteration = 0; // track how many pages have been crawled, so that results are numbered correctly
            var staticPages = System.IO.File.ReadAllLines(pages ?? throw new FileLoadException());
            
            foreach (var uri in staticPages) // perform scraping operation on all available static webpages
            {
                FindResults(uri, iteration);
                iteration += 8; 
            }

            return View("Results", Results());
        }
        private static void FindResults(string searchUrl, int iteration) // this calls the pages, parses the html, and returns all the results we're interested in 
        {
            var response = CallUrl(searchUrl).Result;
            var linkList = ParseHtml(response);
            var finalValueList = ReturnResults(linkList, iteration);
            WriteToTxt(finalValueList);
        }
        private static async Task<string> CallUrl(string fullUrl)
        {
            var client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }
        
        private static IEnumerable<string> ParseHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            
                var resultLinks = htmlDoc.DocumentNode.Descendants("div")
                    .Where(node => 
                        node.GetAttributeValue("class", "").Contains("r")).ToList();

                return (from link in resultLinks where link.FirstChild.Attributes.Contains("href") select link.FirstChild.Attributes[0].Value).ToList();

        }
        private static void WriteToTxt(List<int> links)
        {
            if (links == null) throw new ArgumentNullException(nameof(links));
            var sb = new StringBuilder();
            foreach (var link in links)
            {
                sb.AppendLine(link.ToString() + ".. ");
            }

            System.IO.File.AppendAllText("numberedResults.txt",   sb.ToString());
        }

        private static List<int> ReturnResults(IEnumerable<string> links, int iteration)
        {
            var stringOfNumberedResults = new List<int>();
            var iterator = 1;
            
            foreach (var link in links)
            {
                switch (link.Contains("infotrack"))
                {
                    case false:
                        iterator += 1;
                        break;
                    default:
                        stringOfNumberedResults.Add(iterator + iteration);
                        iterator += 1;
                        break;
                }
            }
            return stringOfNumberedResults;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Results()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}