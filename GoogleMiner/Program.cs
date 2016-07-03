using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace GoogleMiner
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Do you want to use Google search Miner? (Enter)");
            ConsoleKey inputkey= Console.ReadKey().Key;
            int page = -1;
            while (inputkey == ConsoleKey.Enter)
            {
                Console.WriteLine("Search For? ");
                string searchfor = Console.ReadLine();
                GoogleSearchResultMiner gsrm = new GoogleSearchResultMiner(searchfor);
                Console.WriteLine(gsrm.NextPageLink);
                Console.WriteLine("Showing page " + page + ".");
                Console.WriteLine("Left/Right to browse Enter to open in browser.");
                ConsoleKey traverse = Console.ReadKey().Key;
                int idx = -1;
                string browselink = "";
                while (traverse == ConsoleKey.RightArrow || traverse == ConsoleKey.LeftArrow
                    || traverse == ConsoleKey.Enter)
                {
                    if (traverse == ConsoleKey.RightArrow)
                        idx++;
                    else if (traverse == ConsoleKey.LeftArrow)
                        idx--;
                    if (idx >= gsrm.SearchResultsLinks.Count)
                    {
                        idx = gsrm.SearchResultsLinks.Count - 1;
                    }
                    if (idx < 0) idx = 0;
                    browselink = gsrm.SearchResultsLinks[idx];
                    Console.Write("\r{0}", browselink);
                    Clipboard.SetText(browselink);
                    traverse = Console.ReadKey().Key;
                    if (traverse == ConsoleKey.Enter)
                    {
                        Process process = new Process();
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.FileName = browselink;
                        process.Start();
                    }
                }
                if (traverse == ConsoleKey.Escape)
                {
                    Console.WriteLine("Previous search erased...");
                }
                Console.WriteLine("\ninSearch more? (Enter)");
                inputkey = Console.ReadKey().Key;
            }
        }
    }

    
}
