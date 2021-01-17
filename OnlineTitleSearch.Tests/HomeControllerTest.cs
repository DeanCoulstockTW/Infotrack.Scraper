using System.Collections.Generic;
using System.IO;
using Xunit;

namespace OnlineTitleSearch.OnlineTitleSearch.Tests
{
    public class HomeController
    {
        [Fact]
        public void CallUrl_Returns_Blob_Correctly()
        {
            string searchUrl = "https://infotrack-tests.infotrack.com.au/Google/Page01.html";
            string response = Controllers.HomeController.CallUrl(searchUrl).Result;

            Assert.NotEmpty(response);
            Assert.Contains("Google", response);
            Assert.DoesNotContain("Bing", response);


            searchUrl = "https://infotrack-tests.infotrack.com.au/Bing/Page01.html";
            response = Controllers.HomeController.CallUrl(searchUrl).Result;

            Assert.NotEmpty(response);
            Assert.DoesNotContain("Google", response);
            Assert.Contains("Bing", response);
        }

        [Fact]
        public void ParseHtml_Parses_Html_Correctly()
        {
            const string searchUrl = "https://infotrack-tests.infotrack.com.au/Google/Page01.html";
            const string expectedResult = "https://www.infotrack.com.au/solutions/searches-certificates/";

            string response = Controllers.HomeController.CallUrl(searchUrl).Result;
            IEnumerable<string> result = Controllers.HomeController.ParseHtml(response);

            Assert.Contains(expectedResult, result);
        }

        [Fact]
        public void ReturnResults_Returns_Results_Correctly()
        {
            const string searchUrl = "https://infotrack-tests.infotrack.com.au/Google/Page01.html";
            string searchTerm = "infotrack";
            
            string response = Controllers.HomeController.CallUrl(searchUrl).Result;
            IEnumerable<string> result = Controllers.HomeController.ParseHtml(response);
            List<int> finalValueList = Controllers.HomeController.ReturnResults(result, 0, searchTerm);

            Assert.NotEmpty(finalValueList);
            Assert.True((finalValueList[0].Equals(1)) && (finalValueList[1].Equals(8)));
        }

        [Fact]
        public void WriteToTxt_Writes_Correct_Information_To_File()
        {
            File.Delete("numberedResults.txt"); // reset for a fresh file every run

            const string searchUrl = "https://infotrack-tests.infotrack.com.au/Google/Page01.html";
            string searchTerm = "infotrack";

            string response = Controllers.HomeController.CallUrl(searchUrl).Result;
            IEnumerable<string> result = Controllers.HomeController.ParseHtml(response);
            List<int> finalValueList = Controllers.HomeController.ReturnResults(result, 0, searchTerm);

            Controllers.HomeController.WriteToTxt(finalValueList);

            Assert.True(File.Exists("numberedResults.txt")); // assert file now exists
        }
    }
}