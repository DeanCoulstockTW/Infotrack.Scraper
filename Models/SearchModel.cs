namespace OnlineTitleSearch.Models
{
    public class SearchModel
    {
        public enum SearchEngine
        {
            Google,
        }
        public string SearchTerm { get; set; }
        
        public bool GoogleSelected = true; // limit user to use google until Bing logic is implemented
    }
}