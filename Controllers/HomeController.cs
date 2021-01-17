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
            System.IO.File.Delete("numberedResults.txt"); // reset for a fresh file every run

            var searchTerm = searchModel.SearchTerm;
            
            string pagesToScrape = null;
            int iteration = 0; // track how many pages have been crawled, so that results are numbered correctly
            
            pagesToScrape = CurateSearchEngine(pagesToScrape);
            string[] staticPages = System.IO.File.ReadAllLines(pagesToScrape ?? throw new FileLoadException());
            
            foreach (string uri in staticPages) // perform scraping operation on all available static webpages
            {
                FindResults(uri, iteration, searchTerm);
                iteration += 8; 
            }

            return View("Results", Results());

            string CurateSearchEngine(string s)
            {
                s = searchModel.GoogleSelected switch
                {
                    true => "staticGooglePages.txt",
                    _ => s
                };

                return s;
            }
        }
        public static string FindResults(string searchUrl, int iteration, string searchTerm) // this calls the pages, parses the html, and returns all the results we're interested in 
        {
            string response = CallUrl(searchUrl).Result;
            IEnumerable<string> linkList = ParseHtml(response);
            List<int> finalValueList = ReturnResults(linkList, iteration, searchTerm);
            
            WriteToTxt(finalValueList);
            
            return response;
        }
        public static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            
            client.DefaultRequestHeaders.Accept.Clear();
            
            Task<string> response = client.GetStringAsync(fullUrl);
            
            return await response;
        }
        
        public static IEnumerable<string> ParseHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            
                List<HtmlNode> resultLinks = htmlDoc.DocumentNode.Descendants("div")
                    .Where(node => 
                        node.GetAttributeValue("class", "").Contains("r")).ToList();

                return (from link in resultLinks where link.FirstChild.Attributes.Contains("href") select link.FirstChild.Attributes[0].Value).ToList();

        }

        public static List<int> ReturnResults(IEnumerable<string> links, int iteration, string searchTerm)
        {
            List<int> stringOfNumberedResults = new List<int>();
            int iterator = 1;
            
            foreach (string link in links)
            {
                switch (link.Contains(searchTerm))
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

        public static void WriteToTxt(List<int> positions)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            StringBuilder sb = new StringBuilder();
            
            for (int index = 0; index < positions.Count; index++)
            {
                int position = positions[index];
                switch (position)
                {
                    case <= 50:
                        sb.AppendLine(position.ToString() + ".. ");
                        break;
                }
            }

            System.IO.File.AppendAllText("numberedResults.txt",   sb.ToString());
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