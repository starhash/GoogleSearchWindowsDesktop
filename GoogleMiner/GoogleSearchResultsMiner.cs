using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

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
        public string NextPageLink { get { return "http://www.google.com/#q=" + _searchkey + "&start=" + (_currentpage+1) * 10; } }
        public string PreviousPageLink { get { if (_currentpage == 0) _currentpage = 1; return "http://www.google.com/#q=" + _searchkey + "&start=" + (_currentpage - 1) * 10; } }
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
            List<HtmlNode> searchs = doc.DocumentNode.Descendants("div").ToList<HtmlNode>();
            List<HtmlNode> searchs2 = doc.DocumentNode.Descendants("table").ToList<HtmlNode>();
            #region SEARCH
            HtmlNode searchnode = null;
            foreach (HtmlNode h in searchs)
            {
                foreach (HtmlAttribute ha in h.Attributes)
                {
                    if (ha.Name == "id")
                    {
                        if (ha.Value == "search")
                        {
                            searchnode = h;
                            break;
                        }
                    }
                }
                if (searchnode != null)
                    break;
            }
            #endregion
            #region main search
            if (searchnode != null)
            {
                searchs = searchnode.Descendants("li").ToList<HtmlNode>();
                List<HtmlNode> searchresults = new List<HtmlNode>();
                foreach (HtmlNode h in searchs)
                {
                    foreach (HtmlAttribute ha in h.Attributes)
                    {
                        if (ha.Name == "class")
                        {
                            if (ha.Value == "g")
                            {
                                searchresults.Add(h);
                                break;
                            }
                        }
                    }
                }
                foreach (HtmlNode result in searchresults)
                {
                    List<HtmlNode> linkfinder = result.Descendants("h3").ToList<HtmlNode>();
                    List<HtmlNode> linkfoundchildren = new List<HtmlNode>();
                    foreach (HtmlNode linkprop in linkfinder)
                    {
                        if (linkprop.ChildNodes.Count == 1)
                            linkfoundchildren.Add(linkprop.ChildNodes[0]);
                    }
                    linkfinder.Clear();
                    foreach (HtmlNode linkprop in linkfoundchildren)
                    {
                        foreach (HtmlAttribute ha in linkprop.Attributes)
                        {
                            if (ha.Name == "href")
                            {
                                string link = ha.Value;
                                if (link.StartsWith("/url?q="))
                                {
                                    int findq = link.IndexOf("/url?q=");
                                    link = link.Substring(findq + 7);
                                    link = link.Substring(0, link.IndexOf("&amp;sa="));
                                }
                                else if (link.StartsWith("/search"))
                                {
                                    link = "http://www.google.com" + link;
                                }
                                _searchresultslinks.Add(link);
                            }
                        }
                        _searchresults.Add(linkprop.InnerHtml);
                    }
                }
            }
            #endregion
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
            return "http://www.google.com/#q=" + _searchkey + "&start=" + (_currentpage * 10)
                + ((_safesearch) ? "&safe=on" : "&safe=off")
                 + ((_searchoption == 0) ? "" : "&tbm=" + tbmarr[_searchoption]);
        }
    }
}
