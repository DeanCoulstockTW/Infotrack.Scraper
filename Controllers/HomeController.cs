using System.Collections.Generic;
using System.Diagnostics;
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
            System.IO.File.Delete("numberedResults.csv");
            string[] staticPages = System.IO.File.ReadAllLines("staticPages.txt");
            foreach (string URI in staticPages)
            {
                FindResults(URI);
            }
            
            return View();
        }

        private void FindResults(string searchUrl)
        {
            var response = CallUrl(searchUrl).Result;
            var linkList = ParseHtml(response);
            var finalValueList = ReturnResults(linkList);
            WriteToCsv(finalValueList);
        }
        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }
        
        private List<string> ParseHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var resultLinks = htmlDoc.DocumentNode.Descendants("div")
                .Where(node => 
                    node.GetAttributeValue("class", "").Contains("r")).ToList();
                
            List<string> formattedLink = new List<string>();

            foreach (var link in resultLinks)
            {
                if (link.FirstChild.Attributes.Contains("href"))
                    formattedLink.Add(link.FirstChild.Attributes[0].Value);
            }

            return formattedLink;

        }
        private void WriteToCsv(List<int> links)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var link in links)
            {
                sb.Append(link.ToString() + ",");
            }

            System.IO.File.AppendAllText("numberedResults.csv", sb.ToString().TrimEnd(','));
        }

        private List<int> ReturnResults(List<string> links)
        {
            List<int> stringOfNumberedResults = new List<int>();
            var iterator = 1;
            
            foreach (var link in links)
            {
                if (!link.Contains("infotrack"))
                {
                    iterator += 1;
                }
                else
                {
                    stringOfNumberedResults.Add(iterator);
                    iterator += 1;
                }
            }

            return stringOfNumberedResults;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
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