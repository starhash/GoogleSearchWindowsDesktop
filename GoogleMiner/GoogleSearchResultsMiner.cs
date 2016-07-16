using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Windows.Controls;
using System.Windows;

namespace GoogleMiner
{
    public class GoogleSearchResultMiner
    {
        private string _searchkey;
        private List<string> _searchresultslinks;
        private List<string> _searchresults;
        private int _currentpage = 0;
        private bool _safesearch;
        private int _searchoption; //0-web,1-news,2-videos,3-books
        private string[] tbmarr = { "", "nws", "vid", "bks" };

        public string SearchKey { get { return _searchkey; } }
        public string NextPageLink { get { return "http://google.com/search?q=" + _searchkey + "&start=" + (_currentpage+1) * 10; } }
        public string PreviousPageLink { get { if (_currentpage == 0) _currentpage = 1; return "http://google.com/search?q=" + _searchkey + "&start=" + (_currentpage - 1) * 10; } }
        public List<string> SearchResultsLinks { get { return _searchresultslinks; } }
        public List<string> SearchResults { get { return _searchresults; } }
        public int ResultCount
        {
            get
            {
                if (SearchResultsLinks == null) return 0;
                return SearchResultsLinks.Count;
            }
        }
        public bool SafeSearch
        {
            get { return _safesearch; }
            set { _safesearch = value; }
        }
        public int SearchOptions
        {
            get { return _searchoption; }
            set { _searchoption = value; }
        }
        public string CurrentURL
        {
            get { return FormURL(); }
        }
        public int CurrentPage { get { return _currentpage; } }

        public GoogleSearchResultMiner(string key)
        {
            _searchkey = key;
            _searchresults = new List<string>();
            _searchresultslinks = new List<string>();
            _currentpage = 0;
        }

        public void InitializeAndLoad()
        {
            LoadURL(FormURL());
        }
        
        protected void LoadURL(string url)
        {
            _searchresults.Clear();
            _searchresultslinks.Clear();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            List<HtmlNode> searchs = doc.DocumentNode.Descendants("div").ToList();
            string rescounts = doc.DocumentNode.SelectNodes("/html[1]/body[1]/table[1]/tbody[1]/tr[1]/td[2]/div[1]/div[1]")[0].InnerHtml.Replace("About", "").Replace("results", "").Replace(",", "").Trim();
            List<HtmlNode> ufrnodes = doc.DocumentNode.SelectNodes("/html[1]/body[1]/table[1]/tbody[1]/tr[1]/td[2]/div[1]/div[2]/div[2]/div[1]/ol[1]/div").ToList();
            List<HtmlNode> rnodes = ufrnodes.Where((n) => n.ChildNodes.Count == 2)
                .Where((n) => n.ChildNodes[0].Name == "h3" && n.ChildNodes[1].Name == "div").ToList();
            for(int i = 0; i < rnodes.Count; i++)
            {
                HtmlNode a = doc.DocumentNode.SelectSingleNode(rnodes[i].XPath + "/h3[1]/a[1]");
                string label = a.InnerText.Replace("&amp;", "&");
                string link = a.Attributes["href"].Value;
                if (link.StartsWith("/url?q=") && link.Contains("&amp;sa="))
                {
                    link = link.Substring(7, link.IndexOf("&amp;sa=") - 7);
                }
                _searchresults.Add(link);
                _searchresultslinks.Add(link);
            }
        }

        public void NextPage()
        {
            _currentpage++;
            LoadURL(FormURL());
        }

        public void PrevPage()
        {
            _currentpage--;
            LoadURL(FormURL());
        }

        private string FormURL()
        {
            return "http://google.com/search?q=" + _searchkey + "&start=" + (_currentpage * 10)
                + ((_safesearch) ? "&safe=on" : "&safe=off")
                 + ((_searchoption == 0) ? "" : "&tbm=" + tbmarr[_searchoption]);
        }
    }
}
